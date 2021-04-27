// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System.IO;
    using System.Net.Http;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Groove.Api.DataContract.AuthenticationDataContract;

    /// <summary>
    /// Basic Microsoft Account Application authentication client
    /// </summary>
    public class MicrosoftAccountAuthenticationClient : SimpleServiceClient
    {
        private readonly Uri hostname = new Uri("https://login.live.com/");

        /// <summary>
        /// Authenticate an application on Microsoft Accounts
        /// </summary>
        /// <param name="clientId">The application's client ID</param>
        /// <param name="clientSecret">The application's secret</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<MicrosoftAccountAuthenticationResponse> AuthenticateAsync(string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            Dictionary<string, string> request = new Dictionary<string, string>()
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"scope", "app.music.xboxlive.com"},
                {"grant_type", "client_credentials"}
            };
            return PostAsync<MicrosoftAccountAuthenticationResponse, Dictionary<string, string>>(hostname, "/accesstoken.srf", request, cancellationToken);
        }

        protected override HttpContent CreateHttpContent<TRequest>(TRequest requestPayload, StreamWriter writer, MemoryStream stream)
        {
            // We need the url-encoded data for Microsoft Account authentication
            return new FormUrlEncodedContent(requestPayload as Dictionary<string, string>);
        }
    }
}
