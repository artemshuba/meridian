namespace Meridian.ViewModel.Messages
{
    public enum PlayerPlayState
    {
        Opening,
        Playing,
        Paused,
        Stopped,
        Buffering
    }

    public class PlayStateChangedMessage
    {
        public PlayerPlayState NewState { get; set; }
    }
}
