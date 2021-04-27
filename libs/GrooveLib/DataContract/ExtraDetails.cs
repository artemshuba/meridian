// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System;
    using System.Runtime.Serialization;

    [Flags]
    public enum ExtraDetails : byte
    {
        None = 0,
        [EnumMember]
        Albums = 1, // For artists lookup or search
        [EnumMember]
        TopTracks = 2, // For artists lookup only
        [EnumMember]
        RelatedArtists = 4, // For artists lookup only
        [EnumMember]
        Tracks = 8, // For albums lookup or search
        [EnumMember]
        ArtistDetails = 16, // For albums and tracks lookup
        [EnumMember]
        AlbumDetails = 32, // For tracks lookup only
        [EnumMember]
        Artists = 64 // For search only
    }
}
