using System.Windows;
using System.Windows.Controls.Primitives;
using LastFmLib.Core.Artist;
using Meridian.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Search;

namespace Meridian.View.Search
{
    /// <summary>
    /// Interaction logic for ArtistView.xaml
    /// </summary>
    public partial class ArtistView : PageBase
    {
        private ArtistViewModel _viewModel;

        public ArtistView()
        {
            InitializeComponent();

            _viewModel = new ArtistViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            var artist = (LastFmArtist)NavigationContext.Parameters["artist"];
            _viewModel.Artist = artist;

            _viewModel.Activate();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }
    }
}
