// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Groove.Api.DataContract
{
    /// <summary>
    /// Describes a concept that categorizes a content.
    /// For example, a mood can be associated to some content and content can be filtered in this category.
    /// </summary>
    [DataContract(Namespace = Constants.Xmlns)]
    public class ContentCategory
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Some category values can be children of others.
        /// For example a genre can be a sub-genre of another genre ("Commercial Pop" is a child of "Pop").
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ParentId { get; set; }
    }
}
