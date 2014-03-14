using System;
using Newtonsoft.Json.Linq;
using VkLib.Extensions;

namespace VkLib.Core.Users
{
    /// <summary>
    /// User sex
    /// </summary>
    public enum VkSex
    {
        Uknown = 0,
        Female = 1,
        Male = 2,
    }

    /// <summary>
    /// User profile
    /// <seealso cref="http://vk.com/dev/fields"/>
    /// </summary>
    public class VkProfile : VkProfileBase
    {
        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public override string Name
        {
            get { return FirstName + " " + LastName; }
        }

        /// <summary>
        /// Sex
        /// </summary>
        public VkSex Sex { get; set; }

        /// <summary>
        /// Is online
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Is online from mobile device
        /// </summary>
        public bool IsOnlineMobile { get; set; }

        /// <summary>
        /// Last seen
        /// </summary>
        public DateTime LastSeen { get; set; }

        public static VkProfile FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            return ParseV5(json);
        }

        //VK Api v5.0
        private static VkProfile ParseV5(JToken json)
        {
            var result = new VkProfile();

            result.Id = (long)json["id"];
            result.FirstName = (string)json["first_name"];
            result.LastName = (string)json["last_name"];

            if (json["photo"] != null) //NOTE in some methods this used instead of photo_xx
                result.Photo = (string)json["photo"];

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

            if (json["online"] != null)
                result.IsOnline = (int)json["online"] == 1;

            if (json["online_mobile"] != null)
                result.IsOnlineMobile = (int)json["online"] == 1;

            if (json["last_seen"] != null)
                result.LastSeen = DateTimeExtensions.UnixTimeStampToDateTime((long)json["last_seen"]["time"]);

            if (json["sex"] != null)
            {
                result.Sex = (VkSex)(int)json["sex"];
            }

            return result;
        }
    }
}
