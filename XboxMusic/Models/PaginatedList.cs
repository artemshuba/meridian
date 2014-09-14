using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xbox.Music
{

    /// <summary>
    /// Some of the methods in the Xbox Music RESTful API return lists of elements in their responses (for example, 
    /// search results, albums of an artist, tracks of an album, and so on). These lists can potentially be very large, 
    /// so we have put in place a mechanism to paginate these lists by using a continuation token. These lists and 
    /// tokens are returned in a PaginatedList object. 
    /// </summary>
    /// <typeparam name="T">The type of item (inheriting from EntryBase) that comprises the List.</typeparam>
    [DataContract]
    public class PaginatedList<T> where T: EntryBase
    {

        /// <summary>
        /// The items composing the paginated list.
        /// </summary>
        /// <remarks>
        /// When a list is of relatively small size, Items will contain the full list and ContinuationToken 
        /// will be null. However, the list may be incomplete, which is indicated by the value of ContinuationToken.
        /// </remarks>
        [DataMember]
        public List<T> Items { get; set; } 


        /// <summary>
        /// An opaque string that may be provided in a subsequent request to the same URL in order to continue the list.
        /// </summary>
        /// <remarks>
        ///  If ContinuationToken is not null, then the list is not complete, and the token may be used to retrieve the 
        /// remaining elements. A null value indicates that the list has no remaining items yet to be returned.
        /// </remarks>
        [DataMember]
        public string ContinuationToken { get; set; }

    }
}
