using Jupiter.Mvvm;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Model.Discovery;
using Meridian.Services;
using Meridian.Services.Discovery;
using Meridian.View.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Discovery
{
    public class ArtistViewModel : ViewModelBase
    {
        private readonly DiscoveryService _discoveryService;
        private ImageService _imageService;

        private DiscoveryArtist _artist;
        private List<DiscoveryTrack> _allTopTracks;
        private List<DiscoveryTrack> _topTracks;
        private List<DiscoveryAlbum> _albums;
        private List<DiscoveryArtist> _relatedArtists;

        private CachedImage _artistImage;

        #region Commands

        /// <summary>
        /// Play track command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackCommand { get; protected set; }

        /// <summary>
        /// Go to artist command
        /// </summary>
        public DelegateCommand<DiscoveryArtist> GoToArtistCommand { get; private set; }

        /// <summary>
        /// Go to album command
        /// </summary>
        public DelegateCommand<DiscoveryAlbum> GoToAlbumCommand { get; private set; }

        /// <summary>
        /// Show more tracks command
        /// </summary>
        public DelegateCommand ShowMoreTracksCommand { get; private set; }

        /// <summary>
        /// Show more artists command
        /// </summary>
        public DelegateCommand ShowMoreArtistsCommand { get; private set; }

        /// <summary>
        /// Show more albums command
        /// </summary>
        public DelegateCommand ShowMoreAlbumsCommand { get; private set; }

        #endregion

        /// <summary>
        /// Artist
        /// </summary>
        public DiscoveryArtist Artist
        {
            get { return _artist; }
            private set
            {
                if (Set(ref _artist, value))
                    Load();
            }
        }

        /// <summary>
        /// Top tracks
        /// </summary>
        public List<DiscoveryTrack> TopTracks
        {
            get { return _topTracks; }
            private set
            {
                Set(ref _topTracks, value);
            }
        }

        /// <summary>
        /// Top tracks
        /// </summary>
        public List<DiscoveryAlbum> Albums
        {
            get { return _albums; }
            private set
            {
                Set(ref _albums, value);
            }
        }

        /// <summary>
        /// Top tracks
        /// </summary>
        public List<DiscoveryArtist> RelatedArtists
        {
            get { return _relatedArtists; }
            private set
            {
                Set(ref _relatedArtists, value);
            }
        }

        /// <summary>
        /// Artist image
        /// </summary>
        public CachedImage ArtistImage
        {
            get { return _artistImage; }
            private set { Set(ref _artistImage, value); }
        }

        public ArtistViewModel()
        {
            _discoveryService = Ioc.Resolve<DiscoveryService>();
            _imageService = Ioc.Resolve<ImageService>();

            RegisterTasks("tracks", "albums", "artists");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Artist = (DiscoveryArtist)parameters["artist"];

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            PlayTrackCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.PlayAudio(track, TopTracks.OfType<IAudio>().ToList());
            });

            GoToArtistCommand = new DelegateCommand<DiscoveryArtist>(artist =>
            {
                NavigationService.Navigate(typeof(ArtistView), new Dictionary<string, object>
                {
                    ["artist"] = artist
                });

                Analytics.TrackEvent(AnalyticsEvent.SearchGoToRelatedArtist, new Dictionary<string, object>
                {
                    ["artistId"] = artist.Id,
                    ["artistName"] = artist.Name
                });
            });

            GoToAlbumCommand = new DelegateCommand<DiscoveryAlbum>(album =>
            {
                NavigationService.Navigate(typeof(AlbumView), new Dictionary<string, object>
                {
                    ["album"] = album
                });

                Analytics.TrackEvent(AnalyticsEvent.SearchGoToArtistAlbum, new Dictionary<string, object>
                {
                    ["albumId"] = album.Id,
                    ["albumName"] = album.Title
                });
            });

            ShowMoreTracksCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(TracklistView), new Dictionary<string, object>
                {
                    ["tracks"] = _allTopTracks
                });
            });

            ShowMoreArtistsCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(ArtistlistView), new Dictionary<string, object>
                {
                    ["artists"] = RelatedArtists
                });
            });

            ShowMoreAlbumsCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(AlbumListView), new Dictionary<string, object>
                {
                    ["albums"] = Albums
                });
            });
        }

        private void Load()
        {
            LoadTopTracks();
            LoadAlbums();
            LoadRelated();
            LoadArtistImage();
        }

        private async void LoadTopTracks()
        {
            var t = TaskStarted("tracks");
            try
            {
                var topTracks = await _discoveryService.GetArtistTopTracks(Artist.Id, count: 100);
                if (topTracks != null)
                {
                    _allTopTracks = topTracks;
                    TopTracks = topTracks.Take(10).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load artist's top tracks");
            }
            finally
            {
                t.Finish();
            }
        }

        private async void LoadAlbums()
        {
            var t = TaskStarted("albums");
            try
            {
                Albums = await _discoveryService.GetArtistAlbums(Artist.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load artist's albums");
            }
            finally
            {
                t.Finish();
            }
        }

        private async void LoadRelated()
        {
            var t = TaskStarted("artists");
            try
            {
                RelatedArtists = await _discoveryService.GetArtistRelated(Artist.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load artist's related");
            }
            finally
            {
                t.Finish();
            }
        }

        private async void LoadArtistImage()
        {
            try
            {
                var image = await _imageService.GetArtistImage(Artist.Name);
                ArtistImage = image;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load artist image");
            }
        }
    }
}
