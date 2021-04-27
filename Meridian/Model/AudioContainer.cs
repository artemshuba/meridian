using Meridian.Interfaces;
using System.Collections.Generic;

namespace Meridian.Model
{
    /// <summary>
    /// Helper class that allows to pass audio and playlist to command
    /// </summary>
    public class AudioContainer
    {
        public IAudio Track { get; set; }

        public List<IAudio> Tracklist { get; set; }
    }
}
