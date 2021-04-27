using GalaSoft.MvvmLight.Messaging;
using Meridian.Model;
using System.Collections.Generic;

namespace Meridian.Utils.Messaging
{
    public class MessageMyMusicChanged : MessageBase
    {
        public List<AudioVk> AddedTracks { get; set; }

        public List<AudioVk> RemovedTracks { get; set; }
    }
}