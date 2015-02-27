using System.Windows;
using Meridian.Layout;
using Meridian.ViewModel.People;

namespace Meridian.View.People
{
    /// <summary>
    /// Interaction logic for SubscriptionsView.xaml
    /// </summary>
    public partial class SubscriptionsView : PageBase
    {
        private SubscriptionsViewModel _viewModel;

        public SubscriptionsView()
        {
            InitializeComponent();

            _viewModel = new SubscriptionsViewModel();
            this.DataContext = _viewModel;
        }

        private void SubscriptionsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Activate();
        }
    }
}
