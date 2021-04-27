using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using System.Collections.Generic;
using System.Linq;
using Jupiter.Utils.Extensions;
using Jupiter.Collections;
using System;
using Meridian.Enum;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Utils.Messaging;
using Jupiter.Services.Navigation;
using Jupiter.Mvvm;
using Meridian.View.VK;
using Meridian.Controls;
using Meridian.View.Compact.Vk;
using Meridian.Utils.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace Meridian.ViewModel.VK
{
    public class MyMusicViewModel : TracksViewModelBase
    {
        private IncrementalLoadingCollection<IPlaylist> _playlists;
        private int _totalPlaylistsCount;
        private IPlaylist _selectedAlbum;

        private AlbumFilterType? _selectedAlbumFilterType = AlbumFilterType.All;

        private object _reorderingTrack;
        private int _reorderingTrackIndex;

        private int _tabIndex;

        private IncrementalLoadingCollection<AudioPost> _news;
        private string _newsNextFrom;

        private IncrementalLoadingCollection<AudioPost> _wallPosts;
        private int _wallPostsTotalCount;

        #region Commands

        /// <summary>
        /// Add new playlist command
        /// </summary>
        public DelegateCommand AddPlaylistCommand { get; private set; }

        /// <summary>
        /// Go to playlist command (for compact mode)
        /// </summary>
        public DelegateCommand<PlaylistVk> GoToPlaylistCommand { get; private set; }

        #endregion

        /// <summary>
        /// Playlists
        /// </summary>
        public IncrementalLoadingCollection<IPlaylist> Playlists
        {
            get { return _playlists; }
            private set { Set(ref _playlists, value); }
        }

        /// <summary>
        /// Selected album
        /// </summary>
        public IPlaylist SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                if (Set(ref _selectedAlbum, value))
                {
                    if (value != null)
                    {
                        SelectedAlbumFilterType = null;

                        ApplyFilter();
                    }
                    else
                        RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Album filter type
        /// </summary>
        public List<AlbumFilterType> AlbumFilterTypes { get; } = System.Enum.GetValues(typeof(AlbumFilterType)).Cast<AlbumFilterType>().ToList();

        /// <summary>
        /// Selected album filter type
        /// </summary>
        public AlbumFilterType? SelectedAlbumFilterType
        {
            get { return _selectedAlbumFilterType; }
            set
            {
                if (Set(ref _selectedAlbumFilterType, value))
                {
                    if (value != null)
                    {
                        SelectedAlbum = null;

                        ApplyFilter();
                    }
                    else
                        RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Active tab index
        /// </summary>
        public int TabIndex
        {
            get { return _tabIndex; }
            set
            {
                if (Set(ref _tabIndex, value))
                {
                    InitializeToolbar();

                    _ = Load();
                }
            }
        }

        /// <summary>
        /// News
        /// </summary>
        public IncrementalLoadingCollection<AudioPost> News
        {
            get { return _news; }
            private set { Set(ref _news, value); }
        }

        /// <summary>
        /// Wall posts
        /// </summary>
        public IncrementalLoadingCollection<AudioPost> WallPosts
        {
            get { return _wallPosts; }
            private set { Set(ref _wallPosts, value); }
        }

        public MyMusicViewModel()
        {
            RegisterTasks("playlists", "news", "wall");
        }

        public override async void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Messenger.Default.Register<MessageMyMusicChanged>(this, OnMessageMyMusicChanged);
            Messenger.Default.Register<MessagePlaylistRemoved>(this, OnMessagePlaylistRemoved);

            await Load();
        }

        public override void OnNavigatingFrom(NavigatingEventArgs e)
        {
            Messenger.Default.Unregister<MessageMyMusicChanged>(this, OnMessageMyMusicChanged);
            Messenger.Default.Unregister<MessagePlaylistRemoved>(this, OnMessagePlaylistRemoved);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            //place other commands here
            AddPlaylistCommand = new DelegateCommand(async () =>
            {
                var newPlaylist = (IPlaylist)await PopupControl.Show<EditPlaylistView>();
                if (newPlaylist != null)
                {
                    Playlists.Insert(0, newPlaylist);
                }
            });

            GoToPlaylistCommand = new DelegateCommand<PlaylistVk>(playlist =>
            {
                NavigationService.Navigate(typeof(PlaylistView), new Dictionary<string, object>
                {
                    ["playlist"] = playlist
                });
            });

            ShuffleAllCommand = new DelegateCommand(() =>
            {
                List<IAudio> tracks = null;
                switch (_tabIndex)
                {
                    case 0:
                        tracks = Tracks.ToList();
                        break;

                    case 1:
                        tracks = _news?.SelectMany(p => p.Tracks).ToList();
                        break;

                    case 2:
                        tracks = _wallPosts?.SelectMany(p => p.Tracks).ToList();
                        break;
                }

                if (tracks.IsNullOrEmpty())
                    return;

                tracks.Shuffle();

                AudioService.Instance.PlayAudio(tracks.First(), tracks);
            });

            PlayAllCommand = new DelegateCommand(() =>
            {
                List<IAudio> tracks = null;
                if (_tabIndex == 1)
                    tracks = _news?.SelectMany(p => p.Tracks).ToList();
                else if (_tabIndex == 2)
                    tracks = _wallPosts?.SelectMany(p => p.Tracks).ToList();

                if (tracks.IsNullOrEmpty())
                    return;

                AudioService.Instance.PlayAudio(tracks.First(), tracks);
            });
        }

        #region Toolbar

        protected override void InitializeToolbar()
        {
            if (_tabIndex == 0)
                InitializeTracksToolbarItems();
            else
                InitializePostsToolbarItems();
        }

        #endregion

        protected override async Task Load(bool force = false)
        {
            IsToolbarEnabled = false;

            switch (TabIndex)
            {
                case 0:
                    if (force || _tracks.IsNullOrEmpty())
                    {
                        await LoadTracks();
                        await LoadPlaylists();
                    }
                    break;

                case 1:
                    if (force || _news.IsNullOrEmpty())
                        await LoadNews();

                    Analytics.TrackEvent(AnalyticsEvent.NewsTab);
                    break;

                case 2:
                    if (force || _wallPosts.IsNullOrEmpty())
                        await LoadWall();

                    Analytics.TrackEvent(AnalyticsEvent.WallTab);
                    break;
            }

            IsToolbarEnabled = true;
        }

        private async Task LoadTracks(string playlistId = null, bool withoutPlaylistOnly = false)
        {
            TaskStarted("tracks");

            try
            {
                var albumId = playlistId != null ? long.Parse(playlistId) : 0;
                var tracks = await _tracksService.GetTracks(albumId: albumId);
                if (tracks == null)
                    return;

                if (withoutPlaylistOnly)
                    tracks = tracks?.Where(t => t.PlaylistId == 0).ToList();

                if (Tracks != null)
                    Tracks.CollectionChanged -= Tracks_CollectionChanged;

                Tracks = new ObservableCollection<IAudio>(tracks);
                Tracks.CollectionChanged += Tracks_CollectionChanged;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load tracks");
            }
            finally
            {
                TaskFinished("tracks");
            }

            if (!Tracks.IsNullOrEmpty() && AudioService.Instance.CurrentPlaylist.Items.Count == 0)
            {
                AudioService.Instance.SetCurrentPlaylist(new AudioPlaylist(Tracks));
            }
        }

        private async Task LoadPlaylists()
        {
            TaskStarted("playlists");

            try
            {
                var result = await _tracksService.GetPlaylists(tracks: _tracks);

                _totalPlaylistsCount = result.TotalCount;
                Playlists = new IncrementalLoadingCollection<IPlaylist>(result.Playlists ?? new List<IPlaylist>());
                Playlists.HasMoreItemsRequested = () => _totalPlaylistsCount > Playlists?.Count;
                Playlists.OnMoreItemsRequested = LoadMorePlaylists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load playlists");
            }

            TaskFinished("playlists");
        }

        private async Task<List<IPlaylist>> LoadMorePlaylists(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetPlaylists(tracks: _tracks, count: (int)count, offset: Playlists.Count);
                return result.Playlists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more playlists");
            }

            return null;
        }

        private async Task LoadNews()
        {
            TaskStarted("news");

            try
            {
                var result = await _tracksService.GetNews();
                _newsNextFrom = result.NextFrom;
                News = new IncrementalLoadingCollection<AudioPost>(result.Posts ?? new List<AudioPost>());
                News.HasMoreItemsRequested = () => !string.IsNullOrEmpty(_newsNextFrom);
                News.OnMoreItemsRequested = LoadMoreNews;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load news");
            }

            TaskFinished("news");
        }

        private async Task<List<AudioPost>> LoadMoreNews(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetNews((int)count, _newsNextFrom);
                _newsNextFrom = result.NextFrom;
                return result.Posts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more news");
            }

            return null;
        }

        private async Task LoadWall()
        {
            TaskStarted("wall");

            try
            {
                var result = await _tracksService.GetWallPosts();
                _wallPostsTotalCount = result.TotalCount;
                WallPosts = new IncrementalLoadingCollection<AudioPost>(result.Posts ?? new List<AudioPost>());
                WallPosts.HasMoreItemsRequested = () => WallPosts.Count > _wallPostsTotalCount;
                WallPosts.OnMoreItemsRequested = LoadMoreWallPosts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load wall");
            }

            TaskFinished("wall");
        }

        private async Task<List<AudioPost>> LoadMoreWallPosts(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetWallPosts((int)count, _wallPosts.Count);
                _totalPlaylistsCount = result.TotalCount;
                return result.Posts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more wall posts");
            }

            return null;
        }

        private async void ApplyFilter()
        {
            try
            {
                await LoadTracks(playlistId: _selectedAlbum?.Id, withoutPlaylistOnly: _selectedAlbumFilterType == AlbumFilterType.Unsorted);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to filter tracks");
            }
        }

        #region Reordering stuff

        private void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    _reorderingTrack = e.OldItems[0];
                    _reorderingTrackIndex = e.OldStartingIndex;
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (_reorderingTrack == null)
                        return;

                    ReorderTracks(_reorderingTrack as IAudio, _reorderingTrackIndex, e.NewStartingIndex);
                    break;
            }
        }

        private async void ReorderTracks(IAudio track, int indexFrom, int indexTo)
        {
            Logger.Info($"Reorder: {track.Id} from {indexFrom} to {indexTo}");

            try
            {
                string afterTrackId = "0";
                string beforeTrackId = "0";

                if (indexTo > 0)
                    afterTrackId = _tracks[indexTo - 1].Id;

                if (indexTo < _tracks.Count - 1)
                    beforeTrackId = _tracks[indexTo + 1].Id;

                await _tracksService.ReorderTracks(track, beforeTrackId, afterTrackId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to reorder tracks");
            }
        }

        #endregion

        #region Messaging

        private void OnMessageMyMusicChanged(MessageMyMusicChanged message)
        {
            if (!message.AddedTracks.IsNullOrEmpty())
            {
                foreach (var track in message.AddedTracks)
                {
                    Tracks.Insert(0, track);
                }
            }

            if (!message.RemovedTracks.IsNullOrEmpty())
            {
                foreach (var track in message.RemovedTracks)
                {
                    Tracks.Remove(Tracks.FirstOrDefault(t => t.Id == track.Id));
                }
            }
        }

        private void OnMessagePlaylistRemoved(MessagePlaylistRemoved message)
        {
            Playlists.Remove(Playlists.FirstOrDefault(a => a.Id == message.Playlist.Id));
        }

        #endregion
    }
}