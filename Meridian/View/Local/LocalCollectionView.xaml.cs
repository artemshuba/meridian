using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.Model;
using Meridian.ViewModel.Local;

namespace Meridian.View.Local
{
    /// <summary>
    /// Interaction logic for LocalCollectionView.xaml
    /// </summary>
    public partial class LocalCollectionView : Page
    {
        private LocalMusicViewModel _viewModel;

        public LocalCollectionView()
        {
            InitializeComponent();

            _viewModel = new LocalMusicViewModel();
            this.DataContext = _viewModel;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
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
            return track.Title.ToLower().Contains(query) || (track.Artist != null && track.Artist.ToLower().Contains(query));
        }

        private void LocalCollectionView_OnLoaded(object sender, RoutedEventArgs e)
        {
            LocalSearchBox.Filter = Filter;
        }
    }
}
