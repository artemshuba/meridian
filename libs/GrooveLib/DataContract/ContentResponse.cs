// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    
    [DataContract(Namespace = Constants.Xmlns)]
    public class ContentResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Artist> Artists { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Album> Albums { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Track> Tracks { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Playlist> Playlists { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Radio> Radios { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<ContentItem> Results { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Genres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Mood> CatalogMoods { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Activity> CatalogActivities { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Culture { get; set; }
    }
}
