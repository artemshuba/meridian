using System.Windows;
using Meridian.Layout;
using Meridian.Model;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for PopularAudioView.xaml
    /// </summary>
    public partial class PopularAudioView : PageBase
    {
        private PopularAudioViewModel _viewModel;

        public PopularAudioView()
        {
            InitializeComponent();

            _viewModel = new PopularAudioViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();

            LocalSearchBox.Filter = Filter;
        }

        private bool Filter(object o)
        {
            var track = (Audio)o;
            var query = LocalSearchBox.Query.ToLower();
            return track.Title.ToLower().StartsWith(query) || track.Artist.ToLower().StartsWith(query);
        }

        private void LocalSearchItem_OnClick(object sender, RoutedEventArgs e)
        {
            LocalSearchBox.IsActive = true;
        }
    }
}
