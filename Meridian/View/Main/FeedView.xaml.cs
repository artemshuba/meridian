using System.Windows.Controls;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for FeedView.xaml
    /// </summary>
    public partial class FeedView : Page
    {
        private FeedViewModel _viewModel;

        public FeedView()
        {
            InitializeComponent();

            _viewModel = new FeedViewModel();
            this.DataContext = _viewModel;
        }
    }
}
