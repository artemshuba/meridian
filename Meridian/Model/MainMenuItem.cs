using System.Collections.Generic;

namespace Meridian.Model
{
    public class MainMenuItem
    {
        /// <summary>
        /// Group title
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Group icon
        /// </summary>
        public object GroupIcon { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Page
        /// </summary>
        public string Page { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public object Icon { get; set; }
    }


    public class MenuItemsCollection : List<MainMenuItem>
    {

    }
}
