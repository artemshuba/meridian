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
        }

        private void AnimateIn()
        {
            var s = (Storyboard)Resources["TransitionIn"];

            s.Begin(_newImage);
        }
    }
}
