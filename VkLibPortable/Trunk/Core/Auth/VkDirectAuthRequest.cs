using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkLib.Auth;
using VkLib.Error;
using VkLib.Extensions;

namespace VkLib.Core.Auth
{
    public class VkDirectAuthRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkDirectAuthRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        /// <summary>
        /// <para>Direct auth with login and password.</para>
        /// <para>See also: <seealso cref="http://vk.com/pages?oid=-1&p=Прямая_авторизация"/></para>
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <param name="scopeSettings">Scope settings</param>
        /// <param name="captchaSid">Captcha sid</param>
        /// <param name="captchaKey">Captcha key</param>
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> Login(string login, string password, VkScopeSettings scopeSettings = VkScopeSettings.CanAccessFriends, 
            string captchaSid = null, string captchaKey = null)
        {
            if (string.IsNullOrEmpty(_vkontakte.AppId))
                throw new NullReferenceException("App id must be specified.");

            if (string.IsNullOrEmpty(_vkontakte.ClientSecret))
                throw new NullReferenceException("Client secret must be specified.");

            var parameters = new Dictionary<string, string>
            {
                {"username", login},
                {"password", password},
                {"grant_type", "password"},
                {"scope", ((int) scopeSettings).ToString(CultureInfo.InvariantCulture)}
            };

            if (!string.IsNullOrEmpty(captchaSid) && !string.IsNullOrEmpty(captchaKey))
            {
                parameters.Add("captcha_sid", captchaSid);
                parameters.Add("captcha_key", captchaKey);
            }

            parameters.Add("client_id", _vkontakte.AppId);
            parameters.Add("client_secret", _vkontakte.ClientSecret);

            var request = new VkRequest(new Uri(VkConst.DirectAuthUrl), parameters);
            var response = await request.Execute();

            if (response["error"] != null)
            {
                switch (response["error"].Value<string>())
                {
                    case "need_captcha":
                        throw new VkCaptchaNeededException(response["captcha_sid"].Value<string>(), response["captcha_img"].Value<string>());

                    case "invalid_client":
                        throw new VkInvalidClientException();

                    case "need_validation":
                        throw new VkNeedValidationException() { RedirectUri = new Uri(response["redirect_uri"].Value<string>()) };
                }

                return null;
            }

            var token = new AccessToken();
            token.Token = response["access_token"].Value<string>();
            token.UserId = response["user_id"].Value<long>();
            token.ExpiresIn = response["expires_in"].Value<long>() == 0 ? DateTime.MaxValue : DateTimeExtensions.UnixTimeStampToDateTime(response["expires_in"].Value<long>());
            _vkontakte.AccessToken = token;
            return token;
        }
    }
}
