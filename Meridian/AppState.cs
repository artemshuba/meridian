using Jupiter.Services.Settings;
using LastFmLib.Core.Auth;
using Meridian.Enum;
using VkLib.Core.Auth;
using Microsoft.UI.Xaml;

namespace Meridian
{
    /// <summary>
    /// App state
    /// </summary>
    public static class AppState
    {
        /// <summary>
        /// Vk access token
        /// </summary>
        public static VkAccessToken VkToken
        {
            get { return SettingsService.Roaming.Get<VkAccessToken>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Theme
        /// </summary>
        public static ApplicationTheme? Theme
        {
            get { return SettingsService.Roaming.Get<ApplicationTheme>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Accent color
        /// </summary>
        public static string Accent
        {
            get { return SettingsService.Roaming.Get<string>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Start page
        /// </summary>
        public static StartPage StartPage
        {
            get { return SettingsService.Roaming.Get<StartPage>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Last.FM session
        /// </summary>
        public static LastFmAuthResult LastFmSession
        {
            get { return SettingsService.Roaming.Get<LastFmAuthResult>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Enable vk status broadcasting
        /// </summary>
        public static bool EnableStatusBroadcasting
        {
            get { return SettingsService.Roaming.Get(defaultValue: false); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Enable Last.FM scrobbling
        /// </summary>
        public static bool EnableScrobbling
        {
            get { return SettingsService.Roaming.Get(defaultValue: false); }
            set { SettingsService.Roaming.Set(value); }
        }

        /// <summary>
        /// Show artist art on background
        /// </summary>
        public static bool ShowArtistArt
        {
            get { return SettingsService.Local.Get(defaultValue: true); }
            set { SettingsService.Local.Set(value); }
        }

        /// <summary>
        /// Enable background blur
        /// </summary>
        public static bool EnableBackgroundBlur
        {
            get { return SettingsService.Local.Get(defaultValue: true); }
            set { SettingsService.Local.Set(value); }
        }


        #region Audio state

        /// <summary>
        /// Shuffle mode
        /// </summary>
        public static bool Shuffle
        {
            get { return SettingsService.Local.Get<bool>(); }
            set { SettingsService.Local.Set(value); }
        }

        /// <summary>
        /// Repeat mode
        /// </summary>
        public static RepeatMode Repeat
        {
            get { return SettingsService.Local.Get<RepeatMode>(); }
            set { SettingsService.Local.Set(value); }
        }

        /// <summary>
        /// Volume
        /// </summary>
        public static double Volume
        {
            get { return SettingsService.Local.Get(defaultValue: 0.5); }
            set { SettingsService.Local.Set(value); }
        }

        #endregion

        public static void Reset()
        {
            VkToken = null;
            LastFmSession = null;
            EnableScrobbling = false;
            EnableStatusBroadcasting = false;
            Shuffle = false;
            Repeat = RepeatMode.None;
            Volume = 0.5;
            StartPage = StartPage.Explore;
        }
    }
}
