using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Controls;
using Meridian.ViewModel;
using Neptune.Messages;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for LoginLastFmMessageView.xaml
    /// </summary>
    public partial class LoginLastFmMessageView : UserControl
    {
        public LoginLastFmMessageView()
        {
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            Close(true);

            Messenger.Default.Send(new NavigateToPageMessage()
            {
                Page = "/Settings.SettingsView",
                Parameters = new Dictionary<string, object>()
                {
                    {"section", "accounts"}
                }
            });

            ViewModelLocator.Main.ShowSidebar = false;
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
