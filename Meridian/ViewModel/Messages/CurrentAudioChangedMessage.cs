using Meridian.Model;

namespace Meridian.ViewModel.Messages
{
    public class CurrentAudioChangedMessage
    {
        public Audio OldAudio { get; set; }

        public Audio NewAudio { get; set; }
    }
}
