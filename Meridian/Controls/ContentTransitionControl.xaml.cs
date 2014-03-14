using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for ContentTransitionControl.xaml
    /// </summary>
    public partial class ContentTransitionControl : UserControl
    {
        private ContentControl _currentControl;
        private ContentControl _newControl;

        public new static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentTransitionControl), new PropertyMetadata(default(DataTemplate), ContentTemplatePropertyChanged));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(ContentTransitionControl), new PropertyMetadata(default(object), SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ContentTransitionControl)d;

            control._currentControl.Content = e.NewValue;

            control.Swap();

            if (e.OldValue != null)
                control.AnimateOut();

            if (e.NewValue != null)
                control.AnimateIn();
        }

        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void ContentTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ContentTransitionControl)d;

            control._currentControl.ContentTemplate = (DataTemplate)e.NewValue;
            control._newControl.ContentTemplate = (DataTemplate)e.NewValue;
        }

        public new DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        public ContentTransitionControl()
        {
            InitializeComponent();

            _currentControl = Control1;
            _newControl = Control2;
        }

        private void Swap()
        {
            var x = _currentControl;
            _currentControl = _newControl;
            _newControl = x;
        }

        private void AnimateOut()
        {
            var s = (Storyboard)Resources["TransitionOut"];
            s.Begin(_currentControl);
        }

        private void AnimateIn()
        {
            var s = (Storyboard)Resources["TransitionIn"];
            s.Begin(_newControl);
        }
    }
}
