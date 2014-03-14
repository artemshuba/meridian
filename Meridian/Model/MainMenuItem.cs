using System.Collections.Generic;

namespace Meridian.Model
{
    public class MainMenuItem
    {
        /// <summary>
        /// Группа
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Отображаемый заголовок
        /// </summary>
        public string Title { get; set; }

        public string Page { get; set; }

        public object Icon { get; set; }
    }


    public class MenuItemsCollection : List<MainMenuItem>
    {

    }
}
