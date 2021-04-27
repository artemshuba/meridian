using Jupiter.Mvvm;
using Meridian.Interfaces;
using Meridian.View.Compact.Vk;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Common
{
    public class PlaylistsListViewModel : ViewModelBase
    {
        private List<IPlaylist> _playlists;

        #region Commands

        /// <summary>
        /// Go to playlist command
        /// </summary>
        public DelegateCommand<IPlaylist> GoToPlaylistCommand { get; private set; }

        #endregion

        public List<IPlaylist> Playlists
        {
            get { return _playlists; }
            private set
            {
                Set(ref _playlists, value);
            }
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            base.OnNavigatedTo(parameters, mode);

            Playlists = (List<IPlaylist>)parameters["playlists"];
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            GoToPlaylistCommand = new DelegateCommand<IPlaylist>(playlist =>
            {
                NavigationService.Navigate(typeof(PlaylistView), new Dictionary<string, object>
                {
                    ["playlist"] = playlist
                });
            });
        }
    }
}