using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LastFmLib.Core.Album;
using LastFmLib.Core.Artist;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.Extensions;
using Neptune.Messages;

namespace Meridian.ViewModel.Search
{
    public class SearchViewModel : ViewModelBase
    {
        private const int MAX_AUDIO_SEARCH = 300;

        private readonly List<SearchMenuItem> _sections = new List<SearchMenuItem>
        {
            new SearchMenuItem() {Group = MainResources.MainMenuVkTitle, Title = MainResources.SearchSectionTracks, GroupIcon = Application.Current.Resources["VkIcon"]},
            new SearchMenuItem() {Group = MainResources.MainMenuVkTitle, Title = MainResources.SearchSectionAlbums, GroupIcon = Application.Current.Resources["VkIcon"]},
            new SearchMenuItem() {Group = MainResources.MainMenuVkTitle, Title = MainResources.SearchSectionArtists, GroupIcon = Application.Current.Resources["VkIcon"]},

            new SearchMenuItem() {Group = MainResources.MainMenuLocalTitle, GroupIcon = Application.Current.Resources["DeviceIcon"], Title = MainResources.SearchSectionTracks},
            new SearchMenuItem() {Group = MainResources.MainMenuLocalTitle, GroupIcon = Application.Current.Resources["DeviceIcon"], Title = MainResources.SearchSectionAlbums},
            new SearchMenuItem() {Group = MainResources.MainMenuLocalTitle, GroupIcon = Application.Current.Resources["DeviceIcon"], Title = MainResources.SearchSectionArtists}
        };
        private string _query;
        private int _selectedSectionIndex;
        private ObservableCollection<object> _searchResults;
        private CancellationTokenSource _cancellationToken;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand<LastFmAlbum> GoToAlbumCommand { get; private set; }

        public RelayCommand<AudioAlbum> GoToLocalAlbumCommand { get; private set; }

        public RelayCommand<LastFmArtist> GoToArtistCommand { get; private set; }

        #endregion

        public List<SearchMenuItem> Sections
        {
            get { return _sections; }
        }

        public string Header
        {
            get { return MainResources.SearchResultsTitle + " \"" + Query + "\""; }
        }

        public string Query
        {
            get { return _query; }
            set
            {
                if (Set(ref _query, value))
                {
                    RaisePropertyChanged("Header");
                    Search();
                }
            }
        }

        public int SelectedSectionIndex
        {
            get { return _selectedSectionIndex; }
            set
            {
                if (Set(ref _selectedSectionIndex, value))
                    Search();
            }
        }

        public ObservableCollection<object> SearchResults
        {
            get { return _searchResults; }
            set { Set(ref _searchResults, value); }
        }

        public SearchViewModel()
        {
            _cancellationToken = new CancellationTokenSource();

            RegisterTasks("results");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                switch (SelectedSectionIndex)
                {
                    //tracks
                    case 0:
                        AudioService.SetCurrentPlaylist(SearchResults.Cast<Audio>());
                        break;

                    //local artists
                    case 5:
                        AudioService.SetCurrentPlaylist(SearchResults.Cast<AudioArtist>().Where(a => a.Albums != null).SelectMany(a => a.Albums).SelectMany(a => a.Tracks).ToList());
                        break;
                }
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

            GoToLocalAlbumCommand = new RelayCommand<AudioAlbum>(album =>
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
        }

        private void Search()
        {
            CancelAsync();

            switch (_selectedSectionIndex)
            {
                case 0:
                    SearchTracks(_cancellationToken.Token);
                    break;

                case 1:
                    SearchAlbums(_cancellationToken.Token);
                    break;

                case 2:
                    SearchArtists(_cancellationToken.Token);
                    break;

                case 3:
                    SearchLocalTracks(_cancellationToken.Token);
                    break;

                case 4:
                    SearchLocalAlbums(_cancellationToken.Token);
                    break;

                case 5:
                    SearchLocalArtists(_cancellationToken.Token);
                    break;
            }
        }

        private async void SearchTracks(CancellationToken token)
        {
            OnTaskStarted("results");
            SearchResults = new ObservableCollection<object>();

            try
            {
                int offset = 0;
                const int count = 100;
                int requestsCount = 0;

                while (SearchResults != null && SearchResults.Count < MAX_AUDIO_SEARCH)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Tracks search cancelled");
                        break;
                    }

                    var a = await DataService.SearchAudio(Query, count, offset);
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("results");
                    }

                    offset += count;

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Music search cancelled");
                        break;
                    }

                    foreach (var audio in a)
                    {
                        SearchResults.Add(audio);
                    }

                    requestsCount++;

                    if (requestsCount >= 2) //не больше 2-х запросов в секунду
                    {
                        requestsCount = 0;
                        await Task.Delay(1000);
                    }
                }

                if ((SearchResults == null || SearchResults.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private async void SearchAlbums(CancellationToken token)
        {
            SearchResults = null;
            OnTaskStarted("results");

            SearchResults = null;

            try
            {
                var albums = await DataService.SearchAlbums(_query);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Albums search cancelled");
                    return;
                }

                if (albums != null && albums.Count > 0)
                {
                    SearchResults = new ObservableCollection<object>(albums);
                }
                else
                {
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private async void SearchArtists(CancellationToken token)
        {
            SearchResults = null;
            OnTaskStarted("results");

            SearchResults = null;

            try
            {
                var artists = await DataService.SearchArtists(_query);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Artists search cancelled");
                    return;
                }

                if (artists != null && artists.Count > 0)
                {
                    string q = Query.ToLower();
                    artists = artists.OrderByDescending(a => a.Name.ToLower() == q).ThenByDescending(a => a.Name.ToLower().StartsWith(q)).ToList();
                    SearchResults = new ObservableCollection<object>(artists);
                }
                else
                {
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private async void SearchLocalTracks(CancellationToken token)
        {
            SearchResults = null;
            OnTaskStarted("results");

            SearchResults = null;

            try
            {
                var tracks = await ServiceLocator.LocalMusicService.SearchTracks(_query);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Local tracks search cancelled");
                    return;
                }

                if (tracks != null && tracks.Count > 0)
                {
                    SearchResults = new ObservableCollection<object>(tracks);
                }
                else
                {
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private async void SearchLocalAlbums(CancellationToken token)
        {
            SearchResults = null;
            OnTaskStarted("results");

            SearchResults = null;

            try
            {
                var albums = await ServiceLocator.LocalMusicService.SearchAlbums(_query);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Local albums search cancelled");
                    return;
                }

                if (albums != null && albums.Count > 0)
                {
                    SearchResults = new ObservableCollection<object>(albums);
                }
                else
                {
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private async void SearchLocalArtists(CancellationToken token)
        {
            SearchResults = null;
            OnTaskStarted("results");

            SearchResults = null;

            try
            {
                var artists = await ServiceLocator.LocalMusicService.SearchArtists(_query);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Local artists search cancelled");
                    return;
                }

                if (artists != null && artists.Count > 0)
                {
                    var results = new List<AudioArtist>();
                    foreach (var artist in artists)
                    {
                        var a = new AudioArtist(){Title = artist.Title};

                        var albums = await ServiceLocator.LocalMusicService.GetArtistAlbums(artist.Id);

                        if (token.IsCancellationRequested)
                        {
                            Debug.WriteLine("Local artists search cancelled");
                            return;
                        }

                        if (!albums.IsNullOrEmpty())
                        {
                            foreach (var album in albums)
                            {
                                var tracks = await ServiceLocator.LocalMusicService.GetAlbumTracks(album.Id);
                                if (token.IsCancellationRequested)
                                {
                                    Debug.WriteLine("Local artists search cancelled");
                                    return;
                                }

                                if (!tracks.IsNullOrEmpty())
                                    album.Tracks = tracks.Cast<Audio>().ToList();
                            }
                        }

                        a.Albums = albums;
                        results.Add(a);
                    }

                    SearchResults = new ObservableCollection<object>(results);
                }
                else
                {
                    OnTaskError("results", ErrorResources.LoadSearchErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("results", ErrorResources.LoadSearchErrorCommon);
            }

            OnTaskFinished("results");
        }

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }
    }
}
