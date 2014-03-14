using System.Windows;
using System.Windows.Controls.Primitives;
using Meridian.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Search;

namespace Meridian.View.Search
{
    /// <summary>
    /// Interaction logic for ArtistAudioView.xaml
    /// </summary>
    public partial class ArtistAlbumsView : PageBase
    {
        public ArtistAlbumsView()
        {
            InitializeComponent();
        }

        public override void OnNavigatedTo()
        {
            var viewModel = (ArtistViewModel)NavigationContext.Parameters["viewModel"];
            this.DataContext = viewModel;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }
    }
}
