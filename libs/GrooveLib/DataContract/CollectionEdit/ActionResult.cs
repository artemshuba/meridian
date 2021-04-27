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
    public class ActionResult
    {
        [DataMember(EmitDefaultValue = false)]
        public Error Error { get; set; }
    }
}
