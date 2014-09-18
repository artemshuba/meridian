using System.Windows.Controls;
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
    }
}
