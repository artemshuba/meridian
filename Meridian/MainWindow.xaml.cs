using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Helpers;
using Meridian.Services;
using Meridian.View.Flyouts;
using Meridian.View.Main;
using Meridian.ViewModel;
using Neptune.Messages;
using Neptune.UI.Extensions;

namespace Meridian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr _windowHandle;
        private bool _clearStack;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.GetPosition(this).Y < 30)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Top = -e.GetPosition(this).Y / 2;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    DragMove();


                    ViewModelLocator.Main.WindowLeft = Left;
                    ViewModelLocator.Main.WindowTop = Top;
                }
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;

            RootFrame.Navigated += RootFrame_Navigated;
            RootFrame.Navigating += RootFrame_Navigating;

            Top = Settings.Instance.Top;
            Left = Settings.Instance.Left;
            Width = Settings.Instance.Width;
            Height = Settings.Instance.Height;

            if (Settings.Instance.AccessToken != null && !Settings.Instance.AccessToken.HasExpired)
            {
                ViewModelLocator.Vkontakte.AccessToken = Settings.Instance.AccessToken;

                ViewModelLocator.Main.ShowSidebar = true;

                Messenger.Default.Send(new NavigateToPageMessage()
                {
                    Page = "/Main.MusicView"
                });

                ViewModelLocator.Main.LoadUserInfo();

                if (!Settings.Instance.TellRequestShown && (DateTime.Now - Settings.Instance.FirstStart).TotalDays >= 3)
                {
                    Settings.Instance.TellRequestShown = true;
                    Settings.Instance.Save();
                    TellFriendsRequest();
                }
            }
            else
            {
                Messenger.Default.Send(new NavigateToPageMessage()
                {
                    Page = "/Main.LoginView"
                });
            }

            ViewModelLocator.Main.Initialize();
            NotificationService.Initialize(NotificationControl);

            BackgroundArtControl.Effect = Settings.Instance.BlurBackground ? new BlurEffect() { RenderingBias = RenderingBias.Quality, Radius = 35} : null;
        }

        void RootFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (RootFrame.Content is LoginView)
            {
                _clearStack = true;
            }
        }

        void RootFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_clearStack)
            {
                _clearStack = false;

                while (RootFrame.CanGoBack)
                {
                    RootFrame.RemoveBackEntry();
                }
            }

            ViewModelLocator.Main.UpdateCanGoBack();
        }

        private void ResizeGripMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                NativeMethods.SizeWindow(_windowHandle);

                ViewModelLocator.Main.WindowWidth = Width;
                ViewModelLocator.Main.WindowHeight = Height;
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Settings.Instance.Save();
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(SearchBox.Text))
            //{
            //    ViewModelLocator.Main.SearchCommand.Execute(SearchBox.Text);
            //}
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var flyout = RootGrid.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
                if (flyout != null)
                    flyout.Close();
            }
        }

        private void TellFriendsRequest()
        {
            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new TellFriendsRequestView();
            flyout.Show();
        }
    }
}
