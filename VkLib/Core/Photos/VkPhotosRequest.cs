using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Photos
{
    public class VkPhotosRequest
    {
        private readonly Vkontakte _vkontakte;

        internal VkPhotosRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<string> GetWallUploadServer(long albumId = 0, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (albumId > 0)
                parameters.Add("aid", albumId.ToString());

            if (groupId > 0)
                parameters.Add("group_id", groupId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "photos.getWallUploadServer"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response["response"]["upload_url"] != null)
                return response["response"]["upload_url"].Value<string>();

            return null;
        }

        public async Task<VkUploadPhotoResponse> UploadWallPhoto(string url, string fileName, Stream photoStream)
        {
            var client = new HttpClient();

            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            var fileContent = new StreamContent(photoStream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = fileName,
                    Name = "photo"
                };

            content.Add(fileContent);

            var responseMessage = await client.PostAsync(new Uri(url), content);
            var response = await responseMessage.Content.ReadAsStringAsync();
            var json = JObject.Parse(response);
            return VkUploadPhotoResponse.FromJson(json);
        }

        public async Task<VkPhoto> SaveWallPhoto(string server, string photo, string hash, long userId = 0, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("server", server);
            parameters.Add("photo", photo);
            parameters.Add("hash", hash);

            if (userId != 0)
                parameters.Add("user_id", userId.ToString());

            if (groupId != 0)
                parameters.Add("group_id", groupId.ToString());

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "photos.saveWallPhoto"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response["response"] != null)
                return VkPhoto.FromJson(response["response"].First);

            return null;
        }
    }
}
