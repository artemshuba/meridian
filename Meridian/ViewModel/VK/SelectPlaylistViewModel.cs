using Jupiter.Collections;
using Jupiter.Mvvm;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meridian.ViewModel.VK
{
    public class SelectPlaylistViewModel : PopupViewModelBase
    {
        private readonly VkTracksService _tracksService;

        private IncrementalLoadingCollection<IPlaylist> _playlists;
        private int _totalPlaylistsCount;

        #region Commands

        public DelegateCommand<PlaylistVk> SelectPlaylistCommand { get; private set; }

        #endregion

        public IncrementalLoadingCollection<IPlaylist> Playlists
        {
            get { return _playlists; }
            private set { Set(ref _playlists, value); }
        }

        public SelectPlaylistViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();

            Load();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            SelectPlaylistCommand = new DelegateCommand<PlaylistVk>(playlist =>
            {
                Close(playlist);
            });
        }

        private async void Load()
        {
            try
            {
                var result = await _tracksService.GetPlaylists();

                _totalPlaylistsCount = result.TotalCount;
                Playlists = new IncrementalLoadingCollection<IPlaylist>(result.Playlists ?? new List<IPlaylist>());
                Playlists.HasMoreItemsRequested = () => _totalPlaylistsCount > Playlists?.Count;
                Playlists.OnMoreItemsRequested = LoadMorePlaylists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load playlists");
            }
        }

        private async Task<List<IPlaylist>> LoadMorePlaylists(CancellationToken token, uint count)
        {
            try
            {
                var result = await _tracksService.GetPlaylists(tracks: null, count: (int)count, offset: Playlists.Count);
                return result.Playlists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more playlists");
            }

            return null;
        }
    }
}