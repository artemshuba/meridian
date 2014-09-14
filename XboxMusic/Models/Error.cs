using System.Runtime.Serialization;

namespace Xbox.Music
{

    [DataContract]
    public class Error
    {

        /// <summary>
        /// The error code, as described in the following table of error codes.
        /// </summary>
        [DataMember]
        public string ErrorCode { get; set; }


        /// <summary>
        /// A user-friendly description of the error code.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// A more contextual message describing what may have gone wrong.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

    }
}
