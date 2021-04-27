using Meridian.ViewModel.VK;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Meridian.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchView : Page
    {
        public SearchViewModel ViewModel => DataContext as SearchViewModel;

        public SearchView()
        {
            this.InitializeComponent();
        }
    }
}