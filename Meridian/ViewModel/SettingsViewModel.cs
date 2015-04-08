using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Model.Settings;
using Meridian.RemotePlay;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.Services.Media.Core;
using Meridian.View.Flyouts;
using VkLib.Core.Attachments;

namespace Meridian.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly Dictionary<string, string> _menuItems = new Dictionary<string, string>()
        {
            {MainResources.SettingsMenuUI, "/View/Settings/SettingsUIView.xaml"},
            {MainResources.SettingsMenuHotkeys, "/View/Settings/SettingsHotkeysView.xaml"},
            {MainResources.SettingsRemotePlay, "/View/Settings/SettingsRemotePlayView.xaml"},
            {MainResources.SettingsMenuAccounts, "/View/Settings/SettingsAccountsView.xaml"},
            {MainResources.SettingsMenuUpdates, "/View/Settings/SettingsUpdatesView.xaml"},
            {MainResources.SettingsMenuAbout, "/View/Settings/SettingsAboutView.xaml"}
        };

        private readonly List<string> _themes = new List<string>()
        {
            "Light", "Dark", "Graphite", "Accent"
        };

        private readonly List<ColorScheme> _colors = new List<ColorScheme>()
                                                         {
                                                             new ColorScheme("Blue", "#006ac1"),
                                                             new ColorScheme("Red", "#e51400"),
                                                             new ColorScheme("Sky", "#1ba1e2"),
                                                             new ColorScheme("Emerald", "#059f01"),
                                                             new ColorScheme("Mango", "#fe6f11"),
                                                             new ColorScheme("Magenta", "#d80073"),
                                                             new ColorScheme("Sea", "#009f9f"),
                                                             new ColorScheme("Purple", "#6800d3"),
                                                             new ColorScheme("Pink", "#e671b8")
                                                         };

        private readonly List<SettingsLanguage> _languages = new List<SettingsLanguage>()
        {
            new SettingsLanguage() {LanguageCode = "en", Title = CultureInfo.GetCultureInfo("en").NativeName},
            new SettingsLanguage() {LanguageCode = "ru", Title = CultureInfo.GetCultureInfo("ru").NativeName}
        };

        private readonly List<SettingsEngine> _engines = new List<SettingsEngine>()
                {
                    new SettingsEngine() { Title = "Windows Media Player", Engine = MediaEngine.Wmp },
                    new SettingsEngine() { Title = "NAudio (experimental)", Engine = MediaEngine.NAudio }
                };

        private readonly List<SettingsHotkey> _hotkeys = new List<SettingsHotkey>();
        private string _selectedTheme;
        private ColorScheme _selectedColorScheme;
        private bool _restartRequired;
        private bool _canSave;
        private bool _checkForUpdates;
        private bool _enableNotifications;
        private bool _enableTrayIcon;
        private bool _showBackgroundArt;
        private bool _showBackgroundArtCompactMode;
        private bool _blurBackground;
        private bool _downloadArtistArt;
        private bool _downloadAlbumArt;
        private SettingsLanguage _selectedLanguage;
        private string _cacheSize;
        private SettingsEngine _selectedEngine;
        private string _selectedRemotePlayAddress;
        private string _remotePlayPort;
        private bool _enableRemotePlay;
        private bool _useHttps;

        #region Commands

        public RelayCommand CloseSettingsCommand { get; private set; }

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand SaveRestartCommand { get; private set; }

        public RelayCommand SignOutVkCommand { get; private set; }

        public RelayCommand LoginLastFmCommand { get; private set; }

        public RelayCommand SignOutLastFmCommand { get; private set; }

        public RelayCommand CheckUpdatesCommand { get; private set; }

        public RelayCommand ClearCacheCommand { get; private set; }

        public RelayCommand TellCommand { get; private set; }

        #endregion

        public Dictionary<string, string> MenuItems
        {
            get { return _menuItems; }
        }

        public string Version
        {
            get
            {
                if (IsInDesignMode)
                    return "5.0.700.0";
                else
                    return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public List<string> Themes
        {
            get { return _themes; }
        }

        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (Set(ref _selectedTheme, value))
                {
                    CanSave = true;
                    //if (value != Domain.Settings.Instance.AccentColor)
                    //    RestartRequired = true;
                }
            }
        }

        public List<ColorScheme> AccentColors
        {
            get { return _colors; }
        }

        public ColorScheme SelectedColorScheme
        {
            get { return _selectedColorScheme; }
            set
            {
                if (Set(ref _selectedColorScheme, value))
                {
                    CanSave = true;
                }
            }
        }

        public bool RestartRequired
        {
            get { return _restartRequired; }
            set { Set(ref _restartRequired, value); }
        }

        public bool CanSave
        {
            get { return _canSave; }
            set { Set(ref _canSave, value); }
        }

        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set
            {
                if (Set(ref _checkForUpdates, value))
                    CanSave = true;
            }
        }

        public bool InstallDevUpdates
        {
            get { return Domain.Settings.Instance.InstallDevUpdates; }
            set { Domain.Settings.Instance.InstallDevUpdates = value; }
        }

        public bool EnableNotifications
        {
            get { return _enableNotifications; }
            set
            {
                if (Set(ref _enableNotifications, value))
                    CanSave = true;
            }
        }

        public bool EnableTrayIcon
        {
            get { return _enableTrayIcon; }
            set
            {
                if (Set(ref _enableTrayIcon, value))
                {
                    CanSave = true;

                    if (value != Domain.Settings.Instance.EnableTrayIcon)
                        RestartRequired = true;
                }
            }
        }

        public bool ShowBackgroundArt
        {
            get { return _showBackgroundArt; }
            set
            {
                if (Set(ref _showBackgroundArt, value))
                {
                    CanSave = true;
                }
            }
        }

        public bool ShowBackgroundArtCompactMode
        {
            get { return _showBackgroundArtCompactMode; }
            set
            {
                if (Set(ref _showBackgroundArtCompactMode, value))
                {
                    CanSave = true;
                }
            }
        }

        public bool BlurBackground
        {
            get { return _blurBackground; }
            set
            {
                if (Set(ref _blurBackground, value))
                {
                    CanSave = true;
                }
            }
        }

        public bool DownloadArtistArt
        {
            get { return _downloadArtistArt; }
            set
            {
                if (Set(ref _downloadArtistArt, value))
                {
                    CanSave = true;
                }
            }
        }

        public bool DownloadAlbumArt
        {
            get { return _downloadAlbumArt; }
            set
            {
                if (Set(ref _downloadAlbumArt, value))
                {
                    CanSave = true;
                }
            }
        }

        public string CacheSize
        {
            get { return _cacheSize; }
            set { Set(ref _cacheSize, value); }
        }

        public List<SettingsHotkey> Hotkeys
        {
            get { return _hotkeys; }
        }

        public List<SettingsLanguage> Languages
        {
            get { return _languages; }
        }

        public SettingsLanguage SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (Set(ref _selectedLanguage, value))
                {
                    CanSave = true;

                    if (value.LanguageCode != Domain.Settings.Instance.Language)
                        RestartRequired = true;
                }
            }
        }

        public List<SettingsEngine> Engines
        {
            get { return _engines; }
        }

        public SettingsEngine SelectedEngine
        {
            get { return _selectedEngine; }
            set
            {
                if (Set(ref _selectedEngine, value))
                {
                    CanSave = true;

                    if (value.Engine != Domain.Settings.Instance.MediaEngine)
                        RestartRequired = true;

                    if (value.Engine == MediaEngine.Wmp)
                        UseHttps = false;
                    RaisePropertyChanged("CanUseHttps");
                }
            }
        }

        public bool EnableRemotePlay
        {
            get { return _enableRemotePlay; }
            set
            {
                if (Set(ref _enableRemotePlay, value))
                {
                    CanSave = true;
                }
            }
        }

        public List<string> RemotePlayAddresses
        {
            get { return NetworkHelper.GetLocalIpAddresses(); }
        }

        public string SelectedRemotePlayAddress
        {
            get { return _selectedRemotePlayAddress; }
            set
            {
                if (Set(ref _selectedRemotePlayAddress, value))
                {
                    CanSave = true;
                    RaisePropertyChanged("RemotePlayHelp");
                }
            }
        }


        public string RemotePlayPort
        {
            get { return _remotePlayPort; }
            set
            {
                if (Set(ref _remotePlayPort, value))
                {
                    CanSave = true;
                    RaisePropertyChanged("RemotePlayHelp");
                }
            }
        }

        public string RemotePlayHelp
        {
            get
            {
                if (!string.IsNullOrEmpty(_selectedRemotePlayAddress))
                    return string.Format(MainResources.SettingsRemotePlayHelp,
                        "http://" + _selectedRemotePlayAddress + ":" + _remotePlayPort);

                return null;
            }
        }

        public bool CanUseHttps
        {
            //https currently works only in NAudio
            get { return SelectedEngine.Engine == MediaEngine.NAudio; }
        }

        public bool UseHttps
        {
            get { return _useHttps;}
            set
            {
                if (Set(ref _useHttps, value))
                {
                    CanSave = true;
                    RestartRequired = true;
                }
            }
        }

        public SettingsViewModel()
        {
            InitializeCommands();

            _selectedTheme = Domain.Settings.Instance.Theme;
            _selectedColorScheme = _colors.FirstOrDefault(c => c.Name == Domain.Settings.Instance.AccentColor);
            _checkForUpdates = Domain.Settings.Instance.CheckForUpdates;
            _enableNotifications = Domain.Settings.Instance.ShowTrackNotifications;
            _enableTrayIcon = Domain.Settings.Instance.EnableTrayIcon;
            _showBackgroundArt = Domain.Settings.Instance.ShowBackgroundArt;
            _showBackgroundArtCompactMode = Domain.Settings.Instance.ShowBackgroundArtCompactMode;
            _blurBackground = Domain.Settings.Instance.BlurBackground;
            _downloadArtistArt = Domain.Settings.Instance.DownloadArtistArt;
            _downloadAlbumArt = Domain.Settings.Instance.DownloadAlbumArt;
            _selectedEngine = Engines.FirstOrDefault(e => e.Engine == Domain.Settings.Instance.MediaEngine);
            _enableRemotePlay = Domain.Settings.Instance.EnableRemotePlay;
            _remotePlayPort = Domain.Settings.Instance.RemotePlayPort.ToString();
            _useHttps = Domain.Settings.Instance.UseHttps;

            if (string.IsNullOrEmpty(Domain.Settings.Instance.RemotePlayAddress))
            {
                var addresses = NetworkHelper.GetLocalIpAddresses();
                if (addresses != null && addresses.Count > 0)
                    _selectedRemotePlayAddress = addresses.First();
            }
            else
                _selectedRemotePlayAddress = Domain.Settings.Instance.RemotePlayAddress;

            var lang = _languages.FirstOrDefault(l => l.LanguageCode == Domain.Settings.Instance.Language);
            if (lang != null)
                _selectedLanguage = lang;
            else
                _selectedLanguage = _languages.First();

            #region Hotkeys

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "Next",
                Title = MainResources.SettingsHotkeyNext,
                Ctrl = Domain.Settings.Instance.NextHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.NextHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.NextHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.NextHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "Prev",
                Title = MainResources.SettingsHotkeyPrev,
                Ctrl = Domain.Settings.Instance.PrevHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.PrevHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.PrevHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.PrevHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "PlayPause",
                Title = MainResources.SettingsHotkeyPlayPause,
                Ctrl = Domain.Settings.Instance.PlayPauseHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.PlayPauseHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.PlayPauseHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.PlayPauseHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "FastForward",
                Title = MainResources.SettingsHotkeyFastForward,
                Ctrl = Domain.Settings.Instance.FastForwardHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.FastForwardHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.FastForwardHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.FastForwardHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "Rewind",
                Title = MainResources.SettingsHotkeyRewind,
                Ctrl = Domain.Settings.Instance.RewindHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.RewindHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.RewindHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.RewindHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "LikeDislike",
                Title = MainResources.SettingsHotkeyAddRemove,
                Ctrl = Domain.Settings.Instance.LikeDislikeHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.LikeDislikeHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.LikeDislikeHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.LikeDislikeHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "Shuffle",
                Title = MainResources.SettingsHotkeyShuffle,
                Ctrl = Domain.Settings.Instance.ShuffleHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.ShuffleHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.ShuffleHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.ShuffleHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "Repeat",
                Title = MainResources.SettingsHotkeyRepeat,
                Ctrl = Domain.Settings.Instance.RepeatHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.RepeatHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.RepeatHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.RepeatHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "IncreaseVolume",
                Title = MainResources.SettingsHotkeyIncreaseVolume,
                Ctrl = Domain.Settings.Instance.IncreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.IncreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.IncreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.IncreaseVolumeHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "DecreaseVolume",
                Title = MainResources.SettingsHotkeyDecreaseVolume,
                Ctrl = Domain.Settings.Instance.DecreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.DecreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.DecreaseVolumeHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.DecreaseVolumeHotKey.ToString()
            });

            _hotkeys.Add(new SettingsHotkey()
            {
                Id = "ShowHide",
                Title = MainResources.SettingsHotkeyShowHide,
                Ctrl = Domain.Settings.Instance.ShowHideHotKeyModifier.HasFlag(ModifierKeys.Control),
                Alt = Domain.Settings.Instance.ShowHideHotKeyModifier.HasFlag(ModifierKeys.Alt),
                Shift = Domain.Settings.Instance.ShowHideHotKeyModifier.HasFlag(ModifierKeys.Shift),
                Key = Domain.Settings.Instance.ShowHideHotKey.ToString()
            });

            #endregion

        }

        public async void Activate()
        {
            //check cache
            if (Directory.Exists("Cache"))
            {
                var cacheSize = await CalculateFolderSizeAsync("Cache");
                CacheSize = StringHelper.FormatSize(Math.Round(cacheSize, 1));
            }
        }

        private void InitializeCommands()
        {
            CloseSettingsCommand = new RelayCommand(() =>
            {
                ViewModelLocator.Main.ShowSidebar = true;
                ViewModelLocator.Main.GoBackCommand.Execute(null);
            });

            SaveCommand = new RelayCommand(SaveSettings);

            SaveRestartCommand = new RelayCommand(() =>
            {
                SaveSettings();

                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            });

            SignOutVkCommand = new RelayCommand(AccountManager.LogOutVk);

            LoginLastFmCommand = new RelayCommand(() =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new LoginLastFmView();
                flyout.Show();
            });

            SignOutLastFmCommand = new RelayCommand(AccountManager.LogoutLastFm);

            CheckUpdatesCommand = new RelayCommand(() => ViewModelLocator.UpdateService.CheckUpdates());

            ClearCacheCommand = new RelayCommand(async () =>
            {
                if (!Directory.Exists("Cache"))
                    return;

                foreach (var file in Directory.EnumerateFiles("Cache"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                foreach (var dir in Directory.EnumerateDirectories("Cache"))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                var cacheSize = await CalculateFolderSizeAsync("Cache");
                CacheSize = StringHelper.FormatSize(Math.Round(cacheSize, 1));
            });

            TellCommand = new RelayCommand(Tell);
        }

        private void SaveSettings()
        {
            switch (SelectedTheme)
            {
                case "Light":
                case "Dark":
                case "Graphite":
                case "Accent":
                    Application.Current.Resources.MergedDictionaries[1].Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", SelectedTheme), UriKind.Relative);
                    break;

                default:
                    Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Resources/Themes/Light.xaml", UriKind.Relative);
                    break;
            }

            switch (SelectedColorScheme.Name)
            {
                case "Red":
                case "Emerald":
                case "Magenta":
                case "Mango":
                case "Sea":
                case "Sky":
                case "Purple":
                case "Pink":
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri(string.Format("/Resources/Themes/Accents/{0}.xaml", SelectedColorScheme.Name), UriKind.Relative);
                    break;

                default:
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/Resources/Themes/Accents/Blue.xaml", UriKind.Relative);
                    break;
            }

            Domain.Settings.Instance.AccentColor = SelectedColorScheme.Name;

            Domain.Settings.Instance.Theme = SelectedTheme;

            Domain.Settings.Instance.Language = SelectedLanguage.LanguageCode;

            Domain.Settings.Instance.CheckForUpdates = CheckForUpdates;

            Domain.Settings.Instance.ShowTrackNotifications = EnableNotifications;

            Domain.Settings.Instance.EnableTrayIcon = EnableTrayIcon;

            ViewModelLocator.Main.ShowBackgroundArt = ShowBackgroundArt;

            ViewModelLocator.Main.ShowBackgroundArtCompactMode = ShowBackgroundArtCompactMode;

            Domain.Settings.Instance.DownloadArtistArt = DownloadArtistArt;

            Domain.Settings.Instance.BlurBackground = BlurBackground;

            Domain.Settings.Instance.DownloadAlbumArt = DownloadAlbumArt;

            Domain.Settings.Instance.MediaEngine = SelectedEngine.Engine;

            Domain.Settings.Instance.UseHttps = UseHttps;

            if (BlurBackground)
            {
                ((MainWindow)Application.Current.MainWindow).BackgroundArtControl.Effect = new BlurEffect() { RenderingBias = RenderingBias.Quality, Radius = 35 };
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).BackgroundArtControl.Effect = null;
            }

            Domain.Settings.Instance.EnableRemotePlay = _enableRemotePlay;
            Domain.Settings.Instance.RemotePlayAddress = _selectedRemotePlayAddress;
            Domain.Settings.Instance.RemotePlayPort = int.Parse(_remotePlayPort);

            if (EnableRemotePlay)
            {
                RemotePlayService.Instance.Start();
            }
            else
            {
                RemotePlayService.Instance.Stop();
            }

            foreach (var settingsHotkey in _hotkeys)
            {
                var modifier = ModifierKeys.None;
                if (settingsHotkey.Ctrl)
                    modifier |= ModifierKeys.Control;

                if (settingsHotkey.Alt)
                    modifier |= ModifierKeys.Alt;

                if (settingsHotkey.Shift)
                    modifier |= ModifierKeys.Shift;

                var key = (Key)Enum.Parse(typeof(Key), settingsHotkey.Key);

                switch (settingsHotkey.Id)
                {
                    case "Next":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.NextHotKeyModifier, Domain.Settings.Instance.NextHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.NextHotKeyModifier, Domain.Settings.Instance.NextHotKey);
                        }

                        Domain.Settings.Instance.NextHotKey = key;
                        Domain.Settings.Instance.NextHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h => AudioService.SkipNext());


                        break;

                    case "Prev":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.PrevHotKeyModifier, Domain.Settings.Instance.PrevHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.PrevHotKeyModifier, Domain.Settings.Instance.PrevHotKey);
                        }

                        Domain.Settings.Instance.PrevHotKey = key;
                        Domain.Settings.Instance.PrevHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h => AudioService.Prev());
                        break;

                    case "PlayPause":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.PlayPauseHotKeyModifier, Domain.Settings.Instance.PlayPauseHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.PlayPauseHotKeyModifier, Domain.Settings.Instance.PlayPauseHotKey);
                        }

                        Domain.Settings.Instance.PlayPauseHotKey = key;
                        Domain.Settings.Instance.PlayPauseHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                if (AudioService.IsPlaying)
                                    AudioService.Pause();
                                else AudioService.Play();
                            });
                        break;

                    case "LikeDislike":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.LikeDislikeHotKeyModifier, Domain.Settings.Instance.LikeDislikeHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.LikeDislikeHotKeyModifier, Domain.Settings.Instance.LikeDislikeHotKey);
                        }

                        Domain.Settings.Instance.LikeDislikeHotKey = key;
                        Domain.Settings.Instance.LikeDislikeHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                if (AudioService.CurrentAudio is VkAudio)
                                {
                                    ((VkAudio)AudioService.CurrentAudio).IsAddedByCurrentUser =
                                        !((VkAudio)AudioService.CurrentAudio).IsAddedByCurrentUser;
                                    ViewModelLocator.Main.AddRemoveAudioCommand.Execute(AudioService.CurrentAudio);
                                }
                            });
                        break;

                    case "Shuffle":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.ShuffleHotKeyModifier, Domain.Settings.Instance.ShuffleHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.ShuffleHotKeyModifier, Domain.Settings.Instance.ShuffleHotKey);
                        }

                        Domain.Settings.Instance.ShuffleHotKey = key;
                        Domain.Settings.Instance.ShuffleHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                ViewModelLocator.Main.Shuffle = !ViewModelLocator.Main.Shuffle;
                            });
                        break;


                    case "Repeat":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.RepeatHotKeyModifier, Domain.Settings.Instance.RepeatHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.RepeatHotKeyModifier, Domain.Settings.Instance.RepeatHotKey);
                        }

                        Domain.Settings.Instance.RepeatHotKey = key;
                        Domain.Settings.Instance.RepeatHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                ViewModelLocator.Main.Repeat = !ViewModelLocator.Main.Repeat;
                            });
                        break;

                    case "IncreaseVolume":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.IncreaseVolumeHotKeyModifier, Domain.Settings.Instance.IncreaseVolumeHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.IncreaseVolumeHotKeyModifier, Domain.Settings.Instance.IncreaseVolumeHotKey);
                        }

                        Domain.Settings.Instance.IncreaseVolumeHotKey = key;
                        Domain.Settings.Instance.IncreaseVolumeHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                ViewModelLocator.Main.Volume += 5;
                            });
                        break;

                    case "DecreaseVolume":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.DecreaseVolumeHotKeyModifier, Domain.Settings.Instance.DecreaseVolumeHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.DecreaseVolumeHotKeyModifier, Domain.Settings.Instance.DecreaseVolumeHotKey);
                        }

                        Domain.Settings.Instance.DecreaseVolumeHotKey = key;
                        Domain.Settings.Instance.DecreaseVolumeHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                ViewModelLocator.Main.Volume -= 5;
                            });
                        break;

                    case "ShowHide":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.ShowHideHotKeyModifier, Domain.Settings.Instance.ShowHideHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.ShowHideHotKeyModifier, Domain.Settings.Instance.ShowHideHotKey);
                        }

                        Domain.Settings.Instance.ShowHideHotKey = key;
                        Domain.Settings.Instance.ShowHideHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h =>
                            {
                                if (Application.Current.MainWindow.WindowState != WindowState.Minimized)
                                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                                else
                                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                            });
                        break;

                    case "FastForward":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.FastForwardHotKeyModifier, Domain.Settings.Instance.FastForwardHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.FastForwardHotKeyModifier, Domain.Settings.Instance.FastForwardHotKey);
                        }

                        Domain.Settings.Instance.FastForwardHotKey = key;
                        Domain.Settings.Instance.FastForwardHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h => AudioService.FastForward(7));
                        break;

                    case "Rewind":
                        if (ViewModelLocator.Main.HotKeyManager.IsRegistered(Domain.Settings.Instance.RewindHotKeyModifier, Domain.Settings.Instance.RewindHotKey))
                        {
                            ViewModelLocator.Main.HotKeyManager.UnregisterHotkey(Domain.Settings.Instance.RewindHotKeyModifier, Domain.Settings.Instance.RewindHotKey);
                        }

                        Domain.Settings.Instance.RewindHotKey = key;
                        Domain.Settings.Instance.RewindHotKeyModifier = modifier;

                        if (key != Key.None)
                            ViewModelLocator.Main.HotKeyManager.RegisterHotkey(modifier, key, h => AudioService.Rewind(7));
                        break;
                }
            }

            Domain.Settings.Instance.Save();

            CloseSettingsCommand.Execute(null);
        }

        private static Task<float> CalculateFolderSizeAsync(string folder)
        {
            return Task.Run(() =>
            {
                return CalculateFolderSize(folder);
            });
        }

        private static float CalculateFolderSize(string folder)
        {
            float folderSize = 0.0f;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(folder))
                    return folderSize;
                else
                {
                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(folder))
                        {
                            if (File.Exists(file))
                            {
                                var finfo = new FileInfo(file);
                                folderSize += finfo.Length;
                            }
                        }

                        folderSize += Directory.GetDirectories(folder).Sum(dir => CalculateFolderSize(dir));
                    }
                    catch (NotSupportedException ex)
                    {
                        LoggingService.Log(string.Format("Unable to calculate folder size: {0}", ex.Message));
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggingService.Log(string.Format("Unable to calculate folder size: {0}", ex.Message));
            }
            return folderSize;
        }

        private async void Tell()
        {
            try
            {
                var posId = await ViewModelLocator.Vkontakte.Wall.Post(message: MainResources.AboutTellMessage, attachments:
                    new[] { new VkLinkAttachment() { Url = "http://meridianvk.com" } });

                if (posId != 0)
                {
                    var flyout = new FlyoutControl();
                    flyout.FlyoutContent = new TellResultView(posId);
                    flyout.Show();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }
    }
}
