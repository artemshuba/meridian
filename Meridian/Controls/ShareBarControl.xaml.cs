using System.Windows.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Flyouts;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for ShareBarControl.xaml
    /// </summary>
    public partial class ShareBarControl : UserControl
    {
        private ShareViewModel _viewModel;

        public ShareBarControl()
        {
            InitializeComponent();

            _viewModel = new ShareViewModel();
            rootGrid.DataContext = _viewModel;
        }
    }
}
