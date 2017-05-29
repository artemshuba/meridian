using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using LastFmLib;
using Meridian.Domain;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.Messages;
using Neptune.Messages;
using VkLib;
using VkLib.Auth;
using VkLib.Core.Auth;

namespace Meridian.Services
{
    public static class AccountManager
    {
        private static readonly Vkontakte _vkontakte;
        private static readonly LastFm _lastFm;
        private const VkScopeSettings ScopeSettings = VkScopeSettings.CanAccessAudios | VkScopeSettings.CanAccessVideos | VkScopeSettings.CanAccessFriends |
                      VkScopeSettings.CanAccessGroups | VkScopeSettings.CanAccessWall | VkScopeSettings.CanAccessStatus | VkScopeSettings.CanAccessPhotos;

        public static List<Account> Accounts
        {
            get { return Settings.Instance.Accounts; }
        }

        static AccountManager()
        {
            _vkontakte = ViewModelLocator.Vkontakte;
            _lastFm = ViewModelLocator.LastFm;

            Initialize();
        }

        private static void Initialize()
        {
            if (!Accounts.Any(a => a is VkAccount))
                Accounts.Add(new VkAccount());

            if (!Accounts.Any(a => a is LastFmAccount))
                Accounts.Add(new LastFmAccount());
        }

        public static async Task LoginVk(string login, string password, string captchaSid, string captchaKey)
        {
            var token = await _vkontakte.Auth.Login(login, password, ScopeSettings, captchaSid, captchaKey);
            if (token == null || token.Token == null)
            {
                throw new ArgumentException("AccessToken is empty");
            }
            else
            {
                Settings.Instance.AccessToken = token;
                Settings.Instance.Save();

                var p = _vkontakte.LoginParams ?? new Dictionary<string, string>()
                {
                    ["version"] = "4.11.1",
                    ["func_v"] = "5"
                };

                p.Add("userId", token.UserId.ToString());

                await _vkontakte.Execute.GetBaseData(p);

                Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogIn, Service = "vk" });
            }
        }

        public static void SetLoginVk(AccessToken token)
        {
            if (token == null || token.Token == null)
            {
                throw new ArgumentException("AccessToken is empty");
            }
            else
            {
                _vkontakte.AccessToken = token;
                Settings.Instance.AccessToken = token;
                Settings.Instance.Save();
                Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogIn, Service = "vk" });
            }
        }

        public static async Task LoginLastFm(string login, string password)
        {
            var result = await _lastFm.Auth.GetMobileSession(login, password);
            if (result == null || result.Key == null)
            {
                throw new ArgumentException("Session key is empty");
            }
            else
            {
                _lastFm.SessionKey = result.Key;
                Settings.Instance.LastFmUsername = result.Username;
                Settings.Instance.LastFmSession = result.Key;
                Settings.Instance.Save();

                Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogIn, Service = "lastfm" });
            }
        }

        public static void LogoutLastFm()
        {
            Settings.Instance.LastFmUsername = null;
            Settings.Instance.LastFmSession = null;
            Settings.Instance.Save();

            Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogOut, Service = "lastfm" });
        }

        public static void LogOutVk()
        {
            AudioService.Stop();
            AudioService.CurrentAudio = null;
            AudioService.SetCurrentPlaylist(null);
            AudioService.Clear();

            _vkontakte.AccessToken.Token = null;
            _vkontakte.AccessToken.UserId = 0;
            _vkontakte.AccessToken.ExpiresIn = DateTime.MinValue;

            Settings.Instance.AccessToken = null;
            Settings.Instance.Save();

            ViewModelLocator.Main.ShowSidebar = false;
            Messenger.Default.Send(new NavigateToPageMessage() { Page = "/Main.LoginView" });
        }

        public static bool IsLoggedInLastFm()
        {
            return !string.IsNullOrEmpty(Settings.Instance.LastFmSession);
        }
    }
}
