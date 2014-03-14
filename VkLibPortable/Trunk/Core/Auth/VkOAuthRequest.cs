using System;
using System.Collections.Generic;
using VkLib.Auth;
using VkLib.Extensions;

namespace VkLib.Core.Auth
{
    public enum VkAuthDisplayType
    {
        Page,
        Popup,
        Mobile
    }

    public class VkOAuthRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkOAuthRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public string GetAuthUrl(VkScopeSettings scope, VkAuthDisplayType display)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("client_id", _vkontakte.AppId);
            parameters.Add("scope", ((int)scope).ToString());
            parameters.Add("redirect_uri", "https://oauth.vk.com/blank.html");
            parameters.Add("v", _vkontakte.ApiVersion);
            parameters.Add("response_type", "token");

            switch (display)
            {
                case VkAuthDisplayType.Page:
                    parameters.Add("display", "page");
                    break;

                case VkAuthDisplayType.Popup:
                    parameters.Add("display", "popup");
                    break;

                case VkAuthDisplayType.Mobile:
                    parameters.Add("display", "mobile");
                    break;
            }

            return VkConst.OAuthUrl + parameters.ToUrlParams();
        }

        public OAuthResult ProcessAuth(Uri uri)
        {
            return OAuthResult.Parse(uri);
        }
    }
}
