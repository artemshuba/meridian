// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract.AuthenticationDataContract
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MicrosoftAccountAuthenticationResponse
    {
        [DataMember(Name = "token_type", EmitDefaultValue = false)]
        public string TokenType { get; set; }
        [DataMember(Name = "access_token", EmitDefaultValue = false)]
        public string AccessToken { get; set; }
        [DataMember(Name = "expires_in", EmitDefaultValue = false)]
        public string ExpiresIn { get; set; }
        [DataMember(Name = "scope", EmitDefaultValue = false)]
        public string Scope { get; set; }
        [DataMember(Name = "error", EmitDefaultValue = false)]
        public string Error { get; set; }
        [DataMember(Name = "error_description", EmitDefaultValue = false)]
        public string ErrorDescription { get; set; }
    }
}
