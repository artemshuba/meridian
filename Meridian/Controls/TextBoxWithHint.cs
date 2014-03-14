using System.Windows;
using System.Windows.Controls;

namespace Meridian.Controls
{
    public class TextBoxWithHint : TextBox
    {
        private ContentControl _hintContent;

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(object), typeof(TextBoxWithHint), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty HintStyleProperty =
            DependencyProperty.Register("HintStyle", typeof(Style), typeof(TextBoxWithHint), new PropertyMetadata(default(Style)));

        public Style HintStyle
        {
            get { return (Style)GetValue(HintStyleProperty); }
            set { SetValue(HintStyleProperty, value); }
        }

        public object Hint
        {
            get { return GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public TextBoxWithHint()
        {
            DefaultStyleKey = typeof(TextBoxWithHint);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _hintContent = GetTemplateChild("HintContent") as ContentControl;
            if (_hintContent != null)
                DetermineHintVisibility();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (_hintContent != null && string.IsNullOrEmpty(this.Text))
            {
                _hintContent.Visibility = Visibility.Collapsed;
            }

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (_hintContent != null && string.IsNullOrEmpty(this.Text))
            {
                _hintContent.Visibility = Visibility.Visible;
            }

            base.OnLostFocus(e);
        }

        private void DetermineHintVisibility()
        {
            if (string.IsNullOrEmpty(this.Text) && !IsFocused)
            {
                _hintContent.Visibility = Visibility.Visible;
            }
            else
            {
                _hintContent.Visibility = Visibility.Collapsed;
            }
        }
    }
}
