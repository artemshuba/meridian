using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
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

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            LoggingService.Log("Meridian v" + Assembly.GetExecutingAssembly().GetName().Version + " started. OS: " + Environment.OSVersion);

            //DispatcherHelper.Initialize();

            Settings.Load();

            if (Settings.Instance.SendStats)
            {
                YandexMetricaFolder.SetCurrent(Directory.GetCurrentDirectory());
                YandexMetrica.Activate("60fb8ba9-ab3c-4ee8-81ac-559c8aeb305e"); //Yandex Metrica
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Settings.Instance.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (Settings.Instance.Accounts.Count == 0)
            {
                Settings.Instance.Accounts.Add(new Account() { Id = "vk", Title = MainResources.SettingsAccountsVk });
                Settings.Instance.Accounts.Add(new Account() { Id = "lasfm", Title = MainResources.SettingsAccountsLastFm });
            }

            ServiceLocator.DataBaseService.Initialize();

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
                case "Graphite":
                case "Accent":
                    Resources.MergedDictionaries[1].Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Settings.Instance.Theme), UriKind.Relative);
                    break;

                default:
                    Resources.MergedDictionaries[1].Source = new Uri("/Resources/Themes/Light.xaml", UriKind.Relative);
                    break;
            }

            if (Settings.Instance.EnableTrayIcon)
                AddTrayIcon();

            ViewModelLocator.Vkontakte.UseHttps = Settings.Instance.UseHttps;

            AudioService.Load();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            RemoveTrayIcon();

            AudioService.Save();
            AudioService.Dispose();
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

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            var closeItem = new System.Windows.Forms.ToolStripMenuItem();
            closeItem.Text = MainResources.Close;
            closeItem.Click += (s, e) =>
            {
                foreach (Window window in Windows)
                {
                    window.Close();
                }
            };
            _trayIcon.ContextMenuStrip.Items.Add(closeItem);
        }

        private void TrayIconOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            foreach (Window window in Windows)
            {
                if (window.Visibility == Visibility.Collapsed)
                {
                    window.Visibility = Visibility.Visible;
                    window.Show();
                }

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
