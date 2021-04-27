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
    public class UserProfileResponse : BaseResponse
    {
        /// <summary>
        /// If the request is authenticated, provides the user's subscription state.
        /// Will be null if the request is unauthenticated.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? HasSubscription { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public bool IsSubscriptionAvailableForPurchase { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Culture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public CollectionState Collection { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SubscriptionState Subscription { get; set; }
    }
}
