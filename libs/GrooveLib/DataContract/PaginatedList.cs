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

    public interface IPaginatedList<out T>
    {
        IEnumerable<T> ReadOnlyItems { get; }
        string ContinuationToken { get; set; }
        int TotalItemCount { get; set; }
    }

    [DataContract(Namespace = Constants.Xmlns)]
    public class PaginatedList<T> : IPaginatedList<T>
    {
        [DataMember(EmitDefaultValue = false)]
        public List<T> Items { get; set; }

        public IEnumerable<T> ReadOnlyItems => Items;

        [DataMember(EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// An estimate count of the total number of items available in the list
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int TotalItemCount { get; set; }

        public PaginatedList()
        {
            Items = new List<T>();
        }
    }
}
