using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using VkLib.Auth;
using VkLib.Error;
using VkLib.Extensions;

namespace VkLib.Core.Auth
{
    public class VkDirectAuthRequest
    {
        private readonly Vkontakte _vkontakte;
        private const string UserAgentGcm = "Android-GCM/1.5 (generic_x86 KK)";
        private static HttpClient _http;

        internal VkDirectAuthRequest(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(_vkontakte.UserAgent);
        }

        /// <summary>
        /// <para>Direct auth with login and password.</para>
        /// <para>See also: <seealso cref="http://vk.com/pages?oid=-1&p=Прямая_авторизация"/></para>
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <param name="scopeSettings">Scope settings</param>
        /// <param name="captchaSid">Captcha sid</param>
        /// <param name="captchaKey">Captcha key</param>
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> Login(string login, string password, VkScopeSettings scopeSettings = VkScopeSettings.CanAccessFriends,
            string captchaSid = null, string captchaKey = null)
        {
            if (string.IsNullOrEmpty(_vkontakte.AppId))
                throw new NullReferenceException("App id must be specified.");

            if (string.IsNullOrEmpty(_vkontakte.ClientSecret))
                throw new NullReferenceException("Client secret must be specified.");

            var receipt = await GetGcmReceipt();

            var parameters = new Dictionary<string, string>
            {
                {"username", login},
                {"password", password},
                {"grant_type", "password"},
                {"scope", ((int) scopeSettings).ToString(CultureInfo.InvariantCulture)}
            };

            if (!string.IsNullOrEmpty(captchaSid) && !string.IsNullOrEmpty(captchaKey))
            {
                parameters.Add("captcha_sid", captchaSid);
                parameters.Add("captcha_key", captchaKey);
            }

            parameters.Add("client_id", _vkontakte.AppId);
            parameters.Add("client_secret", _vkontakte.ClientSecret);

            var request = new VkRequest(new Uri(VkConst.DirectAuthUrl), parameters);
            var response = await request.Execute();

            if (response["error"] != null)
            {
                switch (response["error"].Value<string>())
                {
                    case "need_captcha":
                        throw new VkCaptchaNeededException(response["captcha_sid"].Value<string>(), response["captcha_img"].Value<string>());

                    case "invalid_client":
                        throw new VkInvalidClientException();

                    case "need_validation":
                        throw new VkNeedValidationException() { RedirectUri = new Uri(response["redirect_uri"].Value<string>()) };
                }

                return null;
            }

            if (response["user_id"] == null) throw new InvalidDataException($"user_id is null! {response}");
            var nonRefreshedToken = response["access_token"].ToString();

            var refreshParameters = new Dictionary<string, string> {
                {"access_token", nonRefreshedToken},
                {"receipt", receipt},
                {"v", "5.92"}
            };
            var refreshRequest = new VkRequest(new Uri(VkConst.MethodBase + "auth.refreshToken"), refreshParameters);
            var refreshResponse = await refreshRequest.Execute();

            if (refreshResponse["token"] == null) throw new InvalidDataException($"token is null! {refreshResponse}");
            if (refreshResponse["token"].ToString() == nonRefreshedToken)
                throw new InvalidOperationException($"token {nonRefreshedToken} not refreshed!");

            var token = new AccessToken {
                Token = refreshResponse["token"].Value<string>(),
                UserId = response["user_id"].Value<long>(),
                ExpiresIn = response["expires_in"].Value<long>() == 0
                    ? DateTime.MaxValue
                    : DateTimeExtensions.UnixTimeStampToDateTime(response["expires_in"].Value<long>())
            };
            _vkontakte.AccessToken = token;
            return token;
        }

        /// <summary>
        ///     Get receipt for authorization w/ access to audio.* methods.
        /// </summary>
        /// <returns>Receipt to refresh token with.</returns>
        /// <exception cref="InvalidOperationException">If token retrieval failed.</exception>
        private static async Task<string> GetGcmReceipt() {
            AndroidCheckinResponse protoCheckIn;
            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/checkin")) {
                requestMessage.Headers.UserAgent.ParseAdd(UserAgentGcm);
                requestMessage.Headers.Add("Expect", "");
                requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-protobuffer");

                var payload = new byte[] {
                    0x10, 0x00, 0x1a, 0x2a, 0x31, 0x2d, 0x39, 0x32, 0x39, 0x61, 0x30, 0x64, 0x63, 0x61, 0x30, 0x65,
                    0x65, 0x65, 0x35, 0x35, 0x35, 0x31, 0x33, 0x32, 0x38, 0x30, 0x31, 0x37, 0x31, 0x61, 0x38, 0x35,
                    0x38, 0x35, 0x64, 0x61, 0x37, 0x64, 0x63, 0x64, 0x33, 0x37, 0x30, 0x30, 0x66, 0x38, 0x22, 0xe3,
                    0x01, 0x0a, 0xbf, 0x01, 0x0a, 0x45, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38,
                    0x36, 0x2f, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x5f, 0x73, 0x64, 0x6b, 0x5f, 0x78, 0x38, 0x36,
                    0x2f, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36, 0x3a, 0x34, 0x2e, 0x34,
                    0x2e, 0x32, 0x2f, 0x4b, 0x4b, 0x2f, 0x33, 0x30, 0x37, 0x39, 0x31, 0x38, 0x33, 0x3a, 0x65, 0x6e,
                    0x67, 0x2f, 0x74, 0x65, 0x73, 0x74, 0x2d, 0x6b, 0x65, 0x79, 0x73, 0x12, 0x06, 0x72, 0x61, 0x6e,
                    0x63, 0x68, 0x75, 0x1a, 0x0b, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36,
                    0x2a, 0x07, 0x75, 0x6e, 0x6b, 0x6e, 0x6f, 0x77, 0x6e, 0x32, 0x0e, 0x61, 0x6e, 0x64, 0x72, 0x6f,
                    0x69, 0x64, 0x2d, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x40, 0x85, 0xb5, 0x86, 0x06, 0x4a, 0x0b,
                    0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36, 0x50, 0x13, 0x5a, 0x19, 0x41,
                    0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x20, 0x53, 0x44, 0x4b, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x74,
                    0x20, 0x66, 0x6f, 0x72, 0x20, 0x78, 0x38, 0x36, 0x62, 0x07, 0x75, 0x6e, 0x6b, 0x6e, 0x6f, 0x77,
                    0x6e, 0x6a, 0x0e, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x5f, 0x73, 0x64, 0x6b, 0x5f, 0x78, 0x38,
                    0x36, 0x70, 0x00, 0x10, 0x00, 0x32, 0x06, 0x33, 0x31, 0x30, 0x32, 0x36, 0x30, 0x3a, 0x06, 0x33,
                    0x31, 0x30, 0x32, 0x36, 0x30, 0x42, 0x0b, 0x6d, 0x6f, 0x62, 0x69, 0x6c, 0x65, 0x3a, 0x4c, 0x54,
                    0x45, 0x3a, 0x48, 0x00, 0x32, 0x05, 0x65, 0x6e, 0x5f, 0x55, 0x53, 0x38, 0xf0, 0xb4, 0xdf, 0xa6,
                    0xb9, 0x9a, 0xb8, 0x83, 0x8e, 0x01, 0x52, 0x0f, 0x33, 0x35, 0x38, 0x32, 0x34, 0x30, 0x30, 0x35,
                    0x31, 0x31, 0x31, 0x31, 0x31, 0x31, 0x30, 0x5a, 0x00, 0x62, 0x10, 0x41, 0x6d, 0x65, 0x72, 0x69,
                    0x63, 0x61, 0x2f, 0x4e, 0x65, 0x77, 0x5f, 0x59, 0x6f, 0x72, 0x6b, 0x70, 0x03, 0x7a, 0x1c, 0x37,
                    0x31, 0x51, 0x36, 0x52, 0x6e, 0x32, 0x44, 0x44, 0x5a, 0x6c, 0x31, 0x7a, 0x50, 0x44, 0x56, 0x61,
                    0x61, 0x65, 0x45, 0x48, 0x49, 0x74, 0x64, 0x2b, 0x59, 0x67, 0x3d, 0xa0, 0x01, 0x00, 0xb0, 0x01, 0x00
                };
                requestMessage.Content = new ByteArrayContent(payload);
                var result = await _http.SendAsync(requestMessage);
                result.EnsureSuccessStatusCode();
                protoCheckIn =
                    Serializer.Deserialize<AndroidCheckinResponse>(result.Content.ReadAsStreamAsync().Result);
            }

            using (var tcp = new TcpClient("mtalk.google.com", 5228)) {
                // Body data (protobuf, but i didn't found any .proto files, so...)
                byte[] message1 = {
                    0x0a, 0x0a, 0x61, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x2d, 0x31, 0x39, 0x12, 0x0f, 0x6d, 0x63,
                    0x73, 0x2e, 0x61, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x2e, 0x63, 0x6f, 0x6d, 0x1a
                };
                byte[] message2 = {0x22};
                byte[] message3 = {0x2a};
                byte[] message4 = {0x32};
                byte[] message5 = {
                    0x42, 0x0b, 0x0a, 0x06, 0x6e, 0x65, 0x77, 0x5f, 0x76, 0x63, 0x12, 0x01, 0x31, 0x60, 0x00, 0x70,
                    0x01, 0x80, 0x01, 0x02, 0x88, 0x01, 0x01
                };
                byte[] message6 = {0x29, 0x02};

                var idStringBytes = Encoding.ASCII.GetBytes(protoCheckIn.AndroidId.ToString());
                var idLen = Utils.VarIntWrite(idStringBytes.Length).ToList();

                var tokenStringBytes = Encoding.ASCII.GetBytes(protoCheckIn.SecurityToken.ToString());
                var tokenLen = Utils.VarIntWrite(tokenStringBytes.Length).ToList();

                var hexId = "android-" + protoCheckIn.AndroidId;
                var hexIdBytes = Encoding.ASCII.GetBytes(hexId);
                var hexIdLen = Utils.VarIntWrite(hexIdBytes.Length);

                var body = message1
                    .Concat(idLen)
                    .Concat(idStringBytes)
                    .Concat(message2)
                    .Concat(idLen)
                    .Concat(idStringBytes)
                    .Concat(message3)
                    .Concat(tokenLen)
                    .Concat(tokenStringBytes)
                    .Concat(message4)
                    .Concat(hexIdLen)
                    .Concat(hexIdBytes)
                    .Concat(message5)
                    .ToList();
                var bodyLen = Utils.VarIntWrite(body.Count);

                var payload = message6
                    .Concat(bodyLen)
                    .Concat(body)
                    .ToArray();
                //

                using (var ssl = new SslStream(tcp.GetStream(), false,
                    (sender, certificate, chain, errors) => errors == SslPolicyErrors.None)) {
                    ssl.AuthenticateAsClient("mtalk.google.com");
                    ssl.Write(payload);
                    ssl.Flush();
                    ssl.ReadByte(); // skip byte
                    var responseCode = ssl.ReadByte();
                    if (responseCode != 3) // success if second byte == 3
                        throw new InvalidOperationException($"MTalk failed, expected 3, got {responseCode}");
                }
            }

            var appid = Utils.GenerateRandomString(11);

            string receipt;

            using (var requestMessage1 =
                new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/register3")) {
                requestMessage1.Headers.UserAgent.ParseAdd(UserAgentGcm);
                requestMessage1.Headers.TryAddWithoutValidation("Authorization",
                    $"AidLogin {protoCheckIn.AndroidId}:{protoCheckIn.SecurityToken}");

                var param = new Dictionary<string, string> {
                    {"X-scope", "GCM"},
                    {"X-osv", "23"},
                    {"X-subtype", "54740537194"},
                    {"X-app_ver", "443"},
                    {"X-kid", "|ID|1|"},
                    {"X-appid", appid},
                    {"X-gmsv", "13283005"},
                    {"X-cliv", "iid-10084000"},
                    {"X-app_ver_name", "51.2 lite"},
                    {"X-X-kid", "|ID|1|"},
                    {"X-subscription", "54740537194"},
                    {"X-X-subscription", "54740537194"},
                    {"X-X-subtype", "54740537194"},
                    {"app", "com.perm.kate_new_6"},
                    {"sender", "54740537194"},
                    {"device", Convert.ToString(protoCheckIn.AndroidId)},
                    {"cert", "966882ba564c2619d55d0a9afd4327a38c327456"},
                    {"app_ver", "443"},
                    {"info", "g57d5w1C4CcRUO6eTSP7b7VoT8yTYhY"},
                    {"gcm_ver", "13283005"},
                    {"plat", "0"},
                    {"X-messenger2", "1"}
                };

                requestMessage1.Content = new FormUrlEncodedContent(param);
                var result1 = await _http.SendAsync(requestMessage1);
                result1.EnsureSuccessStatusCode();
                var body1 = await result1.Content.ReadAsStringAsync();
                if (body1.Contains("Error"))
                    throw new InvalidOperationException($"C2DM registration #1 error ({body1})");

                param["X-scope"] = "id";
                param["X-kid"] = "|ID|2|";
                param["X-X-kid"] = "|ID|2|";

                using (var requestMessage2 = new HttpRequestMessage(HttpMethod.Post,
                    "https://android.clients.google.com/c2dm/register3")) {
                    requestMessage2.Headers.UserAgent.ParseAdd(UserAgentGcm);
                    requestMessage2.Headers.TryAddWithoutValidation("Authorization",
                        $"AidLogin {protoCheckIn.AndroidId}:{protoCheckIn.SecurityToken}");
                    requestMessage2.Content = new FormUrlEncodedContent(param);
                    var result2 = await _http.SendAsync(requestMessage2);

                    result2.EnsureSuccessStatusCode();
                    var body2 = await result2.Content.ReadAsStringAsync();
                    if (body2.Contains("Error"))
                        throw new InvalidOperationException($"C2DM registration #2 error ({body2})");

                    receipt = body2.Substring(13);
                }
            }

            return receipt;
        }
    }
}
