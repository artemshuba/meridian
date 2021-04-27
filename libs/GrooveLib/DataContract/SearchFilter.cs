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

    [Flags]
    public enum SearchFilter : byte
    {
        [EnumMember]
        Artists = 1,
        [EnumMember]
        Albums = 2,
        [EnumMember]
        Tracks = 4,
        [EnumMember]
        Default = Artists | Albums | Tracks
    }
}
