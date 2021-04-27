using Newtonsoft.Json;
using System;

namespace DeezerLib.Data
{
    public class DeezerAlbum
    {
        public string Id { get; set; }

        public string Title { get; set; }

        [JsonProperty("cover_small")]
        public string CoverSmall { get; set; }

        [JsonProperty("cover_medium")]
        public string CoverMedium { get; set; }

        [JsonProperty("cover_big")]
        public string CoverBig { get; set; }

        [JsonProperty("cover_xl")]
        public string CoverXl { get; set; }

        public int Duration { get; set; }

        public DeezerArtist Artist { get; set; }

        [JsonProperty("nb_tracks")]
        public int NumberOfTracks { get; set; }

        [JsonProperty("release_date")]
        public DateTime? ReleaseDate { get; set; }
    }
}