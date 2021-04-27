using Jupiter.Mvvm;
using Meridian.Model.Discovery;
using Meridian.View.Discovery;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Discovery
{
    public class AlbumlistViewModel : ViewModelBase
    {
        private List<DiscoveryAlbum> _albums;

        #region Commands

        /// <summary>
        /// Go to album command
        /// </summary>
        public DelegateCommand<DiscoveryAlbum> GoToAlbumCommand { get; private set; }

        #endregion

        public List<DiscoveryAlbum> Albums
        {
            get { return _albums; }
            private set
            {
                Set(ref _albums, value);
            }
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            base.OnNavigatedTo(parameters, mode);

            Albums = (List<DiscoveryAlbum>)parameters["albums"];
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            GoToAlbumCommand = new DelegateCommand<DiscoveryAlbum>(album =>
            {
                NavigationService.Navigate(typeof(AlbumView), new Dictionary<string, object>
                {
                    ["album"] = album
                });
            });
        }
    }
}