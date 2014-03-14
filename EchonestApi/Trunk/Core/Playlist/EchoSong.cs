using System;
using Newtonsoft.Json.Linq;

namespace EchonestApi.Core.Playlist
{
    public class EchoSong
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string ArtistName { get; set; }

        public string ArtistId { get; set; }

        internal static EchoSong FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new EchoSong();
            result.Id = json["id"].Value<string>();
            result.Title = json["title"].Value<string>();
            result.ArtistId = json["artist_id"].Value<string>();
            result.ArtistName = json["artist_name"].Value<string>();

            return result;
        }
    }
}
