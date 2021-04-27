using System;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Track
{
    public class LastFmTrack
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string ImageSmall { get; set; }
        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageExtraLarge { get; set; }
        public string ImageMega { get; set; }

        internal static LastFmTrack FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new LastFmTrack();

            if (json["name"] != null)
                result.Title = json["name"].Value<string>();
            if (json["artist"] != null)
            {
                if (json.SelectToken("artist.name") != null)
                    result.Artist = json["artist"]["name"].Value<string>();
                else
                    result.Artist = json["artist"].Value<string>();
            }
            if (json["duration"] != null && !string.IsNullOrEmpty(json["duration"].Value<string>()))
            {
                result.Duration = json["duration"].Value<int>();
            }
            if (json["album"] != null)
            {
                result.Album = json["album"]["title"].Value<string>();
                var imageToken = json["album"]["image"];
                if (imageToken != null)
                {
                    foreach (var image in imageToken.Children())
                    {
                        switch (image["size"].Value<string>())
                        {
                            case "small":
                                result.ImageSmall = image["#text"].Value<string>();
                                break;
                            case "medium":
                                result.ImageMedium = image["#text"].Value<string>();
                                break;
                            case "large":
                                result.ImageLarge = image["#text"].Value<string>();
                                break;
                            case "extralarge":
                                result.ImageExtraLarge = image["#text"].Value<string>();
                                break;
                        }
                    }
                }
            }

            var imgToken = json["image"];
            if (imgToken != null)
            {
                foreach (var image in imgToken.Children())
                {
                    switch (image["size"].Value<string>())
                    {
                        case "small":
                            result.ImageSmall = image["#text"].Value<string>();
                            break;
                        case "medium":
                            result.ImageMedium = image["#text"].Value<string>();
                            break;
                        case "large":
                            result.ImageLarge = image["#text"].Value<string>();
                            break;
                        case "extralarge":
                            result.ImageExtraLarge = image["#text"].Value<string>();
                            break;
                    }
                }
            }

            return result;
        }
    }
}
