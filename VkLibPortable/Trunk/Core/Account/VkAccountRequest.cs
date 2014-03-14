using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Account
{
    public class VkAccountRequest
    {
        private readonly Vkontakte _vkontakte;

        public VkAccountRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<bool> RegisterDevice(string token, string deviceModel = null, string systemVersion = null, bool noText = false, string subscribe = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            parametres.Add("token", token);

            if (!string.IsNullOrEmpty(deviceModel))
                parametres.Add("device_model", deviceModel);

            if (!string.IsNullOrEmpty(systemVersion))
                parametres.Add("system_version", systemVersion);

            if (noText)
                parametres.Add("no_text", "1");

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "account.registerDevice"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }
    }
}
