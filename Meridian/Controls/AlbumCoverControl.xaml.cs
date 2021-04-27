using Meridian.Model;
using Meridian.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.Controls
{
    public sealed partial class AlbumCoverControl : UserControl
    {
        private Image _bottomImage;
        private Image _topImage;

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(CachedImage), typeof(AlbumCoverControl), new PropertyMetadata(ImageService.DefaultTrackCover, (d, e) =>
            {
                var control = (AlbumCoverControl)d;

                if (e.NewValue == e.OldValue)
                    return;

                var newImage = e.NewValue as CachedImage;
                var oldImage = e.OldValue as CachedImage;

                if (newImage?.Key == oldImage?.Key)
                    return;

                if (newImage?.Source == null)
                {
                    control.SetImage(ImageService.DefaultTrackCover);
                    return;
                }

                control.SetImage(newImage);
            }));

        public CachedImage Source
        {
            get { return (CachedImage)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public AlbumCoverControl()
        {
            this.InitializeComponent();

            _bottomImage = Image1;
            _topImage = Image2;
        }

        private void SetImage(CachedImage imageSource)
        {
           _topImage.Source = imageSource?.Source;

            var fadeInAnim = (Storyboard)Resources["FadeInAnim"];
            fadeInAnim.Stop();
            Storyboard.SetTarget(fadeInAnim, _topImage);
            fadeInAnim.Begin();

            _topImage.Visibility = Visibility.Visible;

            var fadeOutAnim = (Storyboard)Resources["FadeOutAnim"];
            fadeOutAnim.Stop();
            Storyboard.SetTarget(fadeOutAnim, _bottomImage);
            fadeOutAnim.Begin();
        }

        private void FadeOutAnimCompleted(object sender, object e)
        {
            _bottomImage.Visibility = Visibility.Collapsed;
            _bottomImage.Source = null;

            //swap images

            var index1 = (uint)RootGrid.Children.IndexOf(_topImage);
            var index2 = (uint)RootGrid.Children.IndexOf(_bottomImage);

            RootGrid.Children.Move(index1, index2);

            var x = _topImage;
            _topImage = _bottomImage;
            _bottomImage = x;
        }

        private void FadeInAnimCompleted(object sender, object e)
        {

        }
    }
}
