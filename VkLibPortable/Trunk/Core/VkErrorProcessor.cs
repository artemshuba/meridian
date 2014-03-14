using System.Diagnostics;
using Newtonsoft.Json.Linq;
using VkLib.Error;

namespace VkLib.Core
{
    internal static class VkErrorProcessor
    {
        public static bool ProcessError(JObject response)
        {
            if (response["error"] != null)
            {
                if (response["error"]["error_code"] != null)
                {
                    if (response["error"]["error_code"].Value<string>() == "201")
                        throw new VkAccessDeniedException();
                    if (response["error"]["error_code"].Value<string>() == "221")
                        throw new VkStatusBroadcastDisabledException();
                    if (response["error"]["error_code"].Value<string>() == "5")
                        throw new VkInvalidTokenException();
                    if (response["error"]["error_code"].Value<string>() == "14")
                        throw new VkCaptchaNeededException(response["error"]["captcha_sid"].Value<string>(), response["error"]["captcha_img"].Value<string>());
                }
                if (response["error"].HasValues)
                {
                    Debug.WriteLine(response["error"]["error_code"].Value<string>() + ":" + response["error"]["error_msg"].Value<string>());
                    switch (response["error"]["error_code"].Value<int>())
                    {
                        case 7:
                            throw new VkAccessDeniedException();
                    }
                }
                else
                    switch (response["error"].Value<string>())
                    {
                        case "need_captcha":
                            throw new VkCaptchaNeededException(response["captcha_sid"].Value<string>(), response["captcha_img"].Value<string>());
                        default:
                            throw new VkException(response["error"].Value<string>(), response["error"].Value<string>());
                    }
                return true;
            }
            return false;
        }
    }
}
