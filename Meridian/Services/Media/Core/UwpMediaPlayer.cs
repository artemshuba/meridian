using System;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Meridian.Services.Media.Core
{
    /// <summary>
    /// Wrapper on UWP MediaPlayer (Win10)
    /// </summary>
    public class UwpMediaPlayer : MediaPlayerBase
    {
        private MediaPlayer _mediaPlayer;

        public override TimeSpan Position
        {
            get { return _mediaPlayer.PlaybackSession.Position; }
            set { _mediaPlayer.PlaybackSession.Position = value; }
        }

        public override TimeSpan Duration
        {
            get
            {
                if (_mediaPlayer.PlaybackSession.NaturalDuration != null)
                    return _mediaPlayer.PlaybackSession.NaturalDuration;

                return TimeSpan.Zero;
            }
        }

        public override Uri Source
        {
            get { 
                return _mediaPlayer.Source as Uri;
            }
            set { _mediaPlayer.Source = MediaSource.CreateFromUri(value); }
        }

        public override double Volume
        {
            get { return _mediaPlayer.Volume; }
            set { _mediaPlayer.Volume = value; }
        }

        public override void Initialize()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            _mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            _mediaPlayer.MediaOpened += MediaPlayerOnMediaOpened;
        }

        public override void Play()
        {
            _mediaPlayer.Play();
        }

        public override void Pause()
        {
            _mediaPlayer.Pause();
        }

        public override void Stop()
        {
            _mediaPlayer.Source = null;
        }

        public override void Dispose()
        {
            _mediaPlayer.MediaEnded -= MediaPlayerOnMediaEnded;
            _mediaPlayer.MediaFailed -= MediaPlayerOnMediaFailed;
            _mediaPlayer.MediaOpened -= MediaPlayerOnMediaOpened;

            _mediaPlayer = null;
        }

        private void MediaPlayerOnMediaOpened(MediaPlayer sender, object e)
        {
            if (MediaOpened != null)
                MediaOpened(sender, EventArgs.Empty);
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs e)
        {
            if (MediaFailed != null)
                MediaFailed(sender, new Exception(e.ErrorMessage));
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object e)
        {
            if (MediaEnded != null)
                MediaEnded(sender, EventArgs.Empty);
        }
    }
}
