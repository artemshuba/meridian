using System.Windows;
using Meridian.Layout;
using Meridian.Model;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for MusicView.xaml
    /// </summary>
    public partial class MusicView : PageBase
    {
        private MusicViewModel _viewModel;

        public MusicView()
        {
            InitializeComponent();

            _viewModel = new MusicViewModel();
            this.DataContext = _viewModel;

            LocalSearchBox.Filter = Filter;
        }

        private void LocalSearchItem_OnClick(object sender, RoutedEventArgs e)
        {
            LocalSearchBox.IsActive = true;
        }

        private bool Filter(object o)
        {
            var track = (Audio)o;
            var query = LocalSearchBox.Query.ToLower();
            return track.Title.ToLower().Contains(query) || track.Artist.ToLower().Contains(query);
        }
    }
}
