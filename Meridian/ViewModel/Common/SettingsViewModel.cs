using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using Jupiter.Utils.Helpers;
using LastFmLib.Core.Auth;
using Meridian.Controls;
using Meridian.Enum;
using Meridian.Model;
using Meridian.Services;
using Meridian.Utils.Helpers;
using Meridian.Utils.Messaging;
using Meridian.View.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;

namespace Meridian.ViewModel.Common
{
    public class SettingsViewModel : ViewModelBase
    {
        private Locale _selectedLocale;
        private Theme _selectedTheme;
        private ColorScheme _selectedColor;

        private bool _isRestartRequired;

        #region Commands

        public DelegateCommand SignOutVkCommand { get; private set; }

        public DelegateCommand SignInLastFmCommand { get; private set; }

        public DelegateCommand SignOutLastFmCommand { get; private set; }

        #endregion

        public List<Locale> Locales { get; } = new List<Locale>()
        {
            new Locale() {Title = "English", Code = "en-US"},
            new Locale() {Title = "Русский", Code = "ru"}
        };

        public Locale SelectedLocale
        {
            get { return _selectedLocale; }
            set
            {
                if (Set(ref _selectedLocale, value))
                {
                    ApplicationLanguages.PrimaryLanguageOverride = value.Code;
                    IsRestartRequired = true;

                    Analytics.TrackEvent(AnalyticsEvent.SettingsChangeLanguage, new Dictionary<string, object>
                    {
                        ["language"] = value.Code
                    });
                }
            }
        }

        public List<Theme> Themes { get; } = new List<Theme>()
        {
            new Theme { Title = Resources.GetStringByKey("Settings_ThemeDefault"), Value = ElementTheme.Default },
            new Theme { Title = Resources.GetStringByKey("Settings_ThemeLight"), Value = ElementTheme.Light },
            new Theme { Title = Resources.GetStringByKey("Settings_ThemeDark"), Value = ElementTheme.Dark }
        };

        public Theme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (Set(ref _selectedTheme, value))
                {
                    ChangeTheme();
                };
            }
        }

        public List<ColorScheme> ColorSchemes { get; } = new List<ColorScheme>()
        {
            new ColorScheme() { Color = "#006ac1", Name = "Blue"},
            new ColorScheme() { Color = "#e51400", Name = "Red"},
            new ColorScheme() { Color = "#1ba1e2", Name = "Sky"},
            new ColorScheme() { Color = "#059f01", Name = "Emerald"},
            new ColorScheme() { Color = "#fe6f11", Name = "Mango"},
            new ColorScheme() { Color = "#d80073", Name = "Magenta"},
            new ColorScheme() { Color = "#009f9f", Name = "Sea"},
            new ColorScheme() { Color = "#6800d3", Name = "Purple"},
            new ColorScheme() { Color = "#e671b8", Name = "Pink"},
            new ColorScheme() { Color = "#333333", Name = "System"}
        };

        public ColorScheme SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (Set(ref _selectedColor, value))
                {
                    AppState.Accent = value.Name;

                    Analytics.TrackEvent(AnalyticsEvent.SettingsChangeAccent, new Dictionary<string, object>
                    {
                        ["color"] = value.Name
                    });

                    IsRestartRequired = true;
                }
            }
        }

        public bool EnableBackgroundArt
        {
            get { return AppState.ShowArtistArt; }
            set
            {
                if (AppState.ShowArtistArt == value)
                    return;

                AppState.ShowArtistArt = value;
                if (!value)
                    EnableBackgroundBlur = false;
                RaisePropertyChanged(nameof(EnableBackgroundArt));
            }
        }

        public bool EnableBackgroundBlur
        {
            get { return AppState.EnableBackgroundBlur; }
            set
            {
                if (AppState.EnableBackgroundBlur == value)
                    return;

                AppState.EnableBackgroundBlur = value;
                Shell.Current.EnableBackgroundArtBlur = value;
                RaisePropertyChanged(nameof(EnableBackgroundBlur));
            }
        }

        public bool IsRestartRequired
        {
            get { return _isRestartRequired; }
            private set
            {
                if (Set(ref _isRestartRequired, value))
                {
                    var dialog = new MessageDialog(Resources.GetStringByKey("Settings_RestartDialogContent"), Resources.GetStringByKey("Settings_RestartDialogTitle"));
                    dialog.Commands.Add(new UICommand(Resources.GetStringByKey("Close"), x => Application.Current.Exit()));
                    dialog.Commands.Add(new UICommand(Resources.GetStringByKey("Cancel")));

                    _ = dialog.ShowAsync();
                }
            }
        }

        public List<StartPage> StartPages { get; } = new List<StartPage>
        {
            StartPage.Explore,
            StartPage.Mymusic
        };

        public StartPage SelectedStartPage
        {
            get { return AppState.StartPage; }
            set
            {
                AppState.StartPage = value;

                Analytics.TrackEvent(AnalyticsEvent.SettingsChangeStartPage, new Dictionary<string, object>
                {
                    ["page"] = value.ToString()
                });
            }
        }

        public string Version
        {
            get { return AppInfoHelper.GetAppVersionString(); }
        }

        public LastFmAuthResult LastFmSession
        {
            get { return AppState.LastFmSession; }
        }

        public bool EnableStatusBroadcasting
        {
            get { return AppState.EnableStatusBroadcasting; }
            set
            {
                if (AppState.EnableStatusBroadcasting == value)
                    return;

                AppState.EnableStatusBroadcasting = value;
                //TODO reset status on false
                RaisePropertyChanged(nameof(EnableStatusBroadcasting));
            }
        }

        public bool EnableScrobbling
        {
            get { return AppState.EnableScrobbling; }
            set
            {
                if (AppState.EnableScrobbling == value)
                    return;

                AppState.EnableScrobbling = value;
                RaisePropertyChanged(nameof(EnableScrobbling));
            }
        }

        public SettingsViewModel()
        {
            var currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrEmpty(currentLanguage))
                currentLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();

            _selectedLocale = Locales.FirstOrDefault(l => l.Code == currentLanguage);
            if (_selectedLocale == null)
                _selectedLocale = Locales.First();

            //theme
            if (AppState.Theme != null)
            {
                switch (AppState.Theme)
                {
                    case ApplicationTheme.Light:
                        _selectedTheme = Themes.First(t => t.Value == ElementTheme.Light);
                        break;

                    case ApplicationTheme.Dark:
                        _selectedTheme = Themes.First(t => t.Value == ElementTheme.Dark);
                        break;
                }
            }
            else
                _selectedTheme = Themes.First(t => t.Value == ElementTheme.Default);

            _selectedColor = ColorSchemes.FirstOrDefault(c => c.Name == AppState.Accent) ?? ColorSchemes.FirstOrDefault();
        }

        protected override void InitializeCommands()
        {
            SignOutVkCommand = new DelegateCommand(() =>
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });

                Analytics.TrackEvent(AnalyticsEvent.SignOutVk);
            });

            SignInLastFmCommand = new DelegateCommand(async () =>
            {
                await PopupControl.Show<LastFmLoginView>();
                RaisePropertyChanged(nameof(LastFmSession));
            });

            SignOutLastFmCommand = new DelegateCommand(() =>
            {
                AppState.LastFmSession = null;

                RaisePropertyChanged(nameof(LastFmSession));
            });
        }


        private void ChangeTheme()
        {
            switch (SelectedTheme.Value)
            {
                case ElementTheme.Default:
                    AppState.Theme = null;
                    break;

                case ElementTheme.Light:
                    AppState.Theme = ApplicationTheme.Light;
                    break;

                case ElementTheme.Dark:
                    AppState.Theme = ApplicationTheme.Dark;
                    break;
            }

            Shell.Current.RequestedTheme = SelectedTheme.Value;

            Analytics.TrackEvent(AnalyticsEvent.SettingsChangeTheme, new Dictionary<string, object>
            {
                ["theme"] = AppState.Theme
            });
        }
    }
}