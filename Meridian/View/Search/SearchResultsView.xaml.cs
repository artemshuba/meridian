using Meridian.Layout;
using Meridian.ViewModel.Search;

namespace Meridian.View.Search
{
    /// <summary>
    /// Interaction logic for SearchResultsView.xaml
    /// </summary>
    public partial class SearchResultsView : PageBase
    {
        private SearchViewModel _viewModel;

        public SearchResultsView()
        {
            InitializeComponent();

            _viewModel = new SearchViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            var query = (string)NavigationContext.Parameters["query"];
            _viewModel.Query = query;
        }
    }
}
