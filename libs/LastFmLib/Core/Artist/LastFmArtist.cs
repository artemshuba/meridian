using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Artist
{
    public class LastFmArtist
    {
        public string Name { get; set; }
        public string Mbid { get; set; }
        public string ImageSmall { get; set; }
        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageExtraLarge { get; set; }
        public string ImageMega { get; set; }

        public List<string> TopTags { get; set; }

        public List<LastFmArtist> SimilarArtists { get; set; }

        public string Bio { get; set; }

        public static LastFmArtist FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new LastFmArtist();
            result.Name = json["name"].Value<string>();
            if (json["mbid"] != null)
                result.Mbid = json["mbid"].Value<string>();

            var imageToken = json["image"];
            if (imageToken != null)
            {
                foreach (var image in imageToken.Children())
                {
                    if (image["#text"] == null)
                        continue;

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
                        case "mega":
                            result.ImageMega = image["#text"].Value<string>();
                            break;
                    }
                }
            }

            if (json.SelectToken("tags.tag") != null)
            {
                result.TopTags = new List<string>();
                var tagToken = json.SelectToken("tags.tag");
                if (tagToken is JArray)
                {
                    foreach (var t in tagToken)
                    {
                        result.TopTags.Add(t["name"].Value<string>());
                    }
                }
                else
                {
                    result.TopTags.Add(tagToken["name"].Value<string>());
                }
            }

            if (json.SelectToken("similar.artist") != null)
            {
                result.SimilarArtists = new List<LastFmArtist>();
                var similarToken = json.SelectToken("similar.artist");
                if (similarToken is JArray)
                {
                    foreach (var artistToken in similarToken)
                    {
                        result.SimilarArtists.Add(FromJson(artistToken));
                    }
                }
                else
                {
                    result.SimilarArtists.Add(FromJson(similarToken));
                }
            }

            if (json.SelectToken("bio.summary") != null)
            {
                result.Bio = json.SelectToken("bio.summary").Value<string>().Trim();
            }

            return result;
        }
    }
}
