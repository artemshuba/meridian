using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Flyouts;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using VkLib.Core.Groups;

namespace Meridian.ViewModel.Main
{
    public class FeedViewModel : ViewModelBase
    {
        private ObservableCollection<VkGroup> _societies = new ObservableCollection<VkGroup>();
        private ObservableCollection<Audio> _tracks;
        private const int MAX_NEWS_AUDIOS = 100;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        #region Commands

        public RelayCommand AddSocietyCommand { get; private set; }

        /// <summary>
        /// Команда воспроизведения аудиозаписи
        /// </summary>
        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        #endregion

        public ObservableCollection<VkGroup> Societies
        {
            get { return _societies; }
            set { Set(ref _societies, value); }
        }

        public ObservableCollection<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public FeedViewModel()
        {
            RegisterTasks("feed");

            InitializeCommands();

            LoadSocieties();
        }

        private void InitializeCommands()
        {
            AddSocietyCommand = new RelayCommand(async () =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new AddSocietyFlyout();
                var result = await flyout.ShowAsync();
                if (result != null)
                {
                    CancelAsync();

                    Societies.Add((VkGroup)result);
                    SaveSocieties();
                    LoadFeed(_cancellationToken.Token);
                }
            });

            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });
        }

        private void LoadSocieties()
        {
            if (Domain.Settings.Instance.FeedSocieties != null)
            {
                _societies = new ObservableCollection<VkGroup>(Domain.Settings.Instance.FeedSocieties);

                if (_societies.Any())
                    LoadFeed(_cancellationToken.Token);
            }
        }

        private void SaveSocieties()
        {
            Domain.Settings.Instance.FeedSocieties = _societies.ToList();
        }

        private async void LoadFeed(CancellationToken token)
        {
            OnTaskStarted("feed");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_NEWS_AUDIOS)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    var a = await DataService.GetNewsAudio(count, offset, token, Societies.Select(s => -s.Id).ToList());
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("feed");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    offset += count;

                    foreach (var audio in a)
                    {
                        Tracks.Add(audio);
                    }

                    requestsCount++;

                    if (requestsCount >= 2) //не больше 2-х запросов в секунду
                    {
                        requestsCount = 0;
                        await Task.Delay(1000);
                    }

                    Debug.WriteLine("Loading more audios from news");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("feed", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                OnTaskError("feed", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("feed");
        }

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }

    }
}
