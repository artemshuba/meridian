using System.Runtime.Serialization;

namespace Xbox.Music
{

    /// <summary>
    /// The root object returned by the service.
    /// </summary>
    [DataContract]
    public class ContentResponse
    {

        /// <summary>
        /// A paginated list of Artists that matched the request criteria.
        /// </summary>
        [DataMember]
        public PaginatedList<Artist> Artists { get; set; }

        /// <summary>
        /// A paginated list of Albums that matched the request criteria.
        /// </summary>
        [DataMember]
        public PaginatedList<Album> Albums { get; set; }

        /// <summary>
        /// A paginated list of Tracks that matched the request criteria.
        /// </summary>
        [DataMember]
        public PaginatedList<Track> Tracks { get; set; }

        /// <summary>
        /// Optional error.
        /// </summary>
        [DataMember]
        public Error Error { get; set; }

    }
}
