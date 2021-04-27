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

    /// <summary>
    /// See http://msdn.microsoft.com/en-us/library/aa347850.aspx, paragraph "Customizing Collection Types"
    /// </summary>
    [CollectionDataContract(Namespace = Constants.Xmlns, ItemName = "Genre")]
    public class GenreList : List<string>
    {
        public GenreList()
        {}

        public GenreList(IEnumerable<string> collection)
            : base(collection)
        {}
    }

    [CollectionDataContract(Namespace = Constants.Xmlns, ItemName = "Right")]
    public class RightList : List<string>
    {
        public RightList()
        {}

        public RightList(IEnumerable<string> collection)
            : base(collection)
        {}
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:Mark ISerializable types with SerializableAttribute", Justification = "SerializableAttribute does not exist in Portable .NET")]
    [CollectionDataContract(Namespace = Constants.Xmlns, ItemName = "OtherId", KeyName = "Namespace", ValueName = "Id")]
    public class IdDictionary : Dictionary<string, string>
    {
        public IdDictionary()
        {}

        public IdDictionary(IDictionary<string, string> dictionary)
            : base(dictionary)
        {}
    }

    [CollectionDataContract(Namespace = Constants.Xmlns, ItemName = "TrackId")]
    public class TrackIdList : List<string>
    {
        public TrackIdList()
        {}

        public TrackIdList(IEnumerable<string> collection)
            : base(collection)
        {}
    }
}
