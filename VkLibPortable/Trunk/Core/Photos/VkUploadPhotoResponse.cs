using System;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Photos
{
    /// <summary>
    /// Response from upload photo requests
    /// </summary>
    public class VkUploadPhotoResponse
    {
        /// <summary>
        /// Server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Photo string. Should be passed unmodified.
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Hash
        /// </summary>
        public string Hash { get; set; }

        public static VkUploadPhotoResponse FromJson(JObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var result = new VkUploadPhotoResponse();
            result.Server = (string) json["server"];
            result.Photo = (string) json["photo"];
            result.Hash = (string) json["hash"];

            return result;
        }
    }
}
