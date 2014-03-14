using System.Windows;
using System.Windows.Controls;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for PlayerBarControl.xaml
    /// </summary>
    public partial class PlayerBarControl : UserControl
    {
        public PlayerBarControl()
        {
            InitializeComponent();
        }

        private void CurrentAudioButton_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentAudioPopup.IsOpen = true;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentAudioPopup.IsOpen = false;
        }
    }
}
