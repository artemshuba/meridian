using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.ViewModel;
using Neptune.UI.Extensions;
using VkLib.Core.Attachments;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for TellFriendsRequestView.xaml
    /// </summary>
    public partial class TellFriendsRequestView : UserControl
    {
        public TellFriendsRequestView()
        {
            InitializeComponent();
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

        private void TellButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
            Tell();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Tell()
        {
            try
            {
                var posId = await ViewModelLocator.Vkontakte.Wall.Post(message: MainResources.AboutTellMessage, attachments:
                    new[] { new VkLinkAttachment() { Url = "http://meridianvk.com" } });

                if (posId != 0)
                {
                    var flyout = new FlyoutControl();
                    flyout.FlyoutContent = new TellResultView(posId);
                    flyout.Show();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }
    }
}
