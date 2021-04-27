using Newtonsoft.Json;

namespace DeezerLib.Data
{
    public class DeezerTrack
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DeezerArtist Artist { get; set; }

        public int Duration { get; set; }

        [JsonProperty("track_position")]
        public int TrackPosition { get; set; }
    }
}