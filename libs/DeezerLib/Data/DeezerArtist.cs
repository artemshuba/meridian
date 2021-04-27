using Newtonsoft.Json;

namespace DeezerLib.Data
{
    public class DeezerArtist
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        [JsonProperty("picture_small")]
        public string PictureSmall { get; set; }

        [JsonProperty("picture_medium")]
        public string PictureMedium { get; set; }

        [JsonProperty("picture_big")]
        public string PictureBig { get; set; }

        [JsonProperty("picture_xl")]
        public string PictureXl { get; set; }

        public bool HasRadio { get; set; }

        public string Tracklist { get; set; }
    }
}
