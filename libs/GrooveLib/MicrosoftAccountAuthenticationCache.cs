// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Groove.Api.DataContract.AuthenticationDataContract;

    /// <summary>
    /// Basic Microsoft Account Application authentication cache
    /// </summary>
    public class MicrosoftAccountAuthenticationCache
    {
        public class AccessToken
        {
            public string Token { get; set; }
            public DateTime Expiration { get; set; }
        }

        private readonly string clientId;
        private readonly string clientSecret;
        private AccessToken token;

        private readonly MicrosoftAccountAuthenticationClient client = new MicrosoftAccountAuthenticationClient();

        /// <summary>
        /// Cache a Microsoft application's authentication token
        /// </summary>
        /// <param name="clientId">The application's client ID</param>
        /// <param name="clientSecret">The application's secret</param>
        public MicrosoftAccountAuthenticationCache(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        /// <summary>
        /// Get the application's token. Renew it if needed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AccessToken> CheckAndRenewTokenAsync(CancellationToken cancellationToken)
        {
            if (token == null || token.Expiration < DateTime.UtcNow)
            {
                // This is not thread safe. Unfortunately, portable class library requirements prevent use of
                // asynchronous locking mechanisms. The risk here is authenticating multiple times in parallel
                // which is bad from a performance standpoint but is transparent from a functional standpoint.
                MicrosoftAccountAuthenticationResponse authenticationResponse =
                    await client.AuthenticateAsync(clientId, clientSecret, cancellationToken);
                if (authenticationResponse != null)
                {
                    token = new AccessToken
                    {
                        Token = authenticationResponse.AccessToken,
                        Expiration =
                            DateTime.UtcNow.Add(TimeSpan.FromSeconds(Convert.ToDouble(authenticationResponse.ExpiresIn)))
                    };
                }
            }

            return token;
        }
    }
}
