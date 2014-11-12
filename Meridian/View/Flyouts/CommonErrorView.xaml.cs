using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for CommonErrorView.xaml
    /// </summary>
    public partial class CommonErrorView : UserControl
    {
        public CommonErrorView()
        {
            InitializeComponent();
        }

        public CommonErrorView(string title, string description) : this()
        {
            TitleTextBlock.Text = title;
            DescriptionTextBlock.Text = description;
        }

        private void RestartButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close(true, true);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close(bool now = false, bool restart = false)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
            {
                if (now)
                    flyout.CloseNow(restart);
                else
                    flyout.Close(restart);
            }
        }
    }
}
