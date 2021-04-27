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

    /// <summary>
    /// User's subscription information
    /// </summary>
    [DataContract(Namespace = Constants.Xmlns)]
    public class SubscriptionState
    {
        /// <summary>
        /// Type of the active subscription: possible values at the moment are "Paid" and "PreTrial"
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Type { get; set; }

        /// <summary>
        /// Two-letter region code of the subscription. This usually matches the user's account country, but not necessarily.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Region { get; set; }

        /// <summary>
        /// Expiration date of the current subscription
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? EndDate { get; set; }
    }
}
