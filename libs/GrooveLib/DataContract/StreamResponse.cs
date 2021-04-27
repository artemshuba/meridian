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

    [DataContract(Namespace = Constants.Xmlns)]
    public class StreamResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }

        // HLS: application/vnd.apple.mpegurl
        // MP3: audio/mpeg
        [DataMember(EmitDefaultValue = false)]
        public string ContentType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? ExpiresOn { get; set; }
    }
}
