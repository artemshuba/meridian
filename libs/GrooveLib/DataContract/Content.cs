// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [DataContract(Namespace = Constants.Xmlns)]
    [Flags]
    public enum ContentSource : byte
    {
        Invalid = 0, // to avoid EmitDefaultValue=false "forgetting" the value

        [EnumMember]
        Catalog,
        [EnumMember]
        Collection
    }

    [DataContract(Namespace = Constants.Xmlns)]
    public abstract class Content
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ImageUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Link { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IdDictionary OtherIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentSource Source { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentSource CompatibleSources { get; set; }
    }
}
