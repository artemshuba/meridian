using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meridian.Resources.Localization;

namespace Meridian.Model
{
    /// <summary>
    /// Объект, представляющий собой рекомендацию (например для жанров - это жанр)
    /// </summary>
    public class Recommendation
    {
        /// <summary>
        /// Порядок группы
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Группа
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Отображаемый заголовок
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Внутренний ключ (например для жанров - ключевое слово жанра на английском)
        /// </summary>
        public string Key { get; set; }
    }

    public class RecommendationsCollection : List<Recommendation>
    {

    }

    public class GenreRecommendation : Recommendation
    {
        public GenreRecommendation()
        {
            Group = MainResources.RecommendationsGenresGroup;
            GroupOrder = 2;
        }
    }

    public class MoodRecommendation : Recommendation
    {
        public MoodRecommendation()
        {
            Group = MainResources.RecommendationsMoodsGroup;
            GroupOrder = 1;
        }
    }
}
