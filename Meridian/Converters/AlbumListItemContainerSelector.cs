using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VkLib.Core.Audio;

namespace Meridian.Converters
{
    public class AlbumListItemContainerStyleSelector : StyleSelector
    {
        /// <summary>
        /// Стиль для стандартных альбомов ("все аудиозаписи, "со стены" и т.п.)
        /// </summary>
        public Style DefaultItemStyle { get; set; }

        /// <summary>
        /// Стиль для пользовательских альбомов
        /// </summary>
        public Style ItemStyle { get; set; }

        /// <summary>
        /// Стиль для разделителя
        /// </summary>
        public Style SeparatorStyle { get; set; }

        /// <summary>
        /// Стиль для кнопки создания альбома
        /// </summary>
        public Style AddAlbumStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var album = item as VkAudioAlbum;
            if (album != null)
            {

                switch ((int)album.Id)
                {
                    case -1:
                    case -100:
                    case -101:
                        return DefaultItemStyle;

                    case int.MinValue:
                        return SeparatorStyle;

                    default:
                        return ItemStyle;
                }
            }

            return base.SelectStyle(item, container);
        }
    }
}
