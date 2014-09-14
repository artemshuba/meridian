using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbox.Music
{
    public enum LinkAction
    {
        /// <summary>
        /// Default. Launches the content details view.
        /// </summary>
        View,

        /// <summary>
        /// Launches playback of the media content.
        /// </summary>
        Play,

        /// <summary>
        /// Opens the "add to collection" screen on the Xbox Music service.
        /// </summary>
        AddToCollection,

        /// <summary>
        /// Opens the appropriate purchase flow on the Xbox Music service.
        /// </summary>
        Buy
    }
}
