using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkLib.Core.Groups;
using VkLib.Core.Users;

namespace VkLib.Core.News
{
    public class VkNewsRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkNewsRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<VkNewsResponse> Get(string sourceIds = null, string filters = null, int count = 0, string startFrom = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(sourceIds))
                parameters.Add("source_ids", sourceIds);

            if (!string.IsNullOrEmpty(filters))
                parameters.Add("filters", filters);

            if (count > 0)
                parameters.Add("count", count.ToString());

            if (!string.IsNullOrEmpty(startFrom))
                parameters.Add("start_from", startFrom);

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "newsfeed.get"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                var result = new VkNewsResponse((from n in response["response"]["items"] select VkNewsEntry.FromJson(n)).ToList());

                if (response["response"]["profiles"] != null)
                {
                    var users = (from n in response["response"]["profiles"] select VkProfile.FromJson(n)).ToList();
                    foreach (var entry in result.Items)
                    {
                        entry.Author = users.FirstOrDefault(u => u.Id == entry.SourceId);
                    }
                }

                if (response["response"]["groups"] != null)
                {
                    var groups = (from n in response["response"]["groups"] select VkGroup.FromJson(n)).ToList();
                    foreach (var entry in result.Items.Where(e => e.Author == null))
                    {
                        entry.Author = groups.FirstOrDefault(g => g.Id == Math.Abs(entry.SourceId));
                    }
                }

                if (response["response"]["next_from"] != null)
                    result.NextFrom = response["response"]["next_from"].Value<string>();

                return result;
            }

            return null;
        }
    }
}
