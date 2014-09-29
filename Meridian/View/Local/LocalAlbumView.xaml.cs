using System.Windows;
using System.Windows.Media.Effects;
using Meridian.Controls;
using Meridian.Model;
using Meridian.ViewModel.Local;

namespace Meridian.View.Local
{
    /// <summary>
    /// Interaction logic for LocalAlbumView.xaml
    /// </summary>
    public partial class LocalAlbumView : PageBase
    {
        private LocalAlbumViewModel _viewModel;

        public LocalAlbumView()
        {
            InitializeComponent();


            _viewModel = new LocalAlbumViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            var album = (AudioAlbum)NavigationContext.Parameters["album"];
            _viewModel.Album = album;

            _viewModel.Activate();
        }

        private void LocalAlbumView_OnLoaded(object sender, RoutedEventArgs e)
        {
            BackgroundArtControl.Effect = Domain.Settings.Instance.BlurBackground ? new BlurEffect() { RenderingBias = RenderingBias.Quality, Radius = 80 } : null;
        }
    }
}
