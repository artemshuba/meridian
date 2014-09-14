using System.Runtime.Serialization;

namespace Xbox.Music
{

    /// <summary>
    /// An individual or group who contributed work to an <see cref="Album"/> or <see cref="Track"/>.
    /// </summary>
    [DataContract]
    public class Contributor
    {

        /// <summary>
        /// The type of contribution, such as "Main" or "Featured".
        /// </summary>
        [DataMember]
        public string Role { get; set; }

        /// <summary>
        /// The contributing artist.
        /// </summary>
        [DataMember]
        public Artist Artist { get; set; }

    }

}