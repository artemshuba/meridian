using System.Windows;
using System.Windows.Controls.Primitives;
using LastFmLib.Core.Album;
using Meridian.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Search;

namespace Meridian.View.Search
{
    /// <summary>
    /// Interaction logic for AlbumView.xaml
    /// </summary>
    public partial class AlbumView : PageBase
    {
        private AlbumViewModel _viewModel;

        public AlbumView()
        {
            InitializeComponent();

            _viewModel = new AlbumViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            var album = (LastFmAlbum)NavigationContext.Parameters["album"];
            _viewModel.Album = album;

            _viewModel.Activate();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }
    }
}
