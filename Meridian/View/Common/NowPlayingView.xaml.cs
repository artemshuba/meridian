using Meridian.ViewModel.Common;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Meridian.View.Common
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingView : Page
    {
        public NowPlayingViewModel ViewModel => DataContext as NowPlayingViewModel;

        public NowPlayingView()
        {
            this.InitializeComponent();
        }
    }
}
