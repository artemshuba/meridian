using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using LastFmLib.Core.Album;
using LastFmLib.Core.Artist;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.View.Flyouts;
using Meridian.ViewModel.Flyouts;
using Neptune.Messages;

namespace Meridian.ViewModel.Search
{
    public class ArtistViewModel : ViewModelBase
    {
        private const int MAX_TRACKS_COUNT = 100;

        private LastFmArtist _artist;
        private List<VkAudio> _tracks;
        private List<VkAudio> _allTracks;
        private List<LastFmAlbum> _albums;
        private List<LastFmAlbum> _allAlbums;
        private List<LastFmArtist> _similarArtists;
        private ImageSource _artistImage;
        private List<string> _tags; 

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand ShowAllTracksCommand { get; private set; }

        public RelayCommand ShowAllAlbumsCommand { get; private set; }

        public RelayCommand<LastFmAlbum> GoToAlbumCommand { get; private set; }

        public RelayCommand<LastFmArtist> GoToArtistCommand { get; private set; }

        public RelayCommand ShareCommand { get; private set; }

        #endregion

        public LastFmArtist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<VkAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public List<VkAudio> AllTracks
        {
            get { return _allTracks; }
            set { Set(ref _allTracks, value); }
        }

        public List<LastFmAlbum> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        public List<LastFmAlbum> AllAlbums
        {
            get { return _allAlbums; }
            set { Set(ref _allAlbums, value); }
        }

        public List<LastFmArtist> SimilarArtists
        {
            get { return _similarArtists; }
            set { Set(ref _similarArtists, value); }
        }

        public ImageSource ArtistImage
        {
            get { return _artistImage; }
            set { Set(ref _artistImage, value); }
        }

        public List<string> Tags
        {
            get { return _tags; }
            set
            {
                if (Set(ref _tags, value))
                    RaisePropertyChanged("TagsString");
            }
        }

        public string TagsString
        {
            get
            {
                if (_tags != null)
                    return string.Join(", ", _tags);

                return null;
            }
        }

        public ArtistViewModel()
        {
            InitializeCommands();
        }

        public void Activate()
        {
            LoadTopTracks();
            LoadAlbums();
            LoadInfo();
            GetArtistImage();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(AllTracks);
            });

            ShowAllTracksCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/Search.ArtistAudioView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"viewModel", this}
                    }
                });
            });

            ShowAllAlbumsCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/Search.ArtistAlbumsView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"viewModel", this}
                    }
                });
            });


            GoToAlbumCommand = new RelayCommand<LastFmAlbum>(album =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/Search.AlbumView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"album", album}
                    }
                });
            });

            GoToArtistCommand = new RelayCommand<LastFmArtist>(artist =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/Search.ArtistView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"artist", artist}
                    }
                });
            });

            ShareCommand = new RelayCommand(() =>
            {
                var shareViewModel = new ShareViewModel();
                if (Tracks != null && Tracks.Count > 0)
                {
                    foreach (var track in AllTracks.Take(15))
                    {
                        shareViewModel.Tracks.Add(track);
                    }
                }

                if (File.Exists(App.Root + "/Cache/artists/" + Artist.Name + ".jpg"))
                {
                    shareViewModel.ImagePath = App.Root + "/Cache/artists/" + Artist.Name + ".jpg";
                    shareViewModel.Image = new BitmapImage(new Uri(shareViewModel.ImagePath));
                }

                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new ShareView() { DataContext = shareViewModel };
                flyout.Show();

                shareViewModel.Activate();
            });
        }


        private async void LoadTopTracks()
        {
            IsWorking = true;

            try
            {
                var tracks =
                    await DataService.GetArtistTopTracks(Artist.Mbid, Artist.Name, MAX_TRACKS_COUNT);
                if (tracks != null)
                {
                    for (var i = 0; i < tracks.Count; i++)
                        tracks[i].Order = i + 1;
                    _allTracks = tracks;
                    Tracks = tracks.Take(5).ToList();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void LoadAlbums()
        {
            IsWorking = true;

            try
            {
                _allAlbums = await DataService.GetArtistAlbums(Artist.Mbid, Artist.Name);
                if (_allAlbums != null && _allAlbums.Count > 0)
                    Albums = _allAlbums.Take(10).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void LoadInfo()
        {
            IsWorking = true;

            try
            {
                var info = await DataService.GetArtistInfo(Artist.Mbid, Artist.Name);
                if (info != null)
                {
                    //info.Bio = ProccessBio(info.Bio);
                    if (info.SimilarArtists != null)
                        SimilarArtists = info.SimilarArtists.ToList();
                    Artist = info;
                    Tags = info.TopTags;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void GetArtistImage()
        {
            if (Artist == null)
                return;

            try
            {
                var cachedImage = await CacheService.GetCachedImage("artists/" + CacheService.GetSafeFileName(Artist.Name + "_big.jpg"));
                if (cachedImage != null)
                {
                    ArtistImage = cachedImage;
                    return;
                }

                var imageUri = await DataService.GetArtistImage(Artist.Name, true);
                if (imageUri != null)
                {
                    cachedImage = await CacheService.CacheImage(imageUri.OriginalString, "artists/" + CacheService.GetSafeFileName(Artist.Name + "_big.jpg"));

                    ArtistImage = cachedImage;
                    return;
                }

                ArtistImage = null;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }
    }
}
