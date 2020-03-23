using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using VkLib.Core.Attachments;
using VkLib.Extensions;

namespace VkLib.Core.Messages
{
    /// <summary>
    /// Message
    /// <see cref="http://vk.com/dev/message"/>
    /// </summary>
    public class VkMessage
    {
        /// <summary>
        /// Message id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Author id (for income messages) or recepient id (for outcome messages)
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// True if message was read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// True for outcome message
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// Message or conversation title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Message text
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Attachments
        /// </summary>
        public List<VkAttachment> Attachments { get; set; }

        /// <summary>
        /// Forward messages
        /// </summary>
        public List<VkMessage> ForwardMessages { get; set; }

        /// <summary>
        /// True if message contains emoji
        /// </summary>
        public bool ContainsEmoji { get; set; }

        /// <summary>
        /// True if message was deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        #region Conversation related

        /// <summary>
        /// Conversation id
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// List of ids of active users in conversation
        /// </summary>
        public List<long> ChatUsers { get; set; }

        /// <summary>
        /// Number of users in conversation
        /// </summary>
        public int UsersCount { get; set; }

        /// <summary>
        /// Id of conversation owner
        /// </summary>
        public long AdminId { get; set; }

        #endregion

        public static VkMessage FromJson(JToken json, string apiVersion = null)
        {
            if (json == null)
                throw new Exception("Json can't be null");

            var result = new VkMessage();
            result.Id = (long)json["id"];
            result.UserId = (long)json["user_id"];
            result.Date = DateTimeExtensions.UnixTimeStampToDateTime((long)json["date"]);

            if (json["read_state"] != null)
                result.IsRead = (int)json["read_state"] == 1;

            if (json["out"] != null)
                result.IsOut = (int)json["out"] == 1;

            result.Title = (string)json["title"];
            result.Body = (string)json["body"];

            if (json["deleted"] != null)
                result.IsDeleted = (int)json["deleted"] == 1;

            if (json["attachments"] != null)
                result.Attachments = VkAttachment.FromJson(json["attachments"], apiVersion);

            //TODO forward messages

            if (json["emoji"] != null)
                result.ContainsEmoji = (int)json["emoji"] == 1;

            if (json["chat_id"] != null)
                result.ChatId = (long)json["chat_id"];

            if (json["chat_active"] != null)
            {
                result.ChatUsers = json["chat_active"].Select(t => t.Value<long>()).ToList();
            }

            if (json["users_count"] != null)
                result.UsersCount = (int)json["users_count"];

            if (json["admin_id"] != null)
                result.AdminId = (long)json["admin_id"];

            return result;
        }
    }
}
