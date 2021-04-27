using Meridian.Interfaces;
using System;
using System.Collections.Generic;
using VkLib.Core.Users;

namespace Meridian.Model
{
    /// <summary>
    /// Post with attached tracks
    /// </summary>
    public class AudioPost
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Publish date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Post content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Author
        /// </summary>
        public VkProfileBase Author { get; set; }

        /// <summary>
        /// Image uri
        /// </summary>
        public Uri ImageUri { get; set; }

        /// <summary>
        /// Tracks
        /// </summary>
        public List<IAudio> Tracks { get; set; }

        /// <summary>
        /// Link to this post
        /// </summary>
        public Uri PostUri { get; set; }

        /// <summary>
        /// Link to author
        /// </summary>
        public Uri AuthorUri { get; set; }

    }
}
