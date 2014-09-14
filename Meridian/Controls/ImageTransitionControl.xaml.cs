using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for ImageTransitionControl.xaml
    /// </summary>
    public partial class ImageTransitionControl : UserControl
    {
        private Image _currentImage;
        private Image _newImage;

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageTransitionControl), new PropertyMetadata(default(ImageSource), ImageSourcePropertyChanged));

        private static void ImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageTransitionControl)d;

            control.Swap();

            control._newImage.Source = (ImageSource)e.NewValue;

            if (e.OldValue != null)
                control.AnimateOut();

            if (e.NewValue != null)
            {
                control.AnimateIn();
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty ImageOpacityProperty =
            DependencyProperty.Register("ImageOpacity", typeof(double), typeof(ImageTransitionControl), new PropertyMetadata(default(double), ImageOpacityPropertyChanged));

        private static void ImageOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageTransitionControl)d;

            if (e.NewValue != null)
            {
                var storyboard = (Storyboard)control.Resources["TransitionIn"];
                var anim = (DoubleAnimationUsingKeyFrames)storyboard.Children[0];
                var keyFrame = anim.KeyFrames[1];
                keyFrame.Value = (double)e.NewValue;
            }
        }

        public double ImageOpacity
        {
            get { return (double)GetValue(ImageOpacityProperty); }
            set { SetValue(ImageOpacityProperty, value); }
        }


        public static readonly DependencyProperty ImageBackgroundProperty =
            DependencyProperty.Register("ImageBackground", typeof(Brush), typeof(ImageTransitionControl), new PropertyMetadata(default(Brush)));


        public Brush ImageBackground
        {
            get { return (Brush)GetValue(ImageBackgroundProperty); }
            set { SetValue(ImageBackgroundProperty, value); }
        }

        public ImageTransitionControl()
        {
            InitializeComponent();

            _currentImage = Image1;
            _newImage = Image2;
        }

        private void Swap()
        {
            var x = _currentImage;
            _currentImage = _newImage;
            _newImage = x;
        }

        private void AnimateOut()
        {
            var s = (Storyboard)Resources["TransitionOut"];

            s.Begin(_currentImage);

            s = (Storyboard)Resources["BgTransitionOut"];

            s.Begin();
        }

        private void AnimateIn()
        {
            var s = (Storyboard)Resources["TransitionIn"];

            s.Begin(_newImage);

            s = (Storyboard)Resources["BgTransitionIn"];

            s.Begin();
        }
    }
}
