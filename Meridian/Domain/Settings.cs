using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.Services.Media.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkLib.Core.Auth;
using VkLib.Core.Groups;

namespace Meridian.Domain
{
    public class Settings
    {
        private const string SETTINGS_FILE = "Meridian.settings";

        private static Settings _instance = new Settings();

        public static Settings Instance
        {
            get { return _instance; }
        }

        public List<Account> Accounts { get; set; } 

        public AccessToken AccessToken { get; set; }

        public bool Shuffle { get; set; }

        public bool Repeat { get; set; }

        public float Volume { get; set; }

        public bool IsMuted { get; set; }

        public bool EnableStatusBroadcasting { get; set; }

        public bool EnableScrobbling { get; set; }

        public string LastFmUsername { get; set; }

        public string LastFmSession { get; set; }

        public bool CheckForUpdates { get; set; }

        public bool InstallDevUpdates { get; set; }

        public bool NeedClean { get; set; }

        public string AccentColor { get; set; }

        public string Theme { get; set; }

        public string Language { get; set; }

        public bool SendStats { get; set; }

        public bool ShowTrackNotifications { get; set; }

        public bool EnableTrayIcon { get; set; }

        public bool ShowBackgroundArt { get; set; }

        public bool BlurBackground { get; set; }

        public bool DownloadArtistArt { get; set; }

        public bool DownloadAlbumArt { get; set; }

        public bool TellRequestShown { get; set; }

        public DateTime FirstStart { get; set; }

        public MediaEngine MediaEngine { get; set; }

        public List<VkGroup> FeedSocieties { get; set; } 

        #region Window settings

        public double Width { get; set; }
        public double Height { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public bool IsWindowMaximized { get; set; }
        public double CompactTop { get; set; }
        public double CompactLeft { get; set; }
        public UIMode LastCompactMode { get; set; }

        #endregion

        #region Hotkeys

        public Key NextHotKey { get; set; }
        public ModifierKeys NextHotKeyModifier { get; set; }
        public Key PrevHotKey { get; set; }
        public ModifierKeys PrevHotKeyModifier { get; set; }
        public Key PlayPauseHotKey { get; set; }
        public ModifierKeys PlayPauseHotKeyModifier { get; set; }
        public Key ShowHideHotKey { get; set; }
        public ModifierKeys ShowHideHotKeyModifier { get; set; }
        public Key LikeDislikeHotKey { get; set; }
        public ModifierKeys LikeDislikeHotKeyModifier { get; set; }
        public Key ShuffleHotKey { get; set; }
        public ModifierKeys ShuffleHotKeyModifier { get; set; }
        public Key RepeatHotKey { get; set; }
        public ModifierKeys RepeatHotKeyModifier { get; set; }
        public Key IncreaseVolumeHotKey { get; set; }
        public ModifierKeys IncreaseVolumeHotKeyModifier { get; set; }
        public Key DecreaseVolumeHotKey { get; set; }
        public ModifierKeys DecreaseVolumeHotKeyModifier { get; set; }
        public Key FastForwardHotKey { get; set; }
        public ModifierKeys FastForwardHotKeyModifier { get; set; }
        public Key RewindHotKey { get; set; }
        public ModifierKeys RewindHotKeyModifier { get; set; }

        #endregion

        public Settings()
        {
            Width = 1024;
            Height = 650;

            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;

            CompactLeft = SystemParameters.PrimaryScreenWidth / 2 - 250 / 2;
            CompactTop = SystemParameters.PrimaryScreenHeight / 2 - 160 / 2;

            Volume = 0.5f;

            CheckForUpdates = true;

            AccentColor = "Blue";
            Theme = "Graphite";
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            ShowTrackNotifications = true;

            SendStats = true;

            FirstStart = DateTime.Now;

            DownloadArtistArt = true;
            DownloadAlbumArt = true;

            BlurBackground = true;

            Accounts = new List<Account>();

            MediaEngine = MediaEngine.Wmp;

            FeedSocieties = new List<VkGroup>();
        }

        public static void Load()
        {
            if (!File.Exists(SETTINGS_FILE))
                return;

            try
            {
                var json = File.ReadAllText(SETTINGS_FILE);
                if (string.IsNullOrEmpty(json))
                    return;

                var serializer = new JsonSerializer();
                var o = (JObject)JsonConvert.DeserializeObject(json);
                var settings = serializer.Deserialize<Settings>(o.CreateReader());
                _instance = settings;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public void Save()
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(this, settings);

            File.WriteAllText(SETTINGS_FILE, json);
        }
    }
}
