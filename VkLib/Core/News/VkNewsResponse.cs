using System.Collections.Generic;
using Newtonsoft.Json;

namespace VkLib.Core.News
{
    /// <summary>
    /// News response
    /// </summary>
    public class VkNewsResponse : VkItemsResponse<VkNewsEntry>
    {
        /// <summary>
        /// Used for getting next page of news
        /// </summary>
        [JsonProperty("next_from")]
        public string NextFrom { get; set; }

        public VkNewsResponse(List<VkNewsEntry> items, int totalCount = 0)
            : base(items, totalCount)
        {
        }
    }
}
