using Meridian.Layout;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for FeedView.xaml
    /// </summary>
    public partial class FeedView : PageBase
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
