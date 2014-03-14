using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Meridian.ViewModel;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for TellResultView.xaml
    /// </summary>
    public partial class TellResultView : UserControl
    {
        private readonly long _postId;

        public TellResultView(long postId)
        {
            _postId = postId;

            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GoToPostButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = "http://vk.com/wall" + ViewModelLocator.Vkontakte.AccessToken.UserId + "_" + _postId;
            Process.Start(url);

            Close();
        }

        private void Close()
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
            {
                flyout.Close();
            }
        }
    }
}
