using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View.Flyouts.Local;
using Neptune.Extensions;
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
        private AudioArtist _selectedArtist;
        private List<AudioAlbum> _selectedArtistAlbums;
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

        /// <summary>
        /// Refresh command
        /// </summary>
        public RelayCommand RefreshCommand { get; private set; }

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

        public AudioArtist SelectedArtist
        {
            get { return _selectedArtist; }
            set
            {
                if (Set(ref _selectedArtist, value))
                    LoadSelectedArtist();
            }
        }

        public List<AudioAlbum> SelectedArtistAlbums
        {
            get { return _selectedArtistAlbums; }
            set { Set(ref _selectedArtistAlbums, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (Set(ref _selectedTabIndex, value))
                {
                    switch (value)
                    {
                        case 1:
                            if (Albums.IsNullOrEmpty())
                                LoadAlbums();
                            break;

                        case 2:
                            if (Artists.IsNullOrEmpty())
                            {
                                LoadArtists();
                            }
                            break;
                    }
                }
            }
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

                if (SelectedTabIndex == 0)
                {
                    AudioService.SetCurrentPlaylist(Tracks);
                }
                else if (SelectedArtistAlbums != null)
                {
                    AudioService.SetCurrentPlaylist(SelectedArtistAlbums.Where(a => !a.Tracks.IsNullOrEmpty()).SelectMany(a => a.Tracks).ToList());
                }
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

            RefreshCommand = new RelayCommand(() =>
            {
                Refresh();
            });
        }

        private async void Load()
        {
            await LoadTracks();

            if (Tracks == null || Tracks.Count == 0)
            {
                Refresh();
            }
        }

        private async void Refresh()
        {
            await ServiceLocator.LocalMusicService.Clear();

            if (Tracks != null)
                Tracks.Clear();

            if (Artists != null)
                Artists.Clear();

            if (Albums != null)
                Albums.Clear();

            if (AlbumGroups != null)
                AlbumGroups.Clear();

            if (SelectedArtistAlbums != null)
                SelectedArtistAlbums.Clear();

            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new MusicScanView();
            await flyout.ShowAsync();

            switch (SelectedTabIndex)
            {
                case 0:
                    LoadTracks();
                    break;
                case 1:
                    LoadAlbums();
                    break;

                case 2:
                    LoadArtists();
                    break;
            }
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
                Albums = albums;
            }
            catch (Exception ex)
            {
                OnTaskError("albums", ErrorResources.LoadAlbumsErrorCommon);

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

                if (!Artists.IsNullOrEmpty())
                    SelectedArtist = Artists.FirstOrDefault();
            }
            catch (Exception ex)
            {
                OnTaskError("artists", ErrorResources.LoadArtistsErrorCommon);

                LoggingService.Log(ex);
            }

            OnTaskFinished("artists");
        }

        private async Task LoadSelectedArtist()
        {
            OnTaskStarted("artists");

            try
            {
                if (Albums.IsNullOrEmpty())
                    await LoadAlbums();

                var albums = await ServiceLocator.LocalMusicService.GetArtistAlbums(SelectedArtist.Id);

                if (!albums.IsNullOrEmpty())
                {
                    foreach (var album in albums)
                    {
                        var tracks = await ServiceLocator.LocalMusicService.GetAlbumTracks(album.Id);
                        if (!tracks.IsNullOrEmpty())
                            album.Tracks = tracks.Cast<Audio>().ToList();
                    }
                }

                SelectedArtistAlbums = albums;
            }
            catch (Exception ex)
            {
                OnTaskError("artists", ErrorResources.LoadArtistsErrorCommon);

                LoggingService.Log(ex);
            }

            OnTaskFinished("artists");
        }
    }
}
