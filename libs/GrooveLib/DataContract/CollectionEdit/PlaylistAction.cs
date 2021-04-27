// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract.CollectionEdit
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public class PlaylistAction : IPlaylistEditableMetadata
    {
        /// <summary>
        /// Collection state token. If provided, we will enforce more validation.
        /// We will first check that this token is up-to-date before doing any update action.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CollectionStateToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? IsPublished { get; set; }

        /// <summary>
        /// Only used for TrackActionType.Move
        /// Track before which the set of tracks should be inserted,
        /// or null if we insert at the end of the playlist
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string InsertBeforeTrackId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<TrackAction> TrackActions { get; set; }
    }
}
