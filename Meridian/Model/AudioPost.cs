using System;
using System.Collections.Generic;
using VkLib.Core.Users;

namespace Meridian.Model
{
    /// <summary>
    /// Пост с аудиозаписями
    /// </summary>
    public class AudioPost : Audio
    {
        /// <summary>
        /// Текст
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Аудиозаписи
        /// </summary>
        public List<Audio> Audios { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public VkProfile Author { get; set; }

        /// <summary>
        /// Дата публикации
        /// </summary>
        public DateTime Date { get; set; }
    }
}
