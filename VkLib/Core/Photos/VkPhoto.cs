using System;
using Newtonsoft.Json.Linq;
using VkLib.Extensions;

namespace VkLib.Core.Photos
{
    public class VkPhoto
    {
        public string Id { get; set; }

        public long Pid { get; set; }

        public int Aid { get; set; }

        public long OwnerId { get; set; }

        public string Src { get; set; }

        public string SrcBig { get; set; }

        public string SrcSmall { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Text { get; set; }

        public DateTime Created { get; set; }

        public static VkPhoto FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var result = new VkPhoto();
            result.Id = (string)json["id"];
            result.Pid = (long)json["pid"];
            result.Aid = (int)json["aid"];
            result.OwnerId = (long)json["owner_id"];
            result.Src = (string)json["src"];
            result.SrcBig = (string)json["src_big"];
            result.SrcSmall = (string)json["src_small"];
            result.Width = (int)json["width"];
            result.Height = (int)json["height"];
            result.Text = (string)json["text"];
            result.Created = DateTimeExtensions.UnixTimeStampToDateTime((long)json["created"]);

            return result;
        }
    }
}
