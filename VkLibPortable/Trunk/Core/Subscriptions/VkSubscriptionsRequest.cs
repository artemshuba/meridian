using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkLib.Core.Users;

namespace VkLib.Core.Subscriptions
{
    public class VkSubscriptionsRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkSubscriptionsRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<VkItemsResponse<VkProfile>> Get()
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "subscriptions.get"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkProfile>((from id in response["response"]["items"] select new VkProfile() { Id = (long)id }).ToList(), 
                    response["response"]["count"].Value<int>());
            }

            return VkItemsResponse<VkProfile>.Empty;
        }

    }
}
