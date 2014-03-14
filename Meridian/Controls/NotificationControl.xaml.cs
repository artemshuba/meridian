using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for NotificationControl.xaml
    /// </summary>
    public partial class NotificationControl : UserControl
    {
        private DispatcherTimer _timer;
        private bool _isStatusVisible;

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
          "Status", typeof(string), typeof(NotificationControl), new PropertyMetadata(null, OnNotificationChanged));

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", typeof (int), typeof (NotificationControl), new PropertyMetadata(default(int), ProgressChanged));

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NotificationControl) d;
            if ((int)e.NewValue >= 100)
                control.ProgressBar.Visibility = Visibility.Collapsed;
            else
                control.ProgressBar.Visibility = Visibility.Visible;
        }

        public int Progress
        {
            get { return (int) GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set
            {
                SetValue(StatusProperty, value);
            }
        }

        public NotificationControl()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            //UpdateStatus(string.Empty);
            Status = string.Empty;

            _timer.Stop();

            var outAnim = (Storyboard)Resources["SwitchOutAnim"];
            outAnim.Begin(this);
        }

        private void UpdateStatus(string newValue)
        {
            //var outAnim = (Storyboard)Resources["SwitchOutAnim"];
            //outAnim.Begin(this);

            var inAnim = (Storyboard)Resources["SwitchInAnim"];
            inAnim.Begin();
        }

        private static void OnNotificationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var control = (NotificationControl)o;

            if (e.NewValue != null)
                control.UpdateStatus(e.NewValue.ToString());
            //else if (e.NewValue == null)
            //    control.UpdateStatus(string.Empty);
        }

        private void SwitchOutAnim_OnCompleted(object sender, EventArgs e)
        {
            //this.StatusTextBlock.Text = Status;

            //var inAnim = (Storyboard)Resources["SwitchInAnim"];
            //inAnim.Begin(this);

            //if (!string.IsNullOrEmpty(_newValue))
            //{
            //    _timer.Stop();
            //    _timer.Start();
            //}
        }

        private void SwitchInAnim_OnCompleted(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Status))
            {
                _timer.Stop();
                _timer.Start();
            }
        }
    }
}
