using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using VkLib.Core.Audio;
using System.Linq;

namespace VkLib.Core.Video
{
    public class VkVideoRequest
    {
        private const int MAX_VIDEO_COUNT = 200;

        private readonly Vkontakte _vkontakte;

        internal VkVideoRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task<IEnumerable<VkVideo>> Get(IList<string> videos, string uid = null, string gid = null, string aid = null, int previewWidth = 0, int count = 0, int offset = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            if (count > 200)
                throw new ArgumentException("Maximum count is 200.");

            var parameters = new Dictionary<string, string>();

            if (videos != null)
                parameters.Add("videos", string.Join(",", videos));

            if (!string.IsNullOrEmpty(uid))
                parameters.Add("uid", uid);

            if (!string.IsNullOrEmpty(gid))
                parameters.Add("gid", gid);

            if (!string.IsNullOrEmpty(aid))
                parameters.Add("aid", aid);

            if (previewWidth > 0)
                parameters.Add("width", previewWidth.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));
            else
                parameters.Add("count", MAX_VIDEO_COUNT.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "video.get"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"].HasValues)
            {
                return (from v in response["response"] where v.HasValues select VkVideo.FromJson(v));
            }

            return null;
        }

        public async Task<IEnumerable<VkVideo>> Search(string query, int count = 0, int offset = 0, bool hdOnly = false, VkAudioSortType sort = VkAudioSortType.DateAdded, bool adult = false)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            if (count > 200)
                throw new ArgumentException("Maximum count is 200.");

            if (query == null)
                throw new ArgumentException("Query must not be null.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("q", query);

            if (hdOnly)
                parameters.Add("hd", "1");

            parameters.Add("sort", ((int)sort).ToString(CultureInfo.InvariantCulture));

            if (adult)
                parameters.Add("adult", "1");

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));
            else
                parameters.Add("count", MAX_VIDEO_COUNT.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            parameters.Add("access_token", _vkontakte.AccessToken.Token);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "video.search"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"].HasValues)
            {
                return from v in response["response"] select VkVideo.FromJson(v);
            }

            return null;
        }
    }
}
