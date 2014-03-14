using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for CommonMessageView.xaml
    /// </summary>
    public partial class CommonMessageView : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof (string), typeof (CommonMessageView), new PropertyMetadata(default(string)));

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof (string), typeof (CommonMessageView), new PropertyMetadata(default(string)));

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public CommonMessageView()
        {
            InitializeComponent();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
