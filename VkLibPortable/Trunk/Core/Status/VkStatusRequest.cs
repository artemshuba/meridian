using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Status
{
    public class VkStatusRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkStatusRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        /// <summary>
        /// Транслирует музыку в статус
        /// </summary>
        /// <param name="audioId">Id аудиозаписи</param>
        /// <param name="targetIds">Список id пользователей и сообществ, которым транслируется аудиозапись</param>
        /// <returns></returns>
        [Obsolete("Use VkAudioRequest.SetBroadcast instead")]
        public async Task<bool> SetBroadcast(string audioId, string targetIds = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(audioId))
                parameters.Add("audio", audioId);

            if (!string.IsNullOrEmpty(targetIds))
                parameters.Add("target_ids", targetIds);

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.setBroadcast"), parameters).Execute();

            if (response != null && response["response"] != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Получает значение трансляции в статус играющей музыки
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public async Task<bool?> GetBroadcast()
        {
            //TODO метода audio.getBroadcast больше нет
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getBroadcast"), null).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return null;
        }

        /// <summary>
        /// Устанавливает статус пользователя
        /// </summary>
        /// <param name="text">Текст статуса</param>
        /// <param name="audioId">ID аудиозаписи для музыкального статуса</param>
        /// <param name="audioOwnerId">ID владельца аудиозаписи</param>
        /// <returns></returns>
        public async Task<bool> Set(string text, string audioId, string audioOwnerId)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(text))
                parameters.Add("text", text);

            if (!string.IsNullOrEmpty(audioId) && !string.IsNullOrEmpty(audioOwnerId))
                parameters.Add("audio", audioOwnerId + "_" + audioId);

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "status.set"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }

        public async Task<string> Get(string uid)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(uid)) 
                parameters.Add("uid", uid);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "status.get"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"] != null) return response["response"].Value<string>();
            return null;
        }
    }
}
