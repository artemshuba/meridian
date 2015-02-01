using System;
using System.Collections.Generic;
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
using Neptune.Extensions;
using VkLib.Core.Groups;

namespace Meridian.ViewModel.Main
{
    public class FeedViewModel : ViewModelBase
    {
        private ObservableCollection<VkGroup> _societies = new ObservableCollection<VkGroup>();
        private ObservableCollection<Audio> _tracks;
        private const int MAX_NEWS_AUDIOS = 100;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private VkGroup _selectedSociety;

        #region Commands

        /// <summary>
        /// Add society to feed command
        /// </summary>
        public RelayCommand AddSocietyCommand { get; private set; }

        /// <summary>
        /// Remove society from feed command
        /// </summary>
        public RelayCommand<VkGroup> RemoveSocietyCommand { get; private set; }

        /// <summary>
        /// Play audio command
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

        public VkGroup SelectedSociety
        {
            get { return _selectedSociety; }
            set
            {
                if (Set(ref _selectedSociety, value))
                {
                    CancelAsync();

                    LoadFeed(_cancellationToken.Token);
                }
            }
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

                    if (Societies.Count == 0)
                    {
                        _societies.Add(new VkGroup() { Name = MainResources.AllSocieties });
                    }

                    Societies.Add((VkGroup)result);
                    SaveSocieties();

                    if (SelectedSociety == null)
                        SelectedSociety = _societies.First();
                    else if (SelectedSociety.Id == 0)
                        LoadFeed(_cancellationToken.Token);
                }
            });

            RemoveSocietyCommand = new RelayCommand<VkGroup>(society =>
            {
                bool isActiveSociety = false;
                if (society == SelectedSociety)
                    isActiveSociety = true;

                CancelAsync();
                Societies.Remove(society);

                if (Societies.Count == 1)
                    Societies.Clear();

                SaveSocieties();

                if (!isActiveSociety && (SelectedSociety != null && SelectedSociety.Id != 0))
                {
                    return;
                }

                if (isActiveSociety)
                {
                    if (Societies.Any())
                        SelectedSociety = Societies.First();
                }

                //if (SelectedSociety != null && SelectedSociety.Id == 0)
                //{
                    CancelAsync();

                    LoadFeed(_cancellationToken.Token);
                //}

                //if (isActiveSociety && Societies.Any())
                //    SelectedSociety = Societies.First();
                //else if (isActiveSociety)
                //    LoadFeed(_cancellationToken.Token);
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

                if (_societies.Count > 0)
                {
                    _societies.Insert(0, new VkGroup() { Name = MainResources.AllSocieties });
                    SelectedSociety = _societies.First();
                }

                LoadFeed(_cancellationToken.Token);
            }
        }

        private void SaveSocieties()
        {
            Domain.Settings.Instance.FeedSocieties = _societies.Skip(1).ToList();
        }

        private async void LoadFeed(CancellationToken token)
        {
            if (Societies.Count == 0)
            {
                Tracks = null;

                OnTaskError("feed", ErrorResources.FeedSocietiesEmpty);
                return;
            }

            if (SelectedSociety == null)
                return;

            OnTaskStarted("feed");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                var nextFrom = string.Empty;
                int count = 150;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_NEWS_AUDIOS)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    var sourceIds = SelectedSociety.Id != 0
                        ? new List<long>() { -SelectedSociety.Id }
                        : Societies.Skip(1).Select(s => -s.Id).ToList();

                    var a = await DataService.GetNewsAudio(count, nextFrom, token, sourceIds);
                    if (a.Items.IsNullOrEmpty())
                        break;
                    else if (a.Items.Count > 0)
                    {
                        OnTaskFinished("feed");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    nextFrom = a.NextFrom;

                    foreach (var audio in a.Items)
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
