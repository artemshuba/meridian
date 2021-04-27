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
    public class Artist : Content
    {
        // These items are available when this is the main element of the query or if an extra details parameter has been specified for that sub-element

        [DataMember(EmitDefaultValue = false)]
        public string Biography { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Genres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Subgenres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Artist> RelatedArtists { get; set; }

        // The following lists each require a specific extra details parameter, otherwise they will be null
        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Album> Albums { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PaginatedList<Track> TopTracks { get; set; }
    }

    [DataContract(Namespace = Constants.Xmlns)]
    public class Contributor
    {
        public const string MainRole = "Main";
        public const string DefaultRole = "Other";

        [DataMember(EmitDefaultValue = false)]
        public string Role { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Artist Artist { get; set; }

        public Contributor(string role, Artist artist)
        {
            this.Role = role;
            this.Artist = artist;
        }
    }
}
