using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Groups
{
    public enum VkGroupSearchType
    {
        /// <summary>
        /// Сортировать по количеству пользователей
        /// </summary>
        ByUsers = 0,

        /// <summary>
        /// Сортировать по скорости роста
        /// </summary>
        ByGrowSpeed = 1,

        /// <summary>
        /// Сортировать по отношению дневной посещаемости ко количеству пользователей
        /// </summary>
        ByVisitsPerDay = 2,

        /// <summary>
        /// Сортировать по отношению количества лайков к количеству пользователей
        /// </summary>
        ByLikes = 3,

        /// <summary>
        /// Сортировать по отношению количества комментариев к количеству пользователей
        /// </summary>
        ByComments = 4,

        /// <summary>
        /// Сортировать по отношению количества записей в обсуждениях к количеству пользователей
        /// </summary>
        ByDiscussions = 5
    }

    public class VkGroupsRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkGroupsRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<VkItemsResponse<VkGroup>> Get(long userId, string fields, string filter, int count, int offset, bool extended = true)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (userId != 0)
                parameters.Add("user_id", userId.ToString());

            if (!string.IsNullOrWhiteSpace(fields))
                parameters.Add("fields", fields);

            if (!string.IsNullOrWhiteSpace(filter))
                parameters.Add("filter", filter);

            if (count > 0)
                parameters.Add("count", count.ToString());

            if (offset > 0)
                parameters.Add("offset", offset.ToString());

            if (extended)
                parameters.Add("extended", "1");

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "groups.get"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkGroup>(response["response"]["items"].Select(VkGroup.FromJson).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkGroup>.Empty;
        }

        public async Task<VkItemsResponse<VkGroup>> Search(string query, VkGroupSearchType sort = VkGroupSearchType.ByUsers, int count = 0, int offset = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(query))
                parameters.Add("q", query);

            parameters.Add("sort", ((int)sort).ToString());

            if (count > 0)
                parameters.Add("count", count.ToString());

            if (offset > 0)
                parameters.Add("offset", offset.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "groups.search"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkGroup>((from g in response["response"]["items"] where g.HasValues select VkGroup.FromJson(g)).ToList(), response["response"]["count"].Value<int>());
            }

            return VkItemsResponse<VkGroup>.Empty;
        }
    }
}
