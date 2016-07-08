using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.Layout;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for RecommendationsView.xaml
    /// </summary>
    public partial class RecommendationsView : PageBase
    {
        private RecommendationsViewModel _viewModel;

        public RecommendationsView()
        {
            InitializeComponent();

            _viewModel = new RecommendationsViewModel();
            this.DataContext = _viewModel;
        }

        private void RecommendationsView_OnLoaded(object sender, RoutedEventArgs e)
        {
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
            return track.Title.ToLower().StartsWith(query) || track.Artist.ToLower().StartsWith(query);
        }
    }
}
