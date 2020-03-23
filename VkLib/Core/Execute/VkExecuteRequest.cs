using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VkLib.Core.Execute
{
    public class VkExecuteRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkExecuteRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        //this method used by vk to mark access token as "true" (obtained from official app)
        public async Task GetBaseData(Dictionary<string, string> parameters)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "execute.getBaseData"), parameters).Execute();
        }
    }
}
