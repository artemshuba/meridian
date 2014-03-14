using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.ViewModel;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for NowPlayingView.xaml
    /// </summary>
    public partial class NowPlayingView : Page
    {
        public NowPlayingView()
        {
            InitializeComponent();

            this.DataContext = ViewModelLocator.NowPlaying;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }

        private void NowPlayingView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.NowPlaying.Activate();
        }

        private void NowPlayingView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.NowPlaying.Deactivate();
        }
    }
}
