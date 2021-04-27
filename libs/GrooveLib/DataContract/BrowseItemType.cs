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
    /// Represents a single item type that has subitems browing capabilities.
    /// For example, an artist has albums that can be browsed, an album has tracks that can be browsed, etc.
    /// </summary>
    [DataContract(Namespace = Constants.Xmlns)]
    public enum BrowseItemType : byte
    {
        Invalid = 0,
        [EnumMember]
        Album = 1,
        [EnumMember]
        Artist = 2,
        [EnumMember]
        Playlist = 3
    }
}
