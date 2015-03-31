using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkLib.Error;

namespace VkLib.Core.Audio
{
    /// <summary>
    /// Audios sort type
    /// </summary>
    public enum VkAudioSortType
    {
        /// <summary>
        /// Sort by date added
        /// </summary>
        DateAdded,
        /// <summary>
        /// Sort by duration
        /// </summary>
        Duration,
        /// <summary>
        /// Sort by popularity
        /// </summary>
        Popularity
    }

    /// <summary>
    /// Audio request
    /// </summary>
    public class VkAudioRequest
    {
        private const int MAX_AUDIO_COUNT = 300;

        private readonly Vkontakte _vkontakte;

        internal VkAudioRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        /// <summary>
        /// Get audios of current user.
        /// See also <see cref="http://vk.com/dev/audio.get"/>See also <see cref="http://vk.com/dev/audio.get"/>
        /// </summary>
        /// <returns></returns>
        public async Task<VkItemsResponse<VkAudio>> Get()
        {
            return await Get(_vkontakte.AccessToken.UserId);
        }

        /// <summary>
        /// Get audios of user or society.
        /// See also <see cref="http://vk.com/dev/audio.get"/>
        /// </summary>
        /// <param name="ownerId">Owner id. For society must be negative.</param>
        /// <param name="albumId">Album id</param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<VkItemsResponse<VkAudio>> Get(long ownerId, long albumId = 0, int count = 0, int offset = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new VkInvalidTokenException();

            var parameters = new Dictionary<string, string>();

            if (ownerId != 0)
                parameters.Add("owner_id", ownerId.ToString(CultureInfo.InvariantCulture));

            if (albumId != 0)
                parameters.Add("album_id", albumId.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.get"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkAudio>(response["response"]["items"].Select(VkAudio.FromJson).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkAudio>.Empty;
        }

        /// <summary>
        /// Get albums of user or society.
        /// See also <see cref="http://vk.com/dev/audio.getAlbums"/>
        /// </summary>
        /// <param name="ownerId">Owner id. For society must be negative.</param>
        /// <param name="count">Count</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        public async Task<VkItemsResponse<VkAudioAlbum>> GetAlbums(long ownerId, int count = 0, int offset = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (ownerId != 0)
                parameters.Add("owner_id", ownerId.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getAlbums"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkAudioAlbum>(response["response"]["items"].Select(VkAudioAlbum.FromJson).ToList(), (int)response["response"]["count"]);
            }

            return VkItemsResponse<VkAudioAlbum>.Empty;
        }

        /// <summary>
        /// Search audios.
        /// See also <see cref="http://vk.com/dev/audio.search"/>
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="count">Count</param>
        /// <param name="offset">Offset</param>
        /// <param name="sort">Sort</param>
        /// <param name="withLyricsOnly">If true will show only audios with lyrics</param>
        /// <param name="autoFix">If true will fix incorrect queries</param>
        /// <param name="artistOnly">If true will search only by artist</param>
        /// <param name="ownOnly">If true will search only in audios of current user</param>
        /// <returns></returns>
        public async Task<VkItemsResponse<VkAudio>> Search(string query, int count = 0, int offset = 0, VkAudioSortType sort = VkAudioSortType.DateAdded, bool withLyricsOnly = false, bool autoFix = true,
            bool artistOnly = false, bool ownOnly = false)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            if (count > MAX_AUDIO_COUNT)
                throw new ArgumentException("Maximum count is " + MAX_AUDIO_COUNT + ".");

            if (query == null)
                throw new ArgumentException("Query must not be null.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("q", query);

            if (autoFix)
                parameters.Add("auto_complete", "1");

            parameters.Add("sort", ((int)sort).ToString(CultureInfo.InvariantCulture));

            if (withLyricsOnly)
                parameters.Add("lyrics", "1");

            if (artistOnly)
                parameters.Add("performer_only", "1");

            if (ownOnly)
                parameters.Add("search_own", "1");

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));
            else
                parameters.Add("count", MAX_AUDIO_COUNT.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.search"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response.SelectToken("response.items") != null)
            {
                return new VkItemsResponse<VkAudio>((from a in response["response"]["items"] where a.HasValues && !string.IsNullOrEmpty(a["url"].Value<string>()) select VkAudio.FromJson(a)).ToList(),
                    response["response"]["count"].Value<int>());
            }

            return VkItemsResponse<VkAudio>.Empty;
        }

        /// <summary>
        /// Add audio to current user or society.
        /// See also <see cref="http://vk.com/dev/audio.add"/>
        /// </summary>
        /// <param name="audioId">Audio id</param>
        /// <param name="ownerId">Owner id</param>
        /// <param name="groupId">Target society</param>
        /// <param name="captchaSid">Captcha sid</param>
        /// <param name="captchaKey">Captcha key</param>
        /// <returns>Id of new audio</returns>
        public async Task<long> Add(long audioId, long ownerId, long groupId = 0, string captchaSid = null, string captchaKey = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("audio_id", audioId.ToString());

            parameters.Add("owner_id", ownerId.ToString());

            if (groupId != 0)
                parameters.Add("group_id", groupId.ToString());

            if (!string.IsNullOrEmpty(captchaSid))
                parameters.Add("captcha_sid", captchaSid);

            if (!string.IsNullOrEmpty(captchaKey))
                parameters.Add("captcha_key", captchaKey);

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.add"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return 0;

            if (response["response"] != null)
            {
                return response["response"].Value<long>();
            }

            return 0;
        }

        /// <summary>
        /// Remove audio from current user or society.
        /// See also <see cref="http://vk.com/dev/audio.delete"/>
        /// </summary>
        /// <param name="audioId">Audio id</param>
        /// <param name="ownerId">Owner id</param>
        /// <returns>True if success</returns>
        public async Task<bool> Delete(long audioId, long ownerId)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("audio_id", audioId.ToString());

            parameters.Add("owner_id", ownerId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.delete"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null && response["response"].Value<long>() == 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move audios to album.
        /// See also <see cref="http://vk.com/dev/audio.moveToAlbum"/>
        /// </summary>
        /// <param name="albumId">Album id</param>
        /// <param name="audioIds">List of audios ids</param>
        /// <param name="groupId">Source group id. If not specified, current user will be used.</param>
        /// <returns>True if success</returns>
        public async Task<bool> MoveToAlbum(long albumId, IList<long> audioIds, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("album_id", albumId.ToString());

            parameters.Add("audio_ids", string.Join(",", audioIds));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.moveToAlbum"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null && response["response"].Value<long>() == 1)
            {
                return true;
            }

            return false;
        }

        public async Task<List<VkAudio>> GetById(IList<string> ids)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("audios", string.Join(",", ids));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getById"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"].HasValues)
            {
                return (from a in response["response"] select VkAudio.FromJson(a)).ToList();
            }

            return null;
        }

        public async Task<string> GetLyrics(long lyricsId)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("lyrics_id", lyricsId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getLyrics"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response.SelectToken("response.text") != null)
            {
                var text = response.SelectToken("response.text").Value<string>();
                if (!string.IsNullOrEmpty(text))
                    return WebUtility.HtmlDecode(text);
            }

            return null;
        }

        public async Task<VkItemsResponse<VkAudio>> GetRecommendations(string targetAudio = null, int count = 0, int offset = 0, long userId = 0, bool shuffle = false)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(targetAudio))
                parameters.Add("target_audio", targetAudio);

            if (userId > 0)
                parameters.Add("user_id", userId.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            if (shuffle)
                parameters.Add("shuffle", "1");

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getRecommendations"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return VkItemsResponse<VkAudio>.Empty;

            var token = response.SelectToken("response.items");
            if (token != null && token.HasValues)
            {
                return new VkItemsResponse<VkAudio>((from a in token select VkAudio.FromJson(a)).ToList());
            }

            return VkItemsResponse<VkAudio>.Empty;
        }

        public async Task<VkItemsResponse<VkAudio>> GetPopular(bool onlyEng = false, int count = 0, int offset = 0, int genreId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (onlyEng)
                parameters.Add("only_eng", "1");

            if (genreId != 0)
                parameters.Add("genre_id", genreId.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
                parameters.Add("count", count.ToString(CultureInfo.InvariantCulture));

            if (offset > 0)
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.getPopular"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"].HasValues)
            {
                return new VkItemsResponse<VkAudio>((from a in response["response"] select VkAudio.FromJson(a)).ToList());
            }

            return VkItemsResponse<VkAudio>.Empty;
        }

        public async Task<long> Edit(long ownerId, long audioId, string artist = null, string title = null, string text = null, int genreId = 0, bool noSearch = false)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            const string method = "audio.edit";

            parameters.Add("owner_id", ownerId.ToString());
            parameters.Add("audio_id", audioId.ToString());

            if (!string.IsNullOrEmpty(artist))
                parameters.Add("artist", artist);

            if (!string.IsNullOrEmpty(title))
                parameters.Add("title", title);

            if (!string.IsNullOrEmpty(text))
                parameters.Add("text", text);

            if (noSearch)
                parameters.Add("no_search", "1");

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + method), parameters, string.IsNullOrEmpty(text) ? "GET" : "POST").Execute();

            if (VkErrorProcessor.ProcessError(response))
                return 0;

            return (response["response"].Value<long>());
        }

        public async Task<long> AddAlbum(string title, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("title", title);

            if (groupId > 0)
                parameters.Add("group_id", groupId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.addAlbum"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return 0;

            if (response.SelectToken("response.album_id") != null)
            {
                return response["response"]["album_id"].Value<long>();
            }

            return 0;
        }

        public async Task<bool> DeleteAlbum(long albumId, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("album_id", albumId.ToString());
            if (groupId > 0)
                parameters.Add("group_id", groupId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.deleteAlbum"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }

        public async Task<bool> EditAlbum(string albumId, string title, long groupId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("album_id", albumId);
            parameters.Add("title", title);

            if (groupId > 0)
                parameters.Add("group_id", groupId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.editAlbum"), parameters).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }

        public async Task<bool> Reorder(long audioId, long after, long before, long ownerId = 0)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            parameters.Add("audio_id", audioId.ToString());
            if (before != 0)
                parameters.Add("before", before.ToString());

            if (after != 0)
                parameters.Add("after", after.ToString());

            if (ownerId != 0)
                parameters.Add("owner_id", ownerId.ToString());

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.reorder"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return false;

            if (response["response"] != null)
            {
                return response["response"].Value<int>() == 1;
            }

            return false;
        }

        public async Task<List<long>> SetBroadcast(long audioId, long ownerId, IList<long> targetIds = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parameters = new Dictionary<string, string>();

            if (audioId != 0 && ownerId != 0)
                parameters.Add("audio", string.Format("{0}_{1}", ownerId, audioId));

            if (targetIds != null)
                parameters.Add("target_ids", string.Join(",", targetIds));

            _vkontakte.SignMethod(parameters);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "audio.setBroadcast"), parameters).Execute();

            if (VkErrorProcessor.ProcessError(response))
                return null;

            if (response["response"].HasValues)
            {
                return response["response"].Values<long>().ToList<long>();
            }

            return null;
        }

        /// <summary>
        /// Get list of genres
        /// </summary>
        /// <returns></returns>
        public List<VkGenre> GetGenres()
        {
            //http://vk.com/dev/audio_genres

            var genres = new List<VkGenre>();
            genres.Add(new VkGenre() { Id = 1, Title = "Rock" });
            genres.Add(new VkGenre() { Id = 2, Title = "Pop" });
            genres.Add(new VkGenre() { Id = 3, Title = "Rap & Hip-Hop" });
            genres.Add(new VkGenre() { Id = 4, Title = "Easy Listening" });
            genres.Add(new VkGenre() { Id = 5, Title = "Dance & House" });
            genres.Add(new VkGenre() { Id = 6, Title = "Instrumental" });
            genres.Add(new VkGenre() { Id = 7, Title = "Metal" });
            genres.Add(new VkGenre() { Id = 21, Title = "Alternative" });
            genres.Add(new VkGenre() { Id = 8, Title = "Dubstep" });
            genres.Add(new VkGenre() { Id = 9, Title = "Jazz & Blues" });
            genres.Add(new VkGenre() { Id = 10, Title = "Drum & Bass" });
            genres.Add(new VkGenre() { Id = 11, Title = "Trance" });
            genres.Add(new VkGenre() { Id = 12, Title = "Chanson" });
            genres.Add(new VkGenre() { Id = 13, Title = "Ethnic" });
            genres.Add(new VkGenre() { Id = 14, Title = "Acoustic & Vocal" });
            genres.Add(new VkGenre() { Id = 15, Title = "Reggae" });
            genres.Add(new VkGenre() { Id = 16, Title = "Classical" });
            genres.Add(new VkGenre() { Id = 17, Title = "Indie Pop" });
            genres.Add(new VkGenre() { Id = 19, Title = "Speech" });
            genres.Add(new VkGenre() { Id = 22, Title = "Electropop & Disco" });
            genres.Add(new VkGenre() { Id = 18, Title = "Other" });

            return genres;
        }
    }
}
