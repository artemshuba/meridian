using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Jupiter.Controls
{
    /// <summary>
    /// Control that shows a progress of operation or error if it's failed.
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Busy", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Error", GroupName = "CommonStates")]
    public class LoadingIndicator : ContentControl
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(LoadingIndicator), new PropertyMetadata(default(bool), OnIsBusyPropertyChanged));

        private static void OnIsBusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LoadingIndicator)d;
            control.UpdateVisualState();
        }

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(string), typeof(LoadingIndicator), new PropertyMetadata(default(string), OnErrorPropertyChanged));

        private static void OnErrorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LoadingIndicator)d;
            control.UpdateVisualState();
        }

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public static readonly DependencyProperty LoadingContentStyleProperty = DependencyProperty.Register(
            "LoadingContentStyle", typeof(Style), typeof(LoadingIndicator), new PropertyMetadata(default(Style)));

        public Style LoadingContentStyle
        {
            get { return (Style)GetValue(LoadingContentStyleProperty); }
            set { SetValue(LoadingContentStyleProperty, value); }
        }

        public static readonly DependencyProperty ErrorContentStyleProperty = DependencyProperty.Register(
            "ErrorContentStyle", typeof (Style), typeof (LoadingIndicator), new PropertyMetadata(default(Style)));

        public Style ErrorContentStyle
        {
            get { return (Style) GetValue(ErrorContentStyleProperty); }
            set { SetValue(ErrorContentStyleProperty, value); }
        }

        public LoadingIndicator()
        {
            DefaultStyleKey = typeof(LoadingIndicator);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (string.IsNullOrEmpty(Error))
            {
                if (IsBusy)
                {
                    VisualStateManager.GoToState(this, "Busy", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Normal", true);
                }
            }
            else
                VisualStateManager.GoToState(this, "Error", true);
        }
    }
}