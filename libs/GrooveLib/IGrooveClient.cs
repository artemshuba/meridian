// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataContract;
    using DataContract.CollectionEdit;

    public interface IGrooveClient : IDisposable
    {
        /// <summary>
        /// Timeout applied to all backend service calls.
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Performs a media search.
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="query">Query string to search.</param>
        /// <param name="source">The content source: Catalog, Collection or both</param>
        /// <param name="filter">Filters the response items. Can be "Artists", "Albums", "Tracks" or any combination.</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <param name="maxItems">Max items per category in the response, between 1 and 25. Default value is 25.</param>
        /// <returns>Content response with lists of media items </returns>
        Task<ContentResponse> SearchAsync(
            MediaNamespace mediaNamespace,
            string query,
            ContentSource? source = null,
            SearchFilter filter = SearchFilter.Default,
            string language = null,
            string country = null,
            int? maxItems = null);

        /// <summary>
        /// Request the continuation of an incomplete list of content from the service.
        /// </summary>
        /// <param name="mediaNamespace">Must be the same as in the original request.</param>
        /// <param name="continuationToken">A Continuation Token provided in an earlier service response.</param>
        /// <returns>Content response with lists of media items.</returns>
        Task<ContentResponse> SearchContinuationAsync(
            MediaNamespace mediaNamespace,
            string continuationToken);

        /// <summary>
        /// Lookup a list of items and get details about them.
        /// </summary>
        /// <param name="itemIds">Ids to look up, each of which is prefixed by a namespace: {namespace.id}.</param>
        /// <param name="source">The content source: Catalog, Collection or both</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <param name="extras">Enumeration of extra details.</param>
        /// <returns>Content response with details about one or more items.</returns>
        Task<ContentResponse> LookupAsync(
            List<string> itemIds,
            ContentSource? source = null,
            string language = null,
            string country = null,
            ExtraDetails extras = ExtraDetails.None);

        /// <summary>
        /// Lookup an item and get details about it.
        /// </summary>
        /// <param name="itemId">Id to look up, prefixed by a namespace: {namespace.id}.</param>
        /// <param name="source">The content source: Catalog, Collection or both</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <param name="extras">Enumeration of extra details.</param>
        /// <returns>Content response with details about one or more items.</returns>
        Task<ContentResponse> LookupAsync(
            string itemId,
            ContentSource? source = null,
            string language = null,
            string country = null,
            ExtraDetails extras = ExtraDetails.None);

        /// <summary>
        /// Request the continuation of an incomplete list of content from the service. 
        /// The relative URL (i.e. the ids list) must be the same as in the original request.
        /// </summary>
        /// <param name="itemIds">Ids to look up, each of which is prefixed by a namespace: {namespace.id}.</param>
        /// <param name="continuationToken">A Continuation Token provided in an earlier service response.</param>
        /// <returns>Content response with details about one or more items.</returns>
        Task<ContentResponse> LookupContinuationAsync(
            List<string> itemIds,
            string continuationToken);

        /// <summary>
        /// Request the continuation of an incomplete list of content from the service. 
        /// The relative URL must be the same as in the original request.
        /// </summary>
        /// <param name="itemId">Id to look up, prefixed by a namespace: {namespace.id}.</param>
        /// <param name="continuationToken">A Continuation Token provided in an earlier service response.</param>
        /// <returns>Content response with details about one or more items.</returns>
        Task<ContentResponse> LookupContinuationAsync(
            string itemId,
            string continuationToken);

        /// <summary>
        /// Browse the catalog or your collection
        /// NB: You cannot combine the following filters together in the same request: genre, mood, activity.
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="source">A ContentSource value. Only Collection for now</param>
        /// <param name="type">The item type you want to browse</param>
        /// <param name="genre">Filter to a specific genre.</param>
        /// <param name="mood">Filter to a specific mood.</param>
        /// <param name="activity">Filter to a specific activity.</param>
        /// <param name="orderBy">Specify how results are ordered.</param>
        /// <param name="maxItems">Max items per category in the response, between 1 and 25. Default value is 25.</param>
        /// <param name="page">Go directly to a given page. Page size is maxItems.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <returns>Content response with the items corresponding to the browse request.</returns>
        Task<ContentResponse> BrowseAsync(
            MediaNamespace mediaNamespace,
            ContentSource source,
            ItemType type,
            string genre = null,
            string mood = null,
            string activity = null,
            OrderBy? orderBy = null,
            int? maxItems = null,
            int? page = null,
            string country = null,
            string language = null);

        /// <summary>
        /// Request the continuation of an incomplete browse response. The relative URL (i.e. mediaNamespace, source and type) must be the same as in the original request.
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="source">A ContentSource value. Only Collection for now.</param>
        /// <param name="type">The item type you want to browse</param>
        /// <param name="continuationToken">A Continuation Token provided in an earlier service response.</param>
        /// <returns>Content response with the items corresponding to the browse request.</returns>
        Task<ContentResponse> BrowseContinuationAsync(
            MediaNamespace mediaNamespace,
            ContentSource source,
            ItemType type,
            string continuationToken);

        /// <summary>
        /// Browse sub elements of the catalog or your collection
        /// </summary>
        /// <param name="id">Id of the parent item to browse.</param>
        /// <param name="source">A ContentSource value. Only Collection for now</param>
        /// <param name="browseType">The item type you want to browse</param>
        /// <param name="extra">The extra details to browse.</param>
        /// <param name="orderBy">Specify how results are ordered.</param>
        /// <param name="maxItems">Max items per category in the response, between 1 and 25. Default value is 25.</param>
        /// <param name="page">Go directly to a given page. Page size is maxItems.</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <returns>Content response with the items corresponding to the sub-browse request.</returns>
        Task<ContentResponse> SubBrowseAsync(
            string id,
            ContentSource source,
            BrowseItemType browseType,
            ExtraDetails extra,
            OrderBy? orderBy = null,
            int? maxItems = null,
            int? page = null,
            string language = null,
            string country = null);

        /// <summary>
        /// Request the continuation of an incomplete sub browse
        /// </summary>
        /// <param name="id">Id of the parent item to browse.</param>
        /// <param name="source">A ContentSource value. Only Collection for now</param>
        /// <param name="browseType">The item type you want to browse</param>
        /// <param name="extra">The extra details to browse.</param>
        /// <param name="continuationToken">A Continuation Token provided in an earlier service response.</param>
        /// <returns>Content response with the items corresponding to the sub-browse request.</returns>
        Task<ContentResponse> SubBrowseContinuationAsync(
            string id,
            ContentSource source,
            BrowseItemType browseType,
            ExtraDetails extra,
            string continuationToken);

        /// <summary>
        /// Get spotlight items of the moment
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <returns>Content response with spotlight items in order in the Results element of the response.</returns>
        Task<ContentResponse> SpotlightApiAsync(
            MediaNamespace mediaNamespace,
            string language = null,
            string country = null);

        /// <summary>
        /// Get new releases of the moment.
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="genre">A valid genre coherent with the locale to get specific new releases. If null, new releases are from all genres.</param>
        /// <param name="language">ISO 2 letter code.</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <returns>Content response with spotlight items in order in the Results element of the response.</returns>
        Task<ContentResponse> NewReleasesApiAsync(
            MediaNamespace mediaNamespace,
            string genre = null,
            string language = null,
            string country = null);

        /// <summary>
        /// Get a list of all posible genres for a given locale
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now</param>
        /// <param name="country">ISO 2 letter code</param>
        /// <param name="language">ISO 2 letter code</param>
        /// <returns>Response.Genres contains the list of genres for your locale</returns>
        Task<ContentResponse> BrowseGenresAsync(
            MediaNamespace mediaNamespace,
            string country = null,
            string language = null);

        /// <summary>
        /// Get a list of all posible moods for a given locale
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now</param>
        /// <param name="country">ISO 2 letter code</param>
        /// <param name="language">ISO 2 letter code</param>
        /// <returns>Response.Moods contains the list of moods for your locale</returns>
        Task<ContentResponse> BrowseMoodsAsync(
            MediaNamespace mediaNamespace,
            string country,
            string language);

        /// <summary>
        /// Get a list of all posible activities for a given locale
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now</param>
        /// <param name="country">ISO 2 letter code</param>
        /// <param name="language">ISO 2 letter code</param>
        /// <returns>Response.Moods contains the list of activities for your locale</returns>
        Task<ContentResponse> BrowseActivitiesAsync(
            MediaNamespace mediaNamespace,
            string country,
            string language);

        /// <summary>
        /// Edit your collection
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="operation">Operation to be done on the collection. Possible values are "add" and "delete".</param>
        /// <param name="trackActionRequest">List of track IDs to be processed.</param>
        /// <returns>List of TrackActionResults corresponding to the result for each track action. This shows which operations did fail and why</returns>
        Task<TrackActionResponse> CollectionOperationAsync(
            MediaNamespace mediaNamespace,
            TrackActionType operation,
            TrackActionRequest trackActionRequest);

        /// <summary>
        /// Edit a playlist
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="operation">Operation to be done on the playlist. Possible values are "create", "update" and "delete".</param>
        /// <param name="playlistAction">Playlist Id and List of TrackActions. A trackAction is a track ID and an operation (add/delete) to apply on the playlist.</param>
        /// <returns>PlaylistActionResult giving details on the playlist action and a list of TrackActionResults corresponding to the result for each track action. This shows which operations did fail and why</returns>
        Task<PlaylistActionResponse> PlaylistOperationAsync(
            MediaNamespace mediaNamespace,
            PlaylistActionType operation,
            PlaylistAction playlistAction);

        /// <summary>
        /// Stream a media
        /// </summary>
        /// <param name="id">Id of the media to be streamed</param>
        /// <param name="clientInstanceId">Client instance Id</param>
        /// <returns>Stream response containing the url, expiration date and content type</returns>
        Task<StreamResponse> StreamAsync(
            string id,
            string clientInstanceId);

        /// <summary>
        /// Get a 30s preview of a media
        /// </summary>
        /// <param name="id">Id of the media to be streamed</param>
        /// <param name="clientInstanceId">Client instance Id</param>
        /// <param name="country">ISO 2 letter code.</param>
        /// <returns>Stream response containing the url, expiration date and content type</returns>
        Task<StreamResponse> PreviewAsync(
            string id,
            string clientInstanceId,
            string country = null);

        /// <summary>
        /// Gets the user's profile
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="language">ISO 2 letter code</param>
        /// <param name="country">ISO 2 letter code</param>
        /// <returns></returns>
        Task<UserProfileResponse> GetUserProfileAsync(
            MediaNamespace mediaNamespace,
            string language = null,
            string country = null);

        /// <summary>
        /// Continue a radio given its radio Id or session Id
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="id"></param>
        /// <param name="maxItems">Max items in the response, between 1 and 25. Default value is 25.</param>
        /// <returns></returns>
        Task<RadioResponse> ContinueRadioAsync(
            MediaNamespace mediaNamespace,
            string id,
            int? maxItems = null);

        /// <summary>
        /// Continue a radio given a previous radio response
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="previousResponse">previous radio response</param>
        /// <param name="maxItems">Max items in the response, between 1 and 25. Default value is 25.</param>
        /// <returns>radio tracks</returns>
        Task<RadioResponse> ContinueRadioAsync(
            MediaNamespace mediaNamespace,
            RadioResponse previousResponse,
            int? maxItems = null);

        /// <summary>
        /// Continue a radio given its recently played response
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="radio">recently played radio</param>
        /// <param name="maxItems">Max items in the response, between 1 and 25. Default value is 25.</param>
        /// <returns>radio tracks</returns>
        Task<RadioResponse> ContinueRadioAsync(
            MediaNamespace mediaNamespace,
            Radio radio,
            int? maxItems = null);

        /// <summary>
        /// Browse recently played radio stations
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="maxItems">Max items in the response, between 1 and 25. Default value is 25.</param>
        /// <param name="page">Go directly to a given page. Page size is maxItems.</param>
        /// <returns></returns>
        Task<ContentResponse> GetRecentlyPlayedRadiosAsync(
            MediaNamespace mediaNamespace,
            int? maxItems = null,
            int? page = null);

        /// <summary>
        /// Create a new radio station
        /// </summary>
        /// <param name="mediaNamespace">"music" only for now.</param>
        /// <param name="createRadioRequest">the seed for the radio</param>
        /// <param name="maxItems">Max items in the response, between 1 and 25. Default value is 25.</param>
        /// <returns>a radio</returns>
        Task<RadioResponse> CreateRadioAsync(
            MediaNamespace mediaNamespace,
            CreateRadioRequest createRadioRequest,
            int? maxItems = null);
    }
}
