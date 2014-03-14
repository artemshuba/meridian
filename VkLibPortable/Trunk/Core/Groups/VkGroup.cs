using System;
using System.Net;
using Newtonsoft.Json.Linq;
using VkLib.Core.Users;

namespace VkLib.Core.Groups
{
    public class VkGroup : VkProfileBase
    {
        public bool IsAdmin { get; set; }

        public bool IsMember { get; set; }

        public bool IsClosed { get; set; }

        public string Type { get; set; }

        public static VkGroup FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkGroup();

            result.Id = Math.Abs(json["id"].Value<long>());
            result.Name = WebUtility.HtmlDecode(json["name"].Value<string>()).Trim();

            if (json["photo_50"] != null)
                result.Photo = (string)json["photo_50"];

            if (json["photo_100"] != null)
                result.PhotoMedium = json["photo_100"].Value<string>();

            if (json["photo_200_orig"] != null)
                result.PhotoBig = json["photo_200_orig"].Value<string>();

            if (json["photo_200"] != null)
                result.PhotoBigSquare = json["photo_200"].Value<string>();

            if (json["photo_400_orig"] != null)
                result.PhotoLarge = json["photo_400_orig"].Value<string>();

            if (json["photo_max"] != null)
                result.PhotoMaxSquare = json["photo_max"].Value<string>();

            if (json["photo_max_orig"] != null)
                result.PhotoMax = json["photo_max_orig"].Value<string>();

            if (json["is_admin"] != null)
                result.IsAdmin = json["is_admin"].Value<int>() == 1;

            if (json["is_member"] != null)
                result.IsAdmin = json["is_member"].Value<int>() == 1;

            if (json["is_closed"] != null)
                result.IsClosed = json["is_closed"].Value<int>() == 1;

            if (json["type"] != null)
                result.Type = json["type"].Value<string>();

            return result;
        }
    }
}
