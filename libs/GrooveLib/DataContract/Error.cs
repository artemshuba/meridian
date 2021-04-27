// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public class Error
    {
        [DataMember(EmitDefaultValue = false)]
        public string ErrorCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Message { get; set; }
    }
}
