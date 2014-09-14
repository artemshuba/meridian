using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xbox.Music
{
    
    /// <summary>
    /// An individual piece of musical content from an <see cref="Album"/>. 
    /// </summary>
    [DataContract]
    public class Track : EntryBase
    {

        #region Properties

        /// <summary>
        /// Nullable. The track duration.
        /// </summary>
        [DataMember]
        public TimeSpan? Duration { get; set; }
        
        /// <summary>
        /// Nullable. The position of the track in the album.
        /// </summary>
        [DataMember]
        public int? TrackNumber { get; set; }

        /// <summary>
        /// Nullable. True if the track contains explicit content.
        /// </summary>
        [DataMember]
        public bool? IsExplicit { get; set; }

        /// <summary>
        /// The list of musical genres associated with this track.
        /// </summary>
        [DataMember]
        public List<string> Genres { get; set; }

        /// <summary>
        /// The list of distribution rights associated with this track in Xbox Music (for example, Stream, Purchase, and so on).
        /// </summary>
        [DataMember]
        public List<string> Rights { get; set; }

        /// <summary>
        /// The album this track belongs to.
        /// </summary>
        /// <remarks>
        /// Only a few fields are populated in this Album element, including the ID that should be used in a lookup request in order to have the full album properties.
        /// </remarks>
        [DataMember]
        public Album Album { get; set; }

        /// <summary>
        /// The list of contributors (artists and their roles) to the album.
        /// </summary>
        [DataMember]
        public List<Contributor> Artists { get; set; }

        #endregion

        #region Public Methods



        #endregion


    }

}
