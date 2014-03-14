using System.Collections.Generic;

namespace Meridian.Model
{
    public class ItemsResponse<T>
    {
        public static ItemsResponse<T> Empty = new ItemsResponse<T>();

        /// <summary>
        /// Total items count that can be requested
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public List<T> Items { get; set; }


        public ItemsResponse()
        {

        }

        public ItemsResponse(List<T> items, int totalCount = 0)
        {
            Items = items;

            if (totalCount == 0 && items != null)
                TotalCount = items.Count;
            else
                TotalCount = totalCount;
        }
    }
}
