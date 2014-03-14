using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Meridian.Model;

namespace Meridian.View
{
    /// <summary>
    /// Interaction logic for TrackNotifcationView.xaml
    /// </summary>
    public partial class TrackNotifcationView : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private Audio _track;

        public Audio Track
        {
            get { return _track; }
            set
            {
                if (_track == value)
                    return;

                _track = value;
                ResetTimer();
                OnPropertyChanged("Track");
            }
        }

        public TrackNotifcationView(Audio track)
        {
            _track = track;

            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(7);
            _timer.Tick += _timer_Tick;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private new void Close()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;

            var s = (Storyboard)this.Resources["CloseAnim"];
            s.Begin(this);
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void TrackNotifcationView_OnSourceInitialized(object sender, EventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Width - this.Width - 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 10;

            _timer.Start();
           
            var s = (Storyboard)this.Resources["LoadAnim"];
            s.Begin(this);
        }


        void _timer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void CloseAnim_OnCompleted(object sender, EventArgs e)
        {
            base.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TrackNotifcationView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;

            Application.Current.MainWindow.Activate();

            Close();
        }
    }
}
