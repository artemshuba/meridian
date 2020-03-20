using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Audio
{
    /// <summary>
    /// Audio
    /// <seealso cref="http://vk.com/dev/audio_object"/>
    /// </summary>
    public class VkAudio
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Owner id
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// Album id
        /// </summary>
        public long AlbumId { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Lyrics id
        /// </summary>
        public long LyricsId { get; set; }

        /// <summary>
        /// Genre id
        /// <seealso cref="http://vk.com/dev/audio_genres"/>
        /// </summary>
        public long GenreId { get; set; }

        internal static VkAudio FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkAudio();

            result.Id = json["id"].Value<long>();
            result.OwnerId = json["owner_id"].Value<long>();
            result.Duration = TimeSpan.FromSeconds(json["duration"].Value<double>());
            result.Url = json["url"].Value<string>();

            try
            {
                result.Title = WebUtility.HtmlDecode(json["title"].Value<string>()).Trim();
                result.Artist = WebUtility.HtmlDecode(json["artist"].Value<string>()).Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                result.Title = json["title"].Value<string>().Trim();
                result.Artist = json["artist"].Value<string>().Trim();
            }

            if (json["album_id"] != null)
                result.AlbumId = json["album_id"].Value<long>();

            if (json["lyrics_id"] != null)
                result.LyricsId = json["lyrics_id"].Value<long>();

            if (json["genre_id"] != null)
                result.GenreId = json["genre_id"].Value<long>();

            return result;
        }
    }
}
