using Meridian.ViewModel.VK;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Meridian.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyMusicView : Page
    {
        public MyMusicViewModel ViewModel => DataContext as MyMusicViewModel;

        public MyMusicView()
        {
            this.InitializeComponent();
        }
    }
}