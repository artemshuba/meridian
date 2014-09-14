using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbox.Music
{

    /// <summary>
    /// 
    /// </summary>
    public enum ImageResizeMode
    {
        /// <summary>
        /// Resize to maximum size which fits dimension without changing the aspect ratio.
        /// </summary>
        Scale,

        /// <summary>
        /// Pad to dimension after resize if aspect ratio didn't match.
        /// </summary>
        Letterbox,

        /// <summary>
        /// Get the required width and height but image is cropped.
        /// </summary>
        Crop

    }
}
