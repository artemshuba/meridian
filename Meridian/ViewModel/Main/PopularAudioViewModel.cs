using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using VkLib.Core.Audio;
using VkAudio = Meridian.Model.VkAudio;

namespace Meridian.ViewModel.Main
{
    public class PopularAudioViewModel : ViewModelBase
    {
        private List<VkAudio> _tracks;
        private List<VkGenre> _genres;
        private VkGenre _selectedGenre;
        private bool _foreignOnly;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        #endregion

        public List<VkAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public List<VkGenre> Genres
        {
            get { return _genres; }
            set { Set(ref _genres, value); }
        }

        public VkGenre SelectedGenre
        {
            get { return _selectedGenre; }
            set
            {
                if (Set(ref _selectedGenre, value))
                {
                    CancelAsync();

                    LoadTracks(_cancellationToken.Token);
                }
            }
        }

        public bool ForeignOnly
        {
            get { return _foreignOnly; }
            set
            {
                if (Set(ref _foreignOnly, value))
                {
                    CancelAsync();

                    LoadTracks(_cancellationToken.Token);
                }
            }
        }

        public PopularAudioViewModel()
        {
            _genres = ViewModelLocator.Vkontakte.Audio.GetGenres();
            _genres.Insert(0, new VkGenre() { Title = MainResources.PopularGenresAll });

            _selectedGenre = _genres.First();

            RegisterTasks("audio");

            InitializeCommands();
        }

        public override async void Activate()
        {
            if (Tracks == null || Tracks.Count == 0)
                await LoadTracks(_cancellationToken.Token);
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            RefreshCommand = new RelayCommand(() =>
            {
                CancelAsync();
                LoadTracks(_cancellationToken.Token);
            });
        }

        private async Task LoadTracks(CancellationToken token)
        {
            OnTaskStarted("audio");

            try
            {
                var response = await DataService.GetPopularTracks(SelectedGenre.Id, ForeignOnly);
                if (!token.IsCancellationRequested)
                {
                    if (response.Items != null && response.Items.Count > 0)
                    {
                        Tracks = response.Items;

                        if (AudioService.CurrentAudio == null)
                        {
                            AudioService.CurrentAudio = Tracks.First();
                            AudioService.SetCurrentPlaylist(Tracks);
                        }
                    }
                    else
                    {
                        OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("audio");
        }

        private void CancelAsync()
        {
            _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }
    }
}
