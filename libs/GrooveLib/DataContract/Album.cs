// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public class Album : TrackContainer
    {
        [DataMember(EmitDefaultValue = false)]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? IsExplicit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LabelName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Genres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Subgenres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AlbumType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Subtitle { get; set; }

        // The following sub-element will be provided with just the minimal stuff unless an extra details parameter is specified for additional sub-element details
        [DataMember(EmitDefaultValue = false)]
        public List<Contributor> Artists { get; set; }

        public Album ShallowCopy()
        {
            return (Album) MemberwiseClone();
        }
    }
}
