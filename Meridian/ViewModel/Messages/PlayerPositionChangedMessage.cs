using System;

namespace Meridian.ViewModel.Messages
{
    public class PlayerPositionChangedMessage
    {
        public TimeSpan NewPosition { get; set; }
    }
}
