using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xbox.Music
{

    /// <summary>
    /// The base object from which the main Xbox Music types inherit.
    /// </summary>
    [KnownType(typeof(Album))]
    [KnownType(typeof(Artist))]
    [KnownType(typeof(Track))]
    [DataContract]
    public class EntryBase
    {

        #region Properties

        /// <summary>
        /// Identifier for this piece of content. All IDs are of the form {namespace}.{actual identifier} 
        /// and may be used in lookup requests.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// The name of this piece of content.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// A direct link to the default image associated with this piece of content.
        /// </summary>
        [DataMember]
        public string ImageUrl { get; set; }

        /// <summary>
        /// A music.xbox.com link that redirects to a contextual page for this piece of content on the 
        /// relevant Xbox Music client application depending on the user's device or operating system.
        /// </summary>
        [DataMember]
        public string Link { get; set; }

        /// <summary>
        /// An optional collection of other IDs that identify this piece of content on top of the main ID. 
        /// Each key is the namespace or subnamespace in which the ID belongs, and each value is a secondary ID for this piece of content.
        /// </summary>
        [DataMember]
        public Dictionary<string, string> OtherIds { get; set; }

        /// <summary>
        /// An indication of the data source for this piece of content. Currently only "Catalog" is supported.
        /// </summary>
        [DataMember]
        public string Source { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a resized image based on the formatting criteria allowed by the Xbox Music service.
        /// </summary>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="mode">The mode with wich to resize the image.</param>
        /// <param name="backgroundColor">
        /// HTML-compliant color for letterbox resize mode background. Hex values must start with a #. 
        /// Can also use color names, such as "green". Defaults to an empty string.
        /// </param>
        /// <returns>A string with the URL for the requested image specifications.</returns>
        public string GetImage(int width, int height, ImageResizeMode mode = ImageResizeMode.Crop, string backgroundColor = "")
        {
            var modeString = Enum.GetName(typeof(ImageResizeMode), mode).ToLower();
            switch (mode)
            {
                case ImageResizeMode.Letterbox:
                    return string.Format("{0}&w={1}&h={2}&mode={3}&background={4}", ImageUrl, width, height, modeString, backgroundColor.Replace("#", "%23"));
                case ImageResizeMode.Scale:
                    return string.Format("{0}&w={1}&h={2}&mode={3}", ImageUrl, width, height, modeString);
                default:
                    return string.Format("{0}&w={1}&h={2}", ImageUrl, width, height);
            }

        }

        /// <summary>
        /// Creates a link that opens the platform's Xbox Music app with the specified view.
        /// </summary>
        /// <param name="action">The view to open for this particular entity.</param>
        /// <returns></returns>
        public string GetDeepLink(LinkAction action = LinkAction.View)
        {
            var actionString = Enum.GetName(typeof(LinkAction), action).ToLower();
            var format = Link.Contains("?") ? "{0}&action={1}" : "{0}?action={1}";
            return string.Format(format, Link, actionString);

        }

        /// <summary>
        /// Creates an Affiliate link that opens the platform's Xbox Music app with the specified view.
        /// </summary>
        /// <param name="affiliateId">The AffiliateID assigned to you by LinkSynergy.</param>
        /// <param name="action">The view to open for this particular entity.</param>
        /// <returns></returns>
        public string GetDeepLink(string affiliateId, LinkAction action = LinkAction.View)
        {
            var deepLink = Uri.EscapeUriString(GetDeepLink(action));
            return string.Format("http://click.linksynergy.com/deeplink?id={0}&mid=39033&murl={1}", affiliateId, deepLink);
        }


        #endregion



    }
}
