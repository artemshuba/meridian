using Meridian.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VkLib;
using VkLib.Core.Auth;

namespace Meridian.WinUI.Controls
{
    public class WebValidationView
    {
        private static TaskCompletionSource<VkAccessToken> _tcs;

        public static Task<VkAccessToken> ValidateAsync(Uri redirectUrl, Uri callbackUri)
        {
            _tcs = new TaskCompletionSource<VkAccessToken>();

            var webView = new WebView2();

            webView.NavigationStarting += WebView_NavigationStarting;
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.Source = redirectUrl;

            PopupControl.Show(webView);

            return _tcs.Task;
        }

        private static void WebView_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            Debug.WriteLine("Navigating " + args.Uri);

            //ProgressBar.Visibility = Visibility.Visible;
            //Browser.Visibility = Visibility.Collapsed;

            var token = Ioc.Resolve<Vk>().OAuth.ProcessAuth(new Uri(args.Uri));
            if (token != null && token.AccessToken != null && !string.IsNullOrEmpty(token.AccessToken.Token))
            {
                token.AccessToken.ExpiresIn = DateTime.MaxValue;
                _tcs.SetResult(token.AccessToken);
                PopupControl.CloseCurrent();
                //Close(token.AccessToken);
            }
        }

        private static void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {

        }
    }
}
