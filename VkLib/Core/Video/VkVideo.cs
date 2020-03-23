using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Video
{
    public class VkVideo
    {
        public long Id { get; set; }

        public long OwnerId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public string Link { get; set; }

        public string ImageSmall { get; set; }

        public string ImageMedium { get; set; }

        public DateTime Date { get; set; }

        public string Player { get; set; }

        public Dictionary<string, string> Files { get; set; }

        internal static VkVideo FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkVideo();

            if (json["id"] != null)
                result.Id = json["id"].Value<long>();

            if (json["vid"] != null)
                result.Id = json["vid"].Value<long>();

            result.OwnerId = json["owner_id"].Value<long>();
            result.Duration = TimeSpan.FromSeconds(json["duration"].Value<double>());
            if (json["link"] != null)
                result.Link = json["link"].Value<string>();
            result.Title = WebUtility.HtmlDecode(json["title"].Value<string>());
            result.Description = WebUtility.HtmlDecode(json["description"].Value<string>());

            if (json["thumb"] != null)
                result.ImageSmall = json["thumb"].Value<string>();

            if (json["image_medium"] != null)
                result.ImageMedium = json["image_medium"].Value<string>();

            if (json["files"] != null)
            {
                result.Files = new Dictionary<string, string>();

                foreach (JProperty child in json["files"].Children())
                {
                    if (!child.HasValues)
                        continue;

                    result.Files.Add(child.Name, child.Value.Value<string>());
                }
            }

            return result;
        }
    }
}
