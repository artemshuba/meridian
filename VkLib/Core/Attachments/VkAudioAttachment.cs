using System;
using Newtonsoft.Json.Linq;
using VkLib.Core.Audio;

namespace VkLib.Core.Attachments
{
    public class VkAudioAttachment : VkAttachment
    {
        public string Artist { get; set; }

        public string Title { get; set; }

        public TimeSpan Duration { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public override string Type { get { return "audio"; } }

        public VkAudioAttachment()
        {
            
        }

        public VkAudioAttachment(VkAudio audio)
        {
            this.Id = audio.Id;
            this.OwnerId = audio.OwnerId;
            this.Artist = audio.Artist;
            this.Title = audio.Title;
            this.Duration = audio.Duration;
            this.Url = audio.Url;
        }

        public static new VkAudioAttachment FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkAudioAttachment();
            result.Id = json["id"].Value<long>();
            result.OwnerId = json["owner_id"].Value<long>();
            result.Artist = json["artist"].Value<string>();
            result.Title = json["title"].Value<string>();
            result.Duration = TimeSpan.FromSeconds(json["duration"].Value<int>());
            if (json["url"] != null)
                result.Url = json["url"].Value<string>();
            return result;
        }
    }
}
