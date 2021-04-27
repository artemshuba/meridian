// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [DataContract(Namespace = Constants.Xmlns)]
    public class ContentItem
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Artist Artist { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Album Album { get; set; }
    }
}
