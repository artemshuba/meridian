using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View;
using Meridian.View.Flyouts;
using Meridian.ViewModel;
using Yandex.Metrica;
using Application = System.Windows.Application;

namespace Meridian
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon _trayIcon;

        public static readonly string Root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LoggingService.Log("Meridian v" + Assembly.GetExecutingAssembly().GetName().Version + " started. OS: " + Environment.OSVersion);

            DispatcherHelper.Initialize();

            Settings.Load();

            if (Settings.Instance.SendStats)
            {
                Counter.Start(19168); //Yandex Metrica
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Settings.Instance.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;


            if (Settings.Instance.NeedClean)
            {
                ViewModelLocator.UpdateService.Clean();

                Settings.Instance.NeedClean = false;
            }

            switch (Settings.Instance.AccentColor)
            {
                case "Red":
                case "Emerald":
                case "Magenta":
                case "Mango":
                case "Sea":
                case "Sky":
                case "Purple":
                case "Pink":
                    Resources.MergedDictionaries[0].Source = new Uri(string.Format("/Resources/Themes/Accents/{0}.xaml", Settings.Instance.AccentColor), UriKind.Relative);
                    break;

                default:
                    Resources.MergedDictionaries[0].Source = new Uri("/Resources/Themes/Accents/Blue.xaml", UriKind.Relative);
                    break;
            }

            switch (Settings.Instance.Theme)
            {
                case "Light":
                case "Dark":
                    Resources.MergedDictionaries[1].Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Settings.Instance.Theme), UriKind.Relative);
                    break;

                default:
                    Resources.MergedDictionaries[1].Source = new Uri("/Resources/Themes/Light.xaml", UriKind.Relative);
                    break;
            }

            if (Settings.Instance.CheckForUpdates)
                ViewModelLocator.UpdateService.CheckUpdates();

            if (Settings.Instance.EnableTrayIcon)
                AddTrayIcon();

            AudioService.Load();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            RemoveTrayIcon();

            AudioService.Save();

            if (Settings.Instance.SendStats)
            {
                Counter.ReportExit();
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LoggingService.Log(e.Exception);

            Dispatcher.Invoke(async () =>
            {
                e.Handled = true;

                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new CommonErrorView();
                var restart = (bool)await flyout.ShowAsync();
                if (restart)
                {
                    Process.Start(Application.ResourceAssembly.Location);
                }

                Shutdown();
            });

        }

        public void AddTrayIcon()
        {
            if (_trayIcon != null)
            {
                return;
            }

            _trayIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = "Meridian " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2)
            };
            _trayIcon.MouseClick += TrayIconOnMouseClick;
            _trayIcon.Visible = true;

            _trayIcon.ContextMenu = new ContextMenu();
            var closeItem = new System.Windows.Forms.MenuItem();
            closeItem.Text = MainResources.Close;
            closeItem.Click += (s, e) =>
            {
                foreach (Window window in Windows)
                {
                    window.Close();
                }
            };
            _trayIcon.ContextMenu.MenuItems.Add(closeItem);
        }

        private void TrayIconOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            foreach (Window window in Windows)
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
            }
        }

        public void RemoveTrayIcon()
        {
            if (_trayIcon != null)
            {
                _trayIcon.MouseClick -= TrayIconOnMouseClick;
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        }
    }
}
