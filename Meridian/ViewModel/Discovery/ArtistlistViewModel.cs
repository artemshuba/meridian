using Jupiter.Mvvm;
using Meridian.Model.Discovery;
using Meridian.View.Discovery;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Discovery
{
    public class ArtistlistViewModel : ViewModelBase
    {
        private List<DiscoveryArtist> _artists;

        #region Commands

        /// <summary>
        /// Go to artist command
        /// </summary>
        public DelegateCommand<DiscoveryArtist> GoToArtistCommand { get; private set; }

        #endregion

        public List<DiscoveryArtist> Artists
        {
            get { return _artists; }
            private set
            {
                Set(ref _artists, value);
            }
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            base.OnNavigatedTo(parameters, mode);

            Artists = (List<DiscoveryArtist>)parameters["artists"];
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
            });
        }
    }
}