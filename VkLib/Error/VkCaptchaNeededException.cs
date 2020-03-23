namespace VkLib.Error
{
    public class VkCaptchaNeededException : VkException
    {
        public string CaptchaSid { get; set; }

        public string CaptchaImg { get; set; }

        public VkCaptchaNeededException(string captchaSid, string captchaImg)
        {
            CaptchaSid = captchaSid;
            CaptchaImg = captchaImg;
        }
    }
}
