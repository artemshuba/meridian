using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Image
{
    public class LastFmImage
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public double OriginalWidth { get; set; }

        public double OriginalHeight { get; set; }

        public string Original { get; set; }

        public string Large { get; set; }

        public string LargeSquare { get; set; }

        public string Medium { get; set; }

        public string Small { get; set; }

        public string ExtraLarge { get; set; }

        internal static LastFmImage FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new LastFmImage();
            var sizeToken = json.SelectToken("sizes.size");
            if (sizeToken != null)
            {
                foreach (var size in sizeToken.Children())
                {
                    switch (size["name"].Value<string>())
                    {
                        case "small":
                            result.Small = size["#text"].Value<string>();
                            result.Width = Convert.ToDouble(size["width"].Value<string>());
                            result.Height = Convert.ToDouble(size["height"].Value<string>());
                            break;
                        case "medium":
                            result.Medium = size["#text"].Value<string>();
                            result.Width = Convert.ToDouble(size["width"].Value<string>());
                            result.Height = Convert.ToDouble(size["height"].Value<string>());
                            break;
                        case "large":
                            result.Large = size["#text"].Value<string>();
                            result.Width = Convert.ToDouble(size["width"].Value<string>());
                            result.Height = Convert.ToDouble(size["height"].Value<string>());
                            break;
                        case "extralarge":
                            result.ExtraLarge = size["#text"].Value<string>();
                            result.Width = Convert.ToDouble(size["width"].Value<string>());
                            result.Height = Convert.ToDouble(size["height"].Value<string>());
                            break;
                        case "original":
                            result.Original = size["#text"].Value<string>();
                            result.Width = Convert.ToDouble(size["width"].Value<string>());
                            result.Height = Convert.ToDouble(size["height"].Value<string>());
                            result.OriginalWidth = Convert.ToDouble(size["width"].Value<string>());
                            result.OriginalHeight = Convert.ToDouble(size["height"].Value<string>());
                            break;
                    }
                }

            }

            return result;
        }
    }
}
