// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for a user token manager.
    /// The role of this component is to acquire valid user tokens that can be used to call the Groove API.
    /// </summary>
    public interface IUserTokenManager
    {
        bool UserIsSignedIn { get; }

        /// <summary>
        /// Get a valid user token that can be used to call the Groove API and format it to be used in the request's Authorization header.
        /// </summary>
        /// <param name="forceRefresh">If the API returned an INVALID_AUTHORIZATION_HEADER error you might need to force a token refresh.</param>
        Task<string> GetUserAuthorizationHeaderAsync(bool forceRefresh = false);
    }
}
