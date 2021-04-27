using Jupiter.Collections;
using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Meridian.Enum;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using Meridian.Utils.Helpers;
using Meridian.View.Compact.Vk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkLib.Core.Groups;
using VkLib.Core.Users;
using VkLib.Error;
using Microsoft.UI.Xaml.Navigation;


namespace Meridian.ViewModel.VK
{
    public class SocietyMusicViewModel : TracksViewModelBase
    {
        private VkGroup _society;

        private int _tabIndex;

        private AlbumFilterType? _selectedAlbumFilterType = AlbumFilterType.All;

        private IPlaylist _selectedAlbum;

        private int _totalPlaylistsCount;

        private IncrementalLoadingCollection<IPlaylist> _playlists;

        private IncrementalLoadingCollection<AudioPost> _wallPosts;
        private int _wallPostsTotalCount;

        #region Commands

        /// <summary>
        /// Go to playlist command (for compact mode)
        /// </summary>
        public DelegateCommand<PlaylistVk> GoToPlaylistCommand { get; private set; }

        #endregion

        /// <summary>
        /// Society
        /// </summary>
        public VkGroup Society
        {
            get { return _society; }
            private set
            {
                Set(ref _society, value);
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
        /// Playlists
        /// </summary>
        public IncrementalLoadingCollection<IPlaylist> Playlists
        {
            get { return _playlists; }
            private set { Set(ref _playlists, value); }
        }

        /// <summary>
        /// Wall posts
        /// </summary>
        public IncrementalLoadingCollection<AudioPost> WallPosts
        {
            get { return _wallPosts; }
            private set { Set(ref _wallPosts, value); }
        }

        public SocietyMusicViewModel()
        {
            RegisterTasks("playlists", "wall");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Society = (VkGroup)parameters["society"];

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

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
                    if (force || _wallPosts.IsNullOrEmpty())
                        await LoadWall();
                    break;
            }

            IsToolbarEnabled = true;
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

        private async Task LoadTracks(string playlistId = null, bool withoutPlaylistOnly = false)
        {
            TaskStarted("tracks");

            try
            {
                var albumId = playlistId != null ? long.Parse(playlistId) : 0;
                var tracks = await _tracksService.GetTracks(userId: -Society.Id, albumId: albumId);
                if (tracks == null)
                    return;

                if (withoutPlaylistOnly)
                    tracks = tracks?.Where(t => t.PlaylistId == 0).ToList();

                Tracks = new ObservableCollection<IAudio>(tracks);
            }
            catch (VkAccessDeniedException)
            {
                TaskError("tracks", Resources.GetStringByKey("Society_TracksDisabled"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load society tracks");
            }
            finally
            {
                TaskFinished("tracks");
            }
        }

        private async Task LoadPlaylists()
        {
            TaskStarted("playlists");

            try
            {
                var result = await _tracksService.GetPlaylists(userId: -Society.Id, tracks: _tracks);

                _totalPlaylistsCount = result.TotalCount;
                Playlists = new IncrementalLoadingCollection<IPlaylist>(result.Playlists ?? new List<IPlaylist>());
                Playlists.HasMoreItemsRequested = () => _totalPlaylistsCount > Playlists?.Count;
                Playlists.OnMoreItemsRequested = LoadMorePlaylists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load society playlists");
            }

            TaskFinished("playlists");
        }

        private async Task<List<IPlaylist>> LoadMorePlaylists(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetPlaylists(userId: -Society.Id, tracks: _tracks, count: (int)count, offset: Playlists.Count);
                return result.Playlists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more society playlists");
            }

            return null;
        }

        private async Task LoadWall()
        {
            TaskStarted("wall");

            try
            {
                var result = await _tracksService.GetWallPosts(ownerId: -Society.Id);
                _wallPostsTotalCount = result.TotalCount;
                WallPosts = new IncrementalLoadingCollection<AudioPost>(result.Posts ?? new List<AudioPost>());
                WallPosts.HasMoreItemsRequested = () => WallPosts.Count > _wallPostsTotalCount;
                WallPosts.OnMoreItemsRequested = LoadMoreWallPosts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load society wall");
            }

            TaskFinished("wall");
        }

        private async Task<List<AudioPost>> LoadMoreWallPosts(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetWallPosts((int)count, _wallPosts.Count, ownerId: -Society.Id);
                _totalPlaylistsCount = result.TotalCount;
                return result.Posts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more society wall posts");
            }

            return null;
        }
    }
}
