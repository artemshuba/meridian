using Meridian.Layout;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for MusicView.xaml
    /// </summary>
    public partial class MusicView : PageBase
    {
        private MusicViewModel _viewModel;

        public MusicView()
        {
            InitializeComponent();

            _viewModel = new MusicViewModel();
            this.DataContext = _viewModel;
        }
    }
}
