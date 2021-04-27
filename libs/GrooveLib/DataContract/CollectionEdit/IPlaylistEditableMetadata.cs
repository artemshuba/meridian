// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract.CollectionEdit
{
    public interface IPlaylistEditableMetadata
    {
        string Id { get; set; }
        string Name { get; set; }
        bool? IsPublished { get; set; }
    }
}
