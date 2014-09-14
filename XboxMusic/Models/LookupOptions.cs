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
    public class LookupOptions
    {

        #region Properties

        /// <summary>
        /// Specifies whether or not Albums should be returned for each Artist.
        /// </summary>
        public bool GetArtistAlbums { get; set; }

        /// <summary>
        /// Specifies whether or not <see cref="Artist.TopTracks">Top Tracks</see> should be returned for each Artist.
        /// </summary>
        public bool GetArtistTopTracks { get; set; }

        /// <summary>
        /// Specifies whether or not Tracks should be returned for each Album.
        /// </summary>
        public bool GetAlbumTracks { get; set; }

        /// <summary>
        /// Specifies whether or not Artist details should be returned for each Album.
        /// </summary>
        /// <remarks>Level of detail equivalent to a Lookup call on the <see cref="Artist"/>.</remarks>
        public bool GetAlbumArtistDetails { get; set; }

        /// <summary>
        /// Specifies whether or not Album details should be returned for each Track.
        /// </summary>
        /// <remarks>Level of detail equivalent to a Lookup call on the <see cref="Album"/>.</remarks>
        public bool GetTrackAlbumDetails { get; set; }

        /// <summary>
        /// Specifies whether or not Artist details should be returned for each Track.
        /// </summary>
        /// <remarks>Level of detail equivalent to a Lookup call on the <see cref="Artist"/>.</remarks>
        public bool GetTrackArtistDetails { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="LookupOptions"/> instance with all options disabled.
        /// </summary>
        public LookupOptions()
        {
        }

        /// <summary>
        /// Creates a new <see cref="LookupOptions"/> instance that specifies whether or not with all options should be enabled.
        /// </summary>
        /// <param name="getEverything">Specifies whether or not all lookup options should be returned.</param>
        public LookupOptions(bool getEverything)
        {
            GetArtistAlbums = getEverything;
            GetArtistTopTracks = getEverything;
            GetAlbumTracks = getEverything;
            GetAlbumArtistDetails = getEverything;
            GetTrackAlbumDetails = getEverything;
            GetTrackArtistDetails = getEverything;
        }

        #endregion

    }
}
