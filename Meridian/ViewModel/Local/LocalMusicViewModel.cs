using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.View.Flyouts.Local;
using Neptune.Messages;
using Xbox.Music;

namespace Meridian.ViewModel.Local
{
    public class LocalMusicViewModel : ViewModelBase
    {
        private List<LocalAudio> _tracks;
        private List<AudioAlbum> _albums;
        private List<AudioArtist> _albumGroups;
        private List<AudioArtist> _artists;
        private double _progress;
        private int _selectedTabIndex;

        #region Commands

        /// <summary>
        /// Play audio command
        /// </summary>
        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        /// <summary>
        /// Go to album command
        /// </summary>
        public RelayCommand<AudioAlbum> GoToAlbumCommand { get; private set; }

        #endregion

        public List<LocalAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public List<AudioAlbum> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        public List<AudioArtist> AlbumGroups
        {
            get { return _albumGroups; }
            set { Set(ref _albumGroups, value); }
        }

        public List<AudioArtist> Artists
        {
            get { return _artists; }
            set { Set(ref _artists, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(ref _selectedTabIndex, value); }
        }

        public LocalMusicViewModel()
        {
            InitializeCommands();

            RegisterTasks("tracks", "albums", "artists");

            Load();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            GoToAlbumCommand = new RelayCommand<AudioAlbum>(album =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/Local.LocalAlbumView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"album", album}
                    }
                });
            });
        }

        private async void Load()
        {
            await LoadTracks();

            if (Tracks == null || Tracks.Count == 0)
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new MusicScanView();
                await flyout.ShowAsync();

                await LoadTracks();
            }

            await LoadAlbums();
            await LoadArtists();
        }

        private async Task LoadTracks()
        {
            OnTaskStarted("tracks");

            try
            {
                Tracks = await ServiceLocator.LocalMusicService.GetTracks();
            }
            catch (Exception ex)
            {
                OnTaskError("tracks", "~Unable to load tracks");

                LoggingService.Log(ex);
            }

            OnTaskFinished("tracks");
        }

        private async Task LoadAlbums()
        {
            OnTaskStarted("albums");

            try
            {
                var albums = await ServiceLocator.LocalMusicService.GetAlbums();
                AlbumGroups = albums.GroupBy(a => a.Artist).Select(g => new AudioArtist() { Title = g.Key, Albums = g.OrderBy(a => a.Year).ToList() }).OrderBy(a => a.Title).ToList();
                //Albums = await ServiceLocator.LocalMusicService.GetAlbums();
            }
            catch (Exception ex)
            {
                OnTaskError("albums", "~Unable to load albums");

                LoggingService.Log(ex);
            }

            OnTaskFinished("albums");
        }

        private async Task LoadArtists()
        {
            OnTaskStarted("artists");

            try
            {
                Artists = await ServiceLocator.LocalMusicService.GetArtists();
            }
            catch (Exception ex)
            {
                OnTaskError("artists", "~Unable to load artists");

                LoggingService.Log(ex);
            }

            OnTaskFinished("artists");
        }
    }
}
