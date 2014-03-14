using System.Windows.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for RadioView.xaml
    /// </summary>
    public partial class RadioView : Page
    {
        private RadioViewModel _viewModel;

        public RadioView()
        {
            InitializeComponent();

            _viewModel = new RadioViewModel();
            this.DataContext = _viewModel;
        }
    }
}
