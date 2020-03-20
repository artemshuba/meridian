using System;

namespace VkLib.Error
{
    public class VkNeedValidationException : Exception
    {
        public Uri RedirectUri { get; set; }
    }
}
