// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System.Collections.Generic;

    public static class GrooveClientFactory
    {
        /// <summary>
        /// We will reuse MSA tokens as much as possible.
        /// </summary>
        private static readonly Dictionary<string, MicrosoftAccountAuthenticationCache> MicrosoftAccountAuthenticationCaches =
            new Dictionary<string, MicrosoftAccountAuthenticationCache>();

        /// <summary>
        /// Create a Groove client with user authentication.
        /// </summary>
        /// <param name="microsoftAppClientId">Microsoft application client id</param>
        /// <param name="microsoftAppClientSecret">Microsoft application secret</param>
        /// <param name="userTokenManager"><see cref="IUserTokenManager"/> implementation</param>
        public static IGrooveClient CreateGrooveClient(
            string microsoftAppClientId, 
            string microsoftAppClientSecret, 
            IUserTokenManager userTokenManager)
        {
            return new GrooveClient(
                GetOrAddMicrosoftAccountAuthenticationCache(microsoftAppClientId, microsoftAppClientSecret), 
                userTokenManager);
        }

        /// <summary>
        /// Create a Groove client without user authentication.
        /// </summary>
        /// <param name="microsoftAppClientId">Microsoft application client id</param>
        /// <param name="microsoftAppClientSecret">Microsoft application secret</param>
        public static IGrooveClient CreateGrooveClient(
            string microsoftAppClientId,
            string microsoftAppClientSecret)
        {
            return new GrooveClient(GetOrAddMicrosoftAccountAuthenticationCache(microsoftAppClientId, microsoftAppClientSecret));
        }

        private static MicrosoftAccountAuthenticationCache GetOrAddMicrosoftAccountAuthenticationCache(
            string microsoftAppClientId,
            string microsoftAppClientSecret)
        {
            if (!MicrosoftAccountAuthenticationCaches.ContainsKey(microsoftAppClientId))
            {
                MicrosoftAccountAuthenticationCache microsoftAccountAuthenticationCache = new MicrosoftAccountAuthenticationCache(
                    microsoftAppClientId,
                    microsoftAppClientSecret);

                MicrosoftAccountAuthenticationCaches[microsoftAppClientId] = microsoftAccountAuthenticationCache;
            }

            return MicrosoftAccountAuthenticationCaches[microsoftAppClientId];
        }
    }
}
