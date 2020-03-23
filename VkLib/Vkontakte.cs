using System.Collections.Generic;
using VkLib.Core;
using VkLib.Core.Account;
using VkLib.Core.Audio;
using VkLib.Core.Auth;
using VkLib.Core.Execute;
using VkLib.Core.Favorites;
using VkLib.Core.Friends;
using VkLib.Core.Groups;
using VkLib.Core.Messages;
using VkLib.Core.News;
using VkLib.Core.Photos;
using VkLib.Core.Stats;
using VkLib.Core.Status;
using VkLib.Core.Storage;
using VkLib.Core.Subscriptions;
using VkLib.Core.Users;
using VkLib.Core.Video;
using VkLib.Core.Wall;

namespace VkLib
{
    /// <summary>
    /// Core object for data access
    /// </summary>
    public class Vkontakte
    {
        private string _clientSecret;
        private string _appId;
        private string _apiVersion = "5.57";
        private string _userAgent;

        public string AppId
        {
            get { return _appId; }
            set { _appId = value; }
        }

        public string ClientSecret
        {
            get { return _clientSecret; }
            set { _clientSecret = value; }
        }

        public string UserAgent
        {
            get { return VkRequest.UserAgent; }
            set { VkRequest.UserAgent = value; }
        }

        public Dictionary<string, string> LoginParams { get; set; }

        /// <summary>
        /// Api version
        /// </summary>
        public string ApiVersion
        {
            get { return _apiVersion; }
            set { _apiVersion = value; }
        }

        /// <summary>
        /// Use HTTPS
        /// </summary>
        public bool UseHttps { get; set; }

        /// <summary>
        /// Access token
        /// </summary>
        public AccessToken AccessToken { get; set; }

        /// <summary>
        /// Audio
        /// </summary>
        public VkAudioRequest Audio
        {
            get
            {
                return new VkAudioRequest(this);
            }
        }

        /// <summary>
        /// Users
        /// </summary>
        public VkUsersRequest Users
        {
            get
            {
                return new VkUsersRequest(this);
            }
        }

        /// <summary>
        /// Friends
        /// </summary>
        public VkFriendsRequest Friends
        {
            get
            {
                return new VkFriendsRequest(this);
            }
        }

        /// <summary>
        /// Groups
        /// </summary>
        public VkGroupsRequest Groups
        {
            get
            {
                return new VkGroupsRequest(this);
            }
        }

        /// <summary>
        /// News
        /// </summary>
        public VkNewsRequest News
        {
            get
            {
                return new VkNewsRequest(this);
            }
        }

        /// <summary>
        /// Wall
        /// </summary>
        public VkWallRequest Wall
        {
            get
            {
                return new VkWallRequest(this);
            }
        }

        /// <summary>
        /// Favorites
        /// </summary>
        public VkFavoritesRequest Favorites
        {
            get
            {
                return new VkFavoritesRequest(this);
            }
        }

        /// <summary>
        /// Status
        /// </summary>
        public VkStatusRequest Status
        {
            get
            {
                return new VkStatusRequest(this);
            }
        }

        /// <summary>
        /// Video
        /// </summary>
        public VkVideoRequest Video
        {
            get
            {
                return new VkVideoRequest(this);
            }
        }

        /// <summary>
        /// Messages
        /// </summary>
        public VkMessagesRequest Messages
        {
            get
            {
                return new VkMessagesRequest(this);
            }
        }

        /// <summary>
        /// Photos
        /// </summary>
        public VkPhotosRequest Photos
        {
            get
            {
                return new VkPhotosRequest(this);
            }
        }

        /// <summary>
        /// Long poll service
        /// </summary>
        public VkLongPollService LongPollService
        {
            get
            {
                return new VkLongPollService(this);
            }
        }

        /// <summary>
        /// Account
        /// </summary>
        public VkAccountRequest Account
        {
            get
            {
                return new VkAccountRequest(this);
            }
        }

        /// <summary>
        /// Subscriptions
        /// </summary>
        public VkSubscriptionsRequest Subscriptions
        {
            get
            {
                return new VkSubscriptionsRequest(this);
            }
        }

        /// <summary>
        /// OAuth
        /// </summary>
        public VkOAuthRequest OAuth
        {
            get
            {
                return new VkOAuthRequest(this);
            }
        }


        /// <summary>
        /// Direct Auth by login and password
        /// </summary>
        public VkDirectAuthRequest Auth
        {
            get
            {
                return new VkDirectAuthRequest(this);
            }
        }

        /// <summary>
        /// Statistics
        /// </summary>
        public VkStatsRequest Stats
        {
            get
            {
                return new VkStatsRequest(this);
            }
        }


        /// <summary>
        /// Storage
        /// </summary>
        public VkStorageRequest Storage
        {
            get
            {
                return new VkStorageRequest(this);
            }
        }


        /// <summary>
        /// Execute
        /// </summary>
        public VkExecuteRequest Execute
        {
            get
            {
                return new VkExecuteRequest(this);
            }
        }


        public Vkontakte(string appId, string clientSecret, string apiVersion, string userAgent)
        {
            AccessToken = new AccessToken();
            ApiVersion = apiVersion;

            AppId = appId;
            ClientSecret = clientSecret;
            UserAgent = userAgent;
        }

        internal void SignMethod(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, string>();

            parameters["access_token"] = AccessToken.Token;

            if (!string.IsNullOrEmpty(ApiVersion))
                parameters["v"] = ApiVersion;

            if (UseHttps)
                parameters["https"] = "1";
        }
    }
}
