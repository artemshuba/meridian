using System.Collections.Generic;
using VkLib.Core.News;

namespace Meridian.Model
{
    public class NewsItemsResponse<T> : ItemsResponse<T>
    {
        public string NextFrom { get; set; }

        public NewsItemsResponse(List<T> items, int totalCount = 0)
            : base(items, totalCount)
        {

        }
    }
}
