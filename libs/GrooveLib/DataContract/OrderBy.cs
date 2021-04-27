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
    public enum OrderBy : byte
    {
        None = 0,
        [EnumMember]
        AllTimePlayCount,               // Catalog
        [EnumMember]
        ReleaseDate,                    // Catalog & Collection
        [EnumMember]
        ArtistName,                     // Collection's artists, albums, tracks
        [EnumMember]
        AlbumTitle,                     // Collection's albums, tracks
        [EnumMember]
        TrackTitle,                     // Collection's tracks
        [EnumMember]
        GenreName,                      // Collection's albums, tracks
        [EnumMember]
        CollectionDate,                 // Collection : date added to the collection
        [EnumMember]
        TrackNumber,                    // Collection lookup of an album's tracks
        [EnumMember]
        MostPopular                     // Catalog
    }
}
