using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.Controls
{
    public sealed partial class ImageTransitionControl : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
                    "Source", typeof(ImageSource), typeof(ImageTransitionControl), new PropertyMetadata(default(ImageSource), OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newImage = e.NewValue as BitmapImage;
            var oldImage = e.OldValue as BitmapImage;

            if (newImage == oldImage)
                return;

            var control = (ImageTransitionControl)d;

            if (control.Image.Source != null)
                control.FadeOut();
            else
            {
                control.Image.Source = newImage;
                control.FadeIn();
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch", typeof(Stretch), typeof(ImageTransitionControl), new PropertyMetadata(default(Stretch), OnStretchPropertyChanged));

        private static void OnStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageTransitionControl)d;
            control.Image.Stretch = (Stretch)e.NewValue;
        }

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransitionDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register("TransitionDuration", typeof(TimeSpan), typeof(ImageTransitionControl), new PropertyMetadata(TimeSpan.FromSeconds(0.15), (d, e) =>
            {
                var control = (ImageTransitionControl)d;
                var newValue = (TimeSpan)e.NewValue;

                var fadeInAnim = (Storyboard)control.Resources["FadeInAnim"];
                fadeInAnim.Children[0].Duration = newValue;

                var fadeOutAnim = (Storyboard)control.Resources["FadeOutAnim"];
                fadeOutAnim.Children[0].Duration = newValue;
            }));

        public TimeSpan TransitionDuration
        {
            get { return (TimeSpan)GetValue(TransitionDurationProperty); }
            set { SetValue(TransitionDurationProperty, value); }
        }

        public ImageTransitionControl()
        {
            this.InitializeComponent();
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs e)
        {
            FadeIn();
        }

        private void FadeOut()
        {
            var s = (Storyboard)Resources["FadeOutAnim"];
            s.Begin();
        }

        private void FadeIn()
        {
            var s = (Storyboard)Resources["FadeInAnim"];
            s.Begin();
        }

        private void ImageFadeOutAnim_OnCompleted(object sender, object e)
        {
            Image.Source = Source;

            //if it's a local image, fade in immediately
            if ((Source as BitmapImage)?.UriSource == null)
                FadeIn();
        }
    }
}
