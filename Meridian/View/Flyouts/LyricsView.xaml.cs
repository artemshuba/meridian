using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.Flyouts;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for LyricsView.xaml
    /// </summary>
    public partial class LyricsView : UserControl
    {
        private LyricsViewModel _viewModel;

        public LyricsView(VkAudio audio)
        {
            InitializeComponent();

            _viewModel = new LyricsViewModel();
            this.DataContext = _viewModel;
            _viewModel.Track = audio;
        }

        private void Close(bool now = false)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
            {
                if (now)
                    flyout.CloseNow();
                else
                    flyout.Close();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
