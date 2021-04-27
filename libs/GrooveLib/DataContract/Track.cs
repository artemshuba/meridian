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
    public class Track : Content
    {
        // These items are available when this is the main element of the query or if an extra details parameter has been specified for that sub-element
        [DataMember(EmitDefaultValue = false)]
        public DateTime? ReleaseDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TimeSpan? Duration { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? TrackNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? IsExplicit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Genres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GenreList Subgenres { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public RightList Rights { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Subtitle { get; set; }

        /// <summary>
        /// Possible values are:
        ///     - audio/mp3
        ///     - audio/aac
        ///     - audio/wma
        ///     - audio/mp4
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ContentType { get; set; }

        // This sub-element is null when this Track is queried as a sub-element of an album (to avoid looping), populated with just the minimal stuff by default when this Track is the main element, and extra details can obtained with a details parameter
        [DataMember(EmitDefaultValue = false)]
        public Album Album { get; set; }

        // This sub-element populated with just the minimal stuff by default when this Track is the main element, and extra details can obtained with a details parameter
        [DataMember(EmitDefaultValue = false)]
        public List<Contributor> Artists { get; set; }
    }
}
