// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System;
    using System.Net;

    public enum ErrorCode
    {
        // ReSharper disable InconsistentNaming
        #region Catalog errors
        [Description("No response from Catalog")]
        [StatusCode(HttpStatusCode.BadGateway)]
        CATALOG_UNAVAILABLE,

        [Description("Item does not exist")]
        [StatusCode(HttpStatusCode.NotFound)]
        CATALOG_NO_RESULT,

        [Description("Error while reading catalog data, some results may be missing or incomplete")]
        CATALOG_INVALID_DATA,
        #endregion

        #region Collection errors
        [Description("No result from Collection")]
        [StatusCode(HttpStatusCode.NotFound)]
        COLLECTION_NO_RESULT,

        [Description("Invalid response from Cloud Collection")]
        [StatusCode(HttpStatusCode.BadGateway)]
        COLLECTION_INVALID_RESPONSE,

        [Description("Error while reading collection data, some results may be missing or incomplete")]
        COLLECTION_INVALID_DATA,

        [Description("Your collection is full")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_FULL,

        [Description("This playlist is full")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_PLAYLIST_FULL,

        [Description("This operation is not supported")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_INVALID_OPERATION,

        [Description("Invalid collection id for this operation")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_INVALID_ID,

        [Description("Required playlist information is missing")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_INVALID_PLAYLIST_INFO,

        [Description("The operation failed")]
        [StatusCode(HttpStatusCode.BadGateway)]
        COLLECTION_OPERATION_UNKNOWN_ERROR,

        [Description("Some of the operations failed")]
        COLLECTION_SOME_OPERATIONS_FAILED,

        [Description("Concurrent update of the collection")]
        [StatusCode(HttpStatusCode.Conflict)]
        COLLECTION_CONCURRENT_UPDATE,

        [Description("Invalid playlist reorder operation")]
        [StatusCode(HttpStatusCode.BadRequest)]
        COLLECTION_INVALID_PLAYLIST_REORDER,
        #endregion

        #region Discovery errors
        [Description("No result from Discovery")]
        [StatusCode(HttpStatusCode.NotFound)]
        DISCOVERY_NO_RESULT,

        [Description("No result from Discovery, you might want to check if genre is well formatted")]
        [StatusCode(HttpStatusCode.NotFound)]
        DISCOVERY_INVALID_GENRE,

        [Description("Invalid response from Discovery")]
        [StatusCode(HttpStatusCode.BadGateway)]
        DISCOVERY_INVALID_RESPONSE,

        [Description("Error while reading discovery data, some results may be missing or incomplete")]
        DISCOVERY_INVALID_DATA,

        [Description("Error while reading recent plays data, some results may be missing or incomplete")]
        RECENTPLAYS_INVALID_DATA,
        #endregion

        #region Delivery errors
        [Description("Invalid response from the Music Delivery Service")]
        [StatusCode(HttpStatusCode.BadGateway)]
        DELIVERY_INVALID_RESPONSE,

        [Description("Content is not available for streaming")]
        [StatusCode(HttpStatusCode.NotFound)]
        DELIVERY_UNAVAILABLE_CONTENT,

        [Description("The user is already streaming on another device")]
        [StatusCode(HttpStatusCode.Conflict)]
        DELIVERY_CONCURRENT_STREAMING,

        [Description("Purchases are not available for streaming through the Groove Developer APIs")]
        [StatusCode(HttpStatusCode.Forbidden)]
        DELIVERY_3RD_PARTY_CANNOT_STREAM_PURCHASES,

        [Description("Unknown delivery error")]
        [StatusCode(HttpStatusCode.NotFound)]
        DELIVERY_ERROR,
        #endregion

        #region Subscription errors
        [Description("Invalid response from the Subscription State Service")]
        [StatusCode(HttpStatusCode.BadGateway)]
        SUBSCRIPTION_INVALID_RESPONSE,
        #endregion

        #region AccessToken errors
        [Description("Application access token required")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        ACCESS_TOKEN_MISSING,

        [Description("Invalid Application access token")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        ACCESS_TOKEN_INVALID,

        [Description("Expired Application access token")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        ACCESS_TOKEN_EXPIRED,

        [Description("Unexpected error while validating Application access token")]
        [StatusCode(HttpStatusCode.InternalServerError)]
        ACCESS_TOKEN_VALIDATION_ERROR,

        [Description("The Microsoft Application is not a subscriber to the Groove data offer")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        ACCESS_TOKEN_INVALID_SUBSCRIPTION,

        [Description("Unexpected error while validating Microsoft App subscription status")]
        [StatusCode(HttpStatusCode.InternalServerError)]
        ACCESS_TOKEN_SUBSCRIPTION_VALIDATION_ERROR,
        #endregion

        #region ContinuationToken errors
        [Description("The continuation token provided is incorrect")]
        [StatusCode(HttpStatusCode.BadRequest)]
        CONTINUATION_TOKEN_INVALID_ERROR,
        #endregion

        #region General input validation errors
        [Description("Missing or invalid authorization header")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        INVALID_AUTHORIZATION_HEADER,

        [Description("User authorization token expired")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        AUTHORIZATION_TOKEN_EXPIRED,

        [Description("User needs to grant consent to the MSA scope MicrosoftMediaServices.GrooveApiAccess")]
        [StatusCode(HttpStatusCode.Unauthorized)]
        MSA_SCOPE_REQUIRES_USER_CONSENT,

        [Description("The user does not have a Groove Music Pass")]
        [StatusCode(HttpStatusCode.Forbidden)]
        NO_MUSIC_PASS_SUBSCRIPTION,

        [Description("Missing or empty mandatory parameter")]
        [StatusCode(HttpStatusCode.BadRequest)]
        MISSING_INPUT_PARAMETER,

        [Description("Invalid parameter value")]
        [StatusCode(HttpStatusCode.BadRequest)]
        INVALID_INPUT_PARAMETER,

        [Description("Incompatible parameters")]
        [StatusCode(HttpStatusCode.BadRequest)]
        INCOMPATIBLE_INPUT_PARAMETERS,

        [Description("Unauthorized input parameter")]
        UNAUTHORIZED_INPUT_PARAMETER,

        [Description("Unauthorized API method")]
        [StatusCode(HttpStatusCode.Forbidden)]
        UNAUTHORIZED_API_METHOD,

        [Description("The requested functionality is not available in this region")]
        [StatusCode(HttpStatusCode.BadRequest)]
        INVALID_COUNTRY,
        #endregion

        [Description("Oops, something went seriously wrong")]
        [StatusCode(HttpStatusCode.InternalServerError)]
        INTERNAL_SERVER_ERROR,

        [Description("Too Many Requests")]
        [StatusCode((HttpStatusCode)429)] // http://tools.ietf.org/html/rfc6585
        TOO_MANY_REQUESTS,
        // ReSharper restore InconsistentNaming
    }

    public class StatusCodeAttribute : Attribute
    {
        public HttpStatusCode StatusCode { get; private set; }

        public StatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }

    public class DescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
