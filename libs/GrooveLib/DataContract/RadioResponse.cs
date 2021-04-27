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

    [DataContract(Namespace = Constants.Xmlns)]
    public class RadioResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = false)]
        public string SessionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Track> Tracks { get; set; }

    }
}
