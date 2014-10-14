using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Domain;
using Meridian.ViewModel;
using Meridian.ViewModel.Messages;
using VkLib.Auth;

namespace Meridian.Model
{
    public class LoginParams
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public Dictionary<string, string> AdditionalParams { get; set; }

        public LoginParams()
        {
            AdditionalParams = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Account
    /// </summary>
    public class Account : INotifyPropertyChanged
    {
        private bool _isLoggedIn;

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// TItle
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Is user logged in using this account
        /// </summary>
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn == value)
                    return;

                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public virtual void Login(LoginParams loginParams)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VkAccount : Account
    {
        private const VkScopeSettings ScopeSettings = VkScopeSettings.CanAccessAudios | VkScopeSettings.CanAccessVideos | VkScopeSettings.CanAccessFriends |
              VkScopeSettings.CanAccessGroups | VkScopeSettings.CanAccessWall | VkScopeSettings.CanAccessStatus | VkScopeSettings.CanAccessPhotos;

        public override async void Login(LoginParams loginParams)
        {
            var vk = ViewModelLocator.Vkontakte;
            var captchaSid = loginParams.AdditionalParams.ContainsKey("captchaSid") ? loginParams.AdditionalParams["captchaSid"] : null;
            var captchaKey = loginParams.AdditionalParams.ContainsKey("captchaKey") ? loginParams.AdditionalParams["captchaKey"] : null;

            var token = await vk.Auth.Login(loginParams.Login, loginParams.Password, ScopeSettings, captchaSid, captchaKey);
            if (token == null || token.Token == null)
            {
                throw new ArgumentException("AccessToken is empty");
            }
            else
            {
                Settings.Instance.AccessToken = token;
                Settings.Instance.Save();
                Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogIn, Service = "vk" });
            }
        }
    }

    public class LastFmAccount : Account
    {
        public override async void Login(LoginParams loginParams)
        {
            var lastFm = ViewModelLocator.LastFm;

            var result = await lastFm.Auth.GetMobileSession(loginParams.Login, loginParams.Password);
            if (result == null || result.Key == null)
            {
                throw new ArgumentException("Session key is empty");
            }
            else
            {
                lastFm.SessionKey = result.Key;
                Settings.Instance.LastFmUsername = result.Username;
                Settings.Instance.LastFmSession = result.Key;
                Settings.Instance.Save();

                Messenger.Default.Send(new LoginMessage() { Type = LoginType.LogIn, Service = "lastfm" });
            }
        }
    }
}
