// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using DataContract;

    /// <summary>
    /// Artist, album and track content item extensions.
    /// </summary>
    public static class ContentExtensions
    {
        /// <summary>
        /// Get the content item's image URL. Optionally allows specifying resize parameters.
        /// </summary>
        /// <param name="content">An artist, album or track content item.</param>
        /// <param name="width">Image width, if set height must be set too.</param>
        /// <param name="height">Image height, if set width must be set too.</param>
        /// <returns>An image URL.</returns>
        public static string GetImageUrl(this Content content, int width = 0, int height = 0)
        {
            string imageUrl = content.ImageUrl;
            if (0 < width && 0 < height)
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return imageUrl;
                }

                string paramsSeparator = imageUrl.Contains("?") ? "&" : "?";
                return $"{imageUrl}{paramsSeparator}w={width}&h={height}";
            }
            if (0 < width || 0 < height)
            {
                throw new ArgumentException("width and height must both be set");
            }
            if (width < 0 || height < 0)
            {
                throw new ArgumentException("width and height must be positive");
            }
            return imageUrl;
        }

        /// <summary>
        /// Content deep link actions.
        /// </summary>
        public enum LinkAction
        {
            /// <summary>
            /// Default. Currently launches the content details view.
            /// </summary>
            Default = 0,
            /// <summary>
            /// Launches the content details view.
            /// </summary>
            View,
            /// <summary>
            /// Launches playback of the media content.
            /// </summary>
            Play
        }

        /// <summary>
        /// Get the content's deep linking URL. Optionaly allows specifying an action.
        /// </summary>
        /// <param name="content">An artist, album or track content item.</param>
        /// <param name="action">An action to take when the link opens the Groove client.</param>
        /// <returns>The deep link.</returns>
        public static string GetLink(this Content content, LinkAction action = LinkAction.Default)
        {
            string link = content.Link;
            if (action == LinkAction.Default || string.IsNullOrEmpty(link))
            {
                return link;
            }

            string paramsSeparator = link.Contains("?") ? "&" : "?";
            return $"{link}{paramsSeparator}action={action}";
        }
    }
}
