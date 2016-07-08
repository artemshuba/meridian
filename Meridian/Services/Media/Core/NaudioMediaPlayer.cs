using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;

namespace Meridian.Services.Media.Core
{
    /// <summary>
    /// NAudio Media Player implementation
    /// </summary>
    public class NaudioMediaPlayer : MediaPlayerBase
    {
        private IWavePlayer _wavePlayer;
        private WaveStream _outputStream;
        private WaveChannel32 _volumeStream;
        private bool _initialized;
        private Uri _source;
        private TimeSpan _duration;
        private double _volume;
        private AutoResetEvent _initSourceEvent = new AutoResetEvent(false);

        public override TimeSpan Position
        {
            get
            {
                if (_outputStream == null)
                    return TimeSpan.Zero;

                var currentPos = _outputStream.CurrentTime;
                //if (Math.Round(currentPos.TotalSeconds, 1) >= Math.Round(_duration.TotalSeconds, 1))
                //    SwitchNext();

                return TimeSpan.FromSeconds(Math.Min(Duration.TotalSeconds, currentPos.TotalSeconds));
            }
            set
            {
                if (_outputStream != null)
                    _outputStream.CurrentTime = value;
            }
        }

        public override TimeSpan Duration
        {
            get { return _duration; }
        }

        public override Uri Source
        {
            get { return _source; }
            set
            {
                if (_source == value)
                    Stop();

                _source = value;

                InitSource();
            }
        }

        public override double Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume == value)
                    return;

                _volume = value;

                if (_volumeStream != null)
                    _volumeStream.Volume = (float)value;
            }
        }

        public override void Initialize()
        {
            _wavePlayer = new WaveOutEvent();
        }

        public override void Dispose()
        {
            try
            {
                _wavePlayer.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            _initSourceEvent.Dispose();

            if (_volumeStream != null)
                _volumeStream.Dispose();

            if (_outputStream != null)
                _outputStream.Dispose();

        }

        private void SwitchNext()
        {
            if (MediaEnded != null)
                MediaEnded(this, EventArgs.Empty);
        }

        public async override void Play()
        {
            if (!_initialized)
            {
                await Task.Run(() => _initSourceEvent.WaitOne());
            }
            if (!_initialized)
                return;

            _wavePlayer.Play();
        }

        public override void Pause()
        {
            _wavePlayer.Pause();
        }

        public override void Stop()
        {
            _wavePlayer.Stop();
        }

        private async Task InitSource()
        {
            if (_outputStream != null)
            {
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_volumeStream != null)
            {
                _volumeStream.Dispose();
                _volumeStream = null;
            }

            _initialized = false;

            if (_source == null)
                return;

            await Task.Run(() =>
            {
                try
                {
                    _outputStream = new MediaFoundationReader(_source.OriginalString);
                    _volumeStream = new WaveChannel32(_outputStream, (float)Volume, 0);
                    _volumeStream.PadWithZeroes = false;
                    _wavePlayer.Init(_volumeStream);
                    _wavePlayer.PlaybackStopped += _wavePlayer_PlaybackStopped;
                    _duration = _outputStream.TotalTime;
                    _initialized = true;
                    _initSourceEvent.Set();

                    if (MediaOpened != null)
                        MediaOpened(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    if (MediaFailed != null)
                        MediaFailed(this, ex);
                }
            });
        }

        void _wavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_outputStream != null && _outputStream.CurrentTime.TotalSeconds > Duration.TotalSeconds / 2)
                SwitchNext();
        }
    }
}
