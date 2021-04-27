// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract.CollectionEdit
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public class TrackActionResult : ActionResult
    {
        [DataMember(EmitDefaultValue = false)]
        public string InputId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }
    }
}
