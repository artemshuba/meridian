using GalaSoft.MvvmLight.Messaging;

namespace Meridian.Utils.Messaging
{
    /// <summary>
    /// Message indicates that user logged in or logged out
    /// </summary>
    public class MessageUserAuthChanged : MessageBase
    {
        /// <summary>
        /// True if user is logged in
        /// </summary>
        public bool IsLoggedIn { get; set; }
    }
}
