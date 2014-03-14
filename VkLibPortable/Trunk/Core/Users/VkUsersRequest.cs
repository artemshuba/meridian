using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkLib.Core.Users
{
    public class VkUsersRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkUsersRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<VkProfile> Get(long userId, string fields = null, string nameCase = null)
        {
            var users = await Get(new List<string>() { userId.ToString() }, fields, nameCase);
            if (users.Items != null && users.Items.Count > 0)
                return users.Items.First();

            return null;
        }

        public async Task<VkItemsResponse<VkProfile>> Get(IEnumerable<string> userIds, string fields = null, string nameCase = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (userIds != null)
                parameters.Add("user_ids", string.Join(",", userIds));

            if (!string.IsNullOrWhiteSpace(fields))
                parameters.Add("fields", fields);

            if (!string.IsNullOrWhiteSpace(nameCase))
                parameters.Add("name_case", nameCase);

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "users.get"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response") != null)
            {
                return new VkItemsResponse<VkProfile>((from u in response["response"] select VkProfile.FromJson(u)).ToList());
            }

            return VkItemsResponse<VkProfile>.Empty;
        }

    }
}
