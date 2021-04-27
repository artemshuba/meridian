using Microsoft.UI.Xaml.Media;

namespace Meridian.Model
{
    /// <summary>
    /// Image stored in cache
    /// </summary>
    public class CachedImage
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public ImageSource Source { get; set; }
    }
}
