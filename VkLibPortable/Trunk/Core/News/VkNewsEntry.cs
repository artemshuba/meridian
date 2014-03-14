using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using VkLib.Core.Attachments;
using VkLib.Core.Users;
using VkLib.Extensions;

namespace VkLib.Core.News
{
    public class VkNewsEntry
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public long SourceId { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }

        public List<VkAttachment> Attachments { get; set; }

        public VkProfileBase Author { get; set; }

        public long CommentsCount { get; set; }

        public bool CanWriteComment { get; set; }

        public List<VkNewsEntry> CopyHistory { get; set; } 

        public static VkNewsEntry FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkNewsEntry();
            if (json["post_id"] != null)
                result.Id = json["post_id"].Value<long>();

            if (json["type"] != null)
                result.Type = json["type"].Value<string>();

            if (json["source_id"] != null)
                result.SourceId = json["source_id"].Value<long>();

            if (json["date"] != null)
                result.Date = DateTimeExtensions.UnixTimeStampToDateTime(json["date"].Value<long>());

            if (json["text"] != null)
            {
                result.Text = WebUtility.HtmlDecode(json["text"].Value<string>());
                result.Text = result.Text.Replace("<br>", Environment.NewLine);
            }

            if (json["attachments"] != null)
            {
                result.Attachments = new List<VkAttachment>();

                foreach (var a in json["attachments"])
                {
                    switch (a["type"].Value<string>())
                    {
                        case "audio":
                            result.Attachments.Add(VkAudioAttachment.FromJson(a["audio"]));
                            break;

                        case "photo":
                            result.Attachments.Add(VkPhotoAttachment.FromJson(a["photo"]));
                            break;
                    }
                }
            }


            if (json["copy_history"] != null)
            {
                result.CopyHistory = new List<VkNewsEntry>();

                foreach (var p in json["copy_history"])
                {
                    var post = VkNewsEntry.FromJson(p);
                    if (post != null)
                        result.CopyHistory.Add(post);
                }
            }

            if (json["comments"] != null)
            {
                result.CommentsCount = json["comments"]["count"].Value<long>();
                result.CanWriteComment = json["comments"]["can_post"].Value<int>() == 1;
            }

            return result;
        }
    }
}