using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentAudioMenuPopup.SetValue(Popup.IsOpenProperty, false);
        }
    }
}
