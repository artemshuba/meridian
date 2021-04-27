using Jupiter.Collections;
using Jupiter.Mvvm;
using Meridian.Controls;
using Meridian.Interfaces;
using Meridian.Model.Discovery;
using Meridian.Services;
using Meridian.Services.Discovery;
using Meridian.Utils.Helpers;
using Meridian.View.Discovery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.VK
{
    public class SearchViewModel : TracksViewModelBase
    {
        private readonly DiscoveryService _discoveryService;

        private string _query;
        private int _totalTracksCount;

        private List<DiscoveryArtist> _artists;

        private List<DiscoveryAlbum> _albums;

        #region Commands

        public DelegateCommand<DiscoveryArtist> GoToArtistCommand { get; private set; }

        public DelegateCommand<DiscoveryAlbum> GoToAlbumCommand { get; private set; }

        #endregion

        public string Query
        {
            get { return _query; }
            set
            {
                if (Set(ref _query, value))
                    Search();
            }
        }

        public List<DiscoveryArtist> Artists
        {
            get { return _artists; }
            private set { Set(ref _artists, value); }
        }

        public List<DiscoveryAlbum> Albums
        {
            get { return _albums; }
            private set { Set(ref _albums, value); }
        }

        public SearchViewModel()
        {
            _discoveryService = Ioc.Resolve<DiscoveryService>();

            RegisterTasks("search");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Query = (string)parameters["query"];

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeToolbar()
        {
            var shuffleItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ShuffleAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = ShuffleAllCommand,
            };

            var sortItem = new ToolbarPicker()
            {
                Title = Resources.GetStringByKey("Toolbar_Sort"),
                Items = {
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByDateAdded") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByTitle") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByArtist") }
                },

                OnSelectedItemChanged = index =>
                {
                    this.SelectedSortType = SortTypes[index];
                }
            };

            var selectionModeItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Select"),
                Command = SwitchSelectionModeCommand,
                Icon = new SymbolIcon(Symbol.Bullets)
            };

            sortItem.SelectedItem = sortItem.Items.First();

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { shuffleItem, (ToolbarItem)sortItem, selectionModeItem });
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            GoToArtistCommand = new DelegateCommand<DiscoveryArtist>(artist =>
            {
                NavigationService.Navigate(typeof(ArtistView), new Dictionary<string, object>
                {
                    ["artist"] = artist
                });

                Analytics.TrackEvent(AnalyticsEvent.SearchGoToArtist, new Dictionary<string, object>
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

                Analytics.TrackEvent(AnalyticsEvent.SearchGoToAlbum, new Dictionary<string, object>
                {
                    ["albumId"] = album.Id,
                    ["albumName"] = album.Title
                });
            });
        }

        private void Search()
        {
            SearchTracks();
            SearchArtists();
            SearchAlbums();
        }

        private async void SearchTracks()
        {
            if (string.IsNullOrWhiteSpace(Query))
                return;

            var t = TaskStarted("search");

            try
            {
                var result = await _tracksService.SearchTracks(Query);

                if (result.Tracks != null)
                {
                    _totalTracksCount = result.TotalCount;

                    var tracksCollection = new IncrementalLoadingCollection<IAudio>(result.Tracks);
                    tracksCollection.HasMoreItemsRequested = () => _totalTracksCount > Tracks.Count;
                    tracksCollection.OnMoreItemsRequested = SearchMoreTracks;
                    Tracks = tracksCollection;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to perform search");
            }
            finally
            {
                t.Finish();
            }
        }

        private async Task<List<IAudio>> SearchMoreTracks(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.SearchTracks(Query, count: (int)count, offset: Tracks.Count);
                _totalTracksCount = result.TotalCount;
                return result.Tracks;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to search more");
            }

            return null;
        }

        private async void SearchArtists()
        {
            try
            {
                var artists = await _discoveryService.SearchArtists(Query);
                Artists = artists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to search artists'");
            }
        }

        private async void SearchAlbums()
        {
            try
            {
                var albums = await _discoveryService.SearchAlbums(Query);
                Albums = albums;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to search albums'");
            }
        }
    }
}