using System.Windows.Controls;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.Flyouts;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for EditAudioView.xaml
    /// </summary>
    public partial class EditAudioView : UserControl
    {
        private EditAudioViewModel _viewModel;

        public EditAudioView(VkAudio audio)
        {
            InitializeComponent();

            _viewModel = new EditAudioViewModel();
            this.DataContext = _viewModel;
            _viewModel.Track = audio;
        }
    }
}
