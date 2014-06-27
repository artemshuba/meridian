using System;
using System.Collections.Generic;
using System.Linq;
using LastFmLib.Core.Track;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Album
{
    public class LastFmAlbum
    {
        public string Mbid { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string ImageSmall { get; set; }
        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageExtraLarge { get; set; }
        public string ImageMega { get; set; }

        public List<LastFmTrack> Tracks { get; set; }
        public List<string> TopTags { get; set; }

        public static LastFmAlbum FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new LastFmAlbum();
            if (json["mbid"] != null)
                result.Mbid = json["mbid"].Value<string>();
            result.Name = json["name"].Value<string>();
            if (json["artist"] != null)
            {
                if (!json["artist"].HasValues)
                    result.Artist = json["artist"].Value<string>();
                else
                {
                    result.Artist = json["artist"]["name"].Value<string>();
                }
            }

            var imageToken = json["image"];
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

            if (json.SelectToken("tracks.track") != null)
            {
                result.Tracks = new List<LastFmTrack>();

                var trackJson = json.SelectToken("tracks.track");
                if (trackJson is JArray)
                    result.Tracks.AddRange((from a in trackJson select LastFmTrack.FromJson(a)).ToList());
                else
                    result.Tracks.Add(LastFmTrack.FromJson(trackJson));
            }

            if (json.SelectToken("toptags.tag") != null)
            {
                result.TopTags = new List<string>();
                foreach (var tagToken in json.SelectToken("toptags.tag"))
                {
                    if (tagToken is JProperty)
                        result.TopTags.Add(((JProperty)tagToken).Value.Value<string>());
                    else
                        result.TopTags.Add(tagToken["name"].Value<string>());
                }
            }

            return result;
        }
    }
}
