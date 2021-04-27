// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public enum ItemType : byte
    {
        Invalid = 0,
        [EnumMember]
        Albums = 1,
        [EnumMember]
        Artists = 2,
        [EnumMember]
        Playlists = 3,
        [EnumMember]
        Tracks = 4,
        [EnumMember]
        PlaylistsForYou = 5,
        AlbumOrTrack = 42
    }
}
