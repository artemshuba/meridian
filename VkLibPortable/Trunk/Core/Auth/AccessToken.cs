using System;

namespace VkLib.Core.Auth
{
    /// <summary>
    /// Access token
    /// </summary>
   public class AccessToken
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime ExpiresIn { get; set; }

        /// <summary>
        /// Has token expired
        /// </summary>
        public bool HasExpired
        {
            get { return DateTime.Now > ExpiresIn; }
        }

        /// <summary>
        /// User id associated with this token
        /// </summary>
        public long UserId { get; set; }
    }
}
