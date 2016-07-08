using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Compact
{
    /// <summary>
    /// Interaction logic for CompactView.xaml
    /// </summary>
    public partial class CompactView : Window, INotifyPropertyChanged
    {
        private NowPlayingViewModel _viewModel;
        private bool _showTracklist;

        public bool ShowTracklist
        {
            get { return _showTracklist; }
            set
            {
                if (_showTracklist == value)
                    return;

                _showTracklist = value;
                OnPropertyChanged("ShowTracklist");
            }
        }

        public CompactView()
        {
            InitializeComponent();

            _viewModel = new NowPlayingViewModel();
            this.DataContext = _viewModel;
        }

        private void CompactView_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.GetPosition(this).Y < 30)
            {
                DragMove();
                Domain.Settings.Instance.CompactTop = Top;
                Domain.Settings.Instance.CompactLeft = Left;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CompactView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Top = Domain.Settings.Instance.CompactTop;
            Left = Domain.Settings.Instance.CompactLeft;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ViewModelLocator.Main.Volume -= 5;
                if (ViewModelLocator.Main.Volume < 0)
                    ViewModelLocator.Main.Volume = 0;
            }
            else
            {
                ViewModelLocator.Main.Volume += 5;
                if (ViewModelLocator.Main.Volume > 100)
                    ViewModelLocator.Main.Volume = 100;
            }
        }

        private void CompactView_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Domain.Settings.Instance.EnableTrayIcon)
            {
                Hide();
                Visibility = Visibility.Collapsed;
            }
        }
    }
}
