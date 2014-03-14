using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for RecommendationsView.xaml
    /// </summary>
    public partial class RecommendationsView : Page
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
            _viewModel.Activate();
        }

        private void RecommendationsView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Deactivate();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }
    }
}
