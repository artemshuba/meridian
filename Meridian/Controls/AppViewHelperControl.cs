using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Meridian.Controls
{
    public class AppViewHelperControl : Control
    {
        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppViewForegroundProperty =
            DependencyProperty.Register("AppViewForeground", typeof(Brush), typeof(AppViewHelperControl), new PropertyMetadata(null, (d, e) =>
            {
                //var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                //appView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)e.NewValue).Color;

            }));

        /// <summary>
        /// Foreground brush
        /// </summary>
        public Brush AppViewForeground
        {
            get { return (Brush)GetValue(AppViewForegroundProperty); }
            set { SetValue(AppViewForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppViewBackgroundProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppViewBackgroundProperty =
            DependencyProperty.Register("AppViewBackground", typeof(Brush), typeof(AppViewHelperControl), new PropertyMetadata(null, (d, e) =>
            {

            }));

        /// <summary>
        /// Background brush
        /// </summary>
        public Brush AppViewBackground
        {
            get { return (Brush)GetValue(AppViewBackgroundProperty); }
            set { SetValue(AppViewBackgroundProperty, value); }
        }
    }
}