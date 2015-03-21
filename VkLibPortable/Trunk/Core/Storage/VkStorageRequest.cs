using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Storage
{
    public class VkStorageRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkStorageRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<bool> Set(string key, string value)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("key", key);

            if (!string.IsNullOrEmpty(value))
                parameters.Add("value", value);

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "storage.set"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }
        public async Task<string> Get(string key)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("key", key);

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "storage.get"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"] != null)
            {
                return response["response"].Value<string>();
            }

            return null;
        }
    }
}
