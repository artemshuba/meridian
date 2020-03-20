using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;
using VkLib.Core.Attachments;
using VkLib.Core.Users;
using VkLib.Extensions;

namespace VkLib.Core.Wall
{
    public class VkWallEntry
    {
        public double Id { get; set; }

        public List<VkAttachment> Attachments { get; set; }

        public long SourceId { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }

        public VkProfileBase Author { get; set; }

        public List<VkWallEntry> CopyHistory { get; set; }

        public static VkWallEntry FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new VkWallEntry();
            result.Id = json["id"].Value<double>();

            if (json["text"] != null)
            {
                result.Text = WebUtility.HtmlDecode(json["text"].Value<string>());
                result.Text = result.Text.Replace("<br>", Environment.NewLine);
            }

            if (json["from_id"] != null)
                result.SourceId = json["from_id"].Value<long>();

            if (json["date"] != null)
                result.Date = DateTimeExtensions.UnixTimeStampToDateTime(json["date"].Value<long>());

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
                result.CopyHistory = new List<VkWallEntry>();

                foreach (var p in json["copy_history"])
                {
                    try
                    {
                        var post = VkWallEntry.FromJson(p);
                        if (post != null)
                            result.CopyHistory.Add(post);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                }
            }

            return result;
        }
    }
}