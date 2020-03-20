using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkLib.Core.Attachments;
using VkLib.Core.Users;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Messages
{
    /// <summary>
    /// Messages request
    /// </summary>
    public class VkMessagesRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkMessagesRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        /// <summary>
        /// <para>Send message</para>
        /// <para>See also: <seealso cref="http://vk.com/dev/messages.send"/></para>
        /// </summary>
        /// <returns>Id of new message</returns>
        public async Task<long> Send(long userId, long chatId, string message, VkAttachment attachment = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            if (userId != 0)
                parametres.Add("user_id", userId.ToString());
            else
            {
                if (chatId != 0)
                    parametres.Add("chat_id", chatId.ToString());
                else
                    throw new Exception("User id or chat id must be specified.");
            }

            if (!string.IsNullOrEmpty(message))
                parametres.Add("message", message);

            if (attachment != null)
            {
                string type = null;
                if (attachment is VkAudioAttachment)
                    type = "audio";
                else if (attachment is VkPhotoAttachment)
                    type = "photo";
                if (type != null)
                    parametres.Add("attachment", string.Format("{0}{1}_{2}", type, attachment.OwnerId, attachment.Id));
            }

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.send"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            return response["response"].Value<long>();
        }

        public async Task<VkItemsResponse<VkMessage>> GetDialogs(int offset = 0, int count = 0, uint previewLength = 0,
            string userId = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (offset > 0)
                parameters.Add("offset", offset.ToString());

            if (count > 0)
                parameters.Add("count", count.ToString());

            if (previewLength > 0)
                parameters.Add("preview_length", previewLength.ToString());

            if (!string.IsNullOrEmpty(userId))
                parameters.Add("user_id", userId);

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.getDialogs"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkMessage>(response["response"]["items"].Select(i => VkMessage.FromJson(i, _vkontakte.ApiVersion)).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkMessage>.Empty;
        }

        public async Task<VkItemsResponse<VkMessage>> GetHistory(long userId, long chatId = 0, int offset = 0, int count = 0, bool rev = false)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            if (userId != 0)
                parametres.Add("user_id", userId.ToString());
            else
            {
                if (chatId != 0)
                    parametres.Add("chat_id", chatId.ToString());
                else
                    throw new Exception("User id or chat id must be specified.");
            }

            if (offset > 0)
                parametres.Add("offset", offset.ToString());

            if (count > 0)
                parametres.Add("count", count.ToString());

            if (rev)
                parametres.Add("rev", "1");

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.getHistory"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkMessage>(response["response"]["items"].Select(i => VkMessage.FromJson(i, _vkontakte.ApiVersion)).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkMessage>.Empty;
        }

        public async Task<List<VkProfile>> GetChatUsers(long chatId, IEnumerable<long> chatIds = null, string fields = null, string nameCase = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            if (chatId != 0)
                parametres.Add("chat_id", chatId.ToString());
            else
            {
                if (chatIds != null)
                    parametres.Add("chat_ids", string.Join(",", chatIds));
                else
                    throw new Exception("Chat id or chat ids must be specified.");
            }

            if (!string.IsNullOrEmpty(fields))
                parametres.Add("fields", fields);

            if (!string.IsNullOrEmpty(nameCase))
                parametres.Add("name_case", nameCase);

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.getChatUsers"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response") != null)
            {
                return response["response"].Select(VkProfile.FromJson).ToList();
            }

            return null;
        }

        public async Task<VkMessage> GetById(long messageId, int previewLength = 0)
        {
            var result = await GetById(new List<long>() { messageId }, previewLength);
            if (result.Items != null)
                return result.Items.FirstOrDefault();

            return null;
        }

        public async Task<VkItemsResponse<VkMessage>> GetById(IEnumerable<long> messageIds, int previewLength = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            if (messageIds != null)
                parametres.Add("message_ids", string.Join(",", messageIds));

            if (previewLength > 0)
                parametres.Add("preview_length", previewLength.ToString());

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.getById"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkMessage>(response["response"]["items"].Select(i => VkMessage.FromJson(i, _vkontakte.ApiVersion)).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkMessage>.Empty;
        }
    }
}
