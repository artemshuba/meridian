using Meridian.Interfaces;
using System.Collections.Generic;

namespace Meridian.Model
{
    /// <summary>
    /// Group of tracks
    /// </summary>
    public class AudioGroup
    {
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public IList<IAudio> Items { get; set; }

        public AudioGroup()
        {

        }

        public AudioGroup(string title, IList<IAudio> items)
        {
            Title = title;
            Items = items;
        }
    }
}
