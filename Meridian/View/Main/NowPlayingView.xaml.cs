using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.Model;
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

            LocalSearchBox.Filter = Filter;
        }

        private void NowPlayingView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.NowPlaying.Deactivate();
        }

        private void LocalSearchItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
            LocalSearchBox.IsActive = true;
        }

        private bool Filter(object o)
        {
            var track = (Audio)o;
            var query = LocalSearchBox.Query.ToLower();
            return track.Title.ToLower().StartsWith(query) || track.Artist.ToLower().StartsWith(query);
        }
    }
}
