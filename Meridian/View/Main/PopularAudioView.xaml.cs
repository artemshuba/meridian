using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for PopularAudioView.xaml
    /// </summary>
    public partial class PopularAudioView : Page
    {
        private PopularAudioViewModel _viewModel;

        public PopularAudioView()
        {
            InitializeComponent();

            _viewModel = new PopularAudioViewModel();
            this.DataContext = _viewModel;
        }

        private void PopularAudioView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Activate();

            LocalSearchBox.Filter = Filter;
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
