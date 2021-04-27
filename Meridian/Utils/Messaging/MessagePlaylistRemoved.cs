using GalaSoft.MvvmLight.Messaging;
using Meridian.Model;

namespace Meridian.Utils.Messaging
{
    public class MessagePlaylistRemoved : MessageBase
    {
        public PlaylistVk Playlist { get; set; }
    }
}