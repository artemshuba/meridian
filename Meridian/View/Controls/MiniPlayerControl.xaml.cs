using Meridian.View.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.View.Controls
{
    public sealed partial class MiniPlayerControl : UserControl
    {
        public MiniPlayerControl()
        {
            this.InitializeComponent();
        }

        private void NowPlayingButtonClick(object sender, RoutedEventArgs e)
        {
            //App.Current.NavigationService.Navigate(typeof(NowPlayingView));
        }
    }
}