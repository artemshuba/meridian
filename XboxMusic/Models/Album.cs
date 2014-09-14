using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xbox.Music
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Album : EntryBase
    {

        #region Properties

        /// <summary>
        /// Nullable. The album release date.
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Nullable. The album total duration.
        /// </summary>
        [DataMember]
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Nullable. The number of tracks on the album.
        /// </summary>
        [DataMember]
        public int? TrackCount { get; set; }

        /// <summary>
        /// Nullable. True if the album contains explicit content.
        /// </summary>
        [DataMember]
        public bool? IsExplicit { get; set; }

        /// <summary>
        /// The name of the music label that produced this album.
        /// </summary>
        [DataMember]
        public string LabelName { get; set; }

        /// <summary>
        /// The list of musical genres associated with this album.
        /// </summary>
        [DataMember]
        public List<string> Genres { get; set; }

        /// <summary>
        /// The type of album (for example, Album, Single, and so on).
        /// </summary>
        [DataMember]
        public string AlbumType { get; set; }

        /// <summary>
        /// The list of distribution rights associated with this album in Xbox Music (for example, Stream, Purchase, and so on).
        /// </summary>
        [DataMember]
        public List<string> Rights { get; set; }

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