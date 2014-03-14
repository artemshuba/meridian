using System.Collections.Generic;

namespace VkLib.Core
{
    /// <summary>
    /// Generic items response from VK api. Contains items collection and total count of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VkItemsResponse<T>
    {
        public static VkItemsResponse<T> Empty = new VkItemsResponse<T>();

        /// <summary>
        /// Total items count that can be requested
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public List<T> Items { get; set; }


        public VkItemsResponse()
        {

        }

        public VkItemsResponse(List<T> items, int totalCount = 0)
        {
            Items = items;

            if (totalCount == 0 && items != null)
                TotalCount = items.Count;
            else
                TotalCount = totalCount;
        }
    }
}
