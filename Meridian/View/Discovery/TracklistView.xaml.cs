using Meridian.ViewModel.Discovery;
using Microsoft.UI.Xaml.Controls;

namespace Meridian.View.Discovery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TracklistView : Page
    {
        public TracklistViewModel ViewModel => DataContext as TracklistViewModel;

        public TracklistView()
        {
            this.InitializeComponent();
        }
    }
}