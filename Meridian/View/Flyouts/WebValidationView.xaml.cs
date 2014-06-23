using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Meridian.Controls;
using Meridian.ViewModel;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for WebValidationView.xaml
    /// </summary>
    public partial class WebValidationView : UserControl
    {
        private Uri _redirectUri;

        public WebValidationView(Uri redirectUri)
        {
            InitializeComponent();

            _redirectUri = redirectUri;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebValidationView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate(_redirectUri);
        }


        private void Close(object result = null, bool now = false)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
            {
                if (now)
                    flyout.CloseNow(result);
                else
                    flyout.Close(result);
            }
        }

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            Browser.Visibility = Visibility.Visible;
        }

        private void Browser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            Browser.Visibility = Visibility.Collapsed;

            var token = ViewModelLocator.Vkontakte.OAuth.ProcessAuth(e.Uri);
            if (token != null && token.AccessToken != null && !string.IsNullOrEmpty(token.AccessToken.Token))
            {
                token.AccessToken.ExpiresIn = DateTime.MaxValue;
                Close(token.AccessToken);
            }
        }
    }
}
