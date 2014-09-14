using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Xbox.Music
{
 
    [DataContract]
    internal class TokenResponse
    {

        #region Properties

        /// <summary>
        /// The access token that you can use to authenticate you access to the Xbox Music RESTful API.
        /// </summary>
        [DataMember(Name = "access_token")]
        internal string AccessToken { get; set; }

        /// <summary>
        /// The data type of the token. Currently, Azure Marketplace returns http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0, which indicates that a Simple Web Token will be returned.
        /// </summary>
        [DataMember(Name = "token_type")]
        internal string TokenType { get; set; }

        /// <summary>
        /// The number of seconds for which the access token is valid.
        /// </summary>
        [DataMember(Name = "expires_in")]
        internal int ExpiresInSeconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "scope")]
        internal string Scope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal DateTime TimeStamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal bool IsValid
        {
            get { return TimeStamp.AddSeconds(ExpiresInSeconds - 5) < DateTime.Now; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool NeedsRefresh
        {
            get { return TimeStamp.AddSeconds(ExpiresInSeconds - 30) < DateTime.Now; }
        }

        #endregion



    }
}
