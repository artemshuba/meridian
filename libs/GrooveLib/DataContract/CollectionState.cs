// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Runtime.Serialization;

    /// <summary>
    /// User's collection global information
    /// </summary>
    [DataContract(Namespace = Constants.Xmlns)]
    public class CollectionState
    {
        /// <summary>
        /// Token that indicates the current version of the collection
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Token { get; set; }

        /// <summary>
        /// Number of playlists in the collection
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PlaylistCount { get; set; }

        /// <summary>
        /// Number of playlists the user can add to his collection
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int RemainingPlaylistCount { get; set; }

        /// <summary>
        /// Number of tracks in the collection
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int TrackCount { get; set; }

        /// <summary>
        /// Number of tracks the user can add to his collection
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int RemainingTrackCount { get; set; }
    }
}
