using System;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using Meridian.Domain;
using Meridian.Model;
using Meridian.Services;
using Meridian.ViewModel.Messages;
using Neptune.Desktop.Storage;

namespace Meridian.ViewModel.Main
{
    public class NowPlayingViewModel : ViewModelBase, IDropTarget
    {
        private ImageSource _artistImage;
        private string _lastArtist;
        private CancellationTokenSource _cancellationToken;
        private bool _artRequested;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand ClearCommand { get; private set; }

        #endregion

        public ImageSource ArtistImage
        {
            get { return _artistImage; }
            set { Set(ref _artistImage, value); }
        }

        public NowPlayingViewModel()
        {
            _cancellationToken = new CancellationTokenSource();

            InitializeCommands();
        }

        public override void Activate()
        {
            ViewModelLocator.Main.ShowWindowButtons = false;

            GetArtistImage(_cancellationToken.Token);

            InitializeMessageInterception();
        }

        public override void Deactivate()
        {
            DeinitializeMessageInterception();

            ViewModelLocator.Main.ShowWindowButtons = true;

            CancelAsync();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
            });

            ClearCommand = new RelayCommand(() =>
            {
                AudioService.Playlist.Clear();
            });
        }

        #region Messages

        private void InitializeMessageInterception()
        {
            MessengerInstance.Register<PlayerPositionChangedMessage>(this, OnPlayerPositionChanged);
            MessengerInstance.Register<CurrentAudioChangedMessage>(this, OnCurrentAudioChanged);
        }

        private void DeinitializeMessageInterception()
        {
            MessengerInstance.Unregister<PlayerPositionChangedMessage>(this, OnPlayerPositionChanged);
            MessengerInstance.Unregister<CurrentAudioChangedMessage>(this, OnCurrentAudioChanged);
        }

        private void OnPlayerPositionChanged(PlayerPositionChangedMessage message)
        {
            if (message.NewPosition.TotalSeconds >= 3)
            {
                if (!_artRequested)
                {
                    CancelAsync();
                    _artRequested = true;
                    GetArtistImage(_cancellationToken.Token);
                }
            }
        }

        private void OnCurrentAudioChanged(CurrentAudioChangedMessage message)
        {
            _artRequested = false;
        }

        #endregion

        private async void GetArtistImage(CancellationToken token)
        {
            var audio = AudioService.CurrentAudio;

            if (audio == null)
                return;

            if (audio.Artist == _lastArtist)
                return;

            _lastArtist = audio.Artist;
            string imageType = "big";

            try
            {
                var cachedImage = await CacheService.GetCachedImage("artists/" + CacheService.GetSafeFileName(audio.Artist + "_" + imageType + ".jpg"));
                if (cachedImage != null)
                {
                    var lastUpdateTime = FileStorage.GetFileUpdateTime("artists/" + CacheService.GetSafeFileName(audio.Artist + "_" + imageType + ".jpg"));
                    if ((DateTime.Now - lastUpdateTime).TotalDays < 14)
                    {
                        //if image was downloaded less than 2 weeks ago, show it, else download newer
                        ArtistImage = cachedImage;
                        return;
                    }
                }

                if (Settings.Instance.DownloadArtistArt)
                {
                    var imageUri = await DataService.GetArtistImage(audio.Artist, true);
                    if (imageUri != null)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        cachedImage = await CacheService.CacheImage(imageUri.OriginalString, "artists/" + CacheService.GetSafeFileName(audio.Artist + "_" + imageType + ".jpg"));

                        if (token.IsCancellationRequested)
                            return;

                        ArtistImage = cachedImage;
                        return;
                    }
                }

                ArtistImage = null;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private void CancelAsync()
        {
            _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }

        #region Drag&Drop

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio)
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio)
            {
                var source = (Audio)dropInfo.Data;
                var target = (Audio)dropInfo.TargetItem;
                if (source == target)
                    return;

                int index = ViewModelLocator.Main.CurrentPlaylist.IndexOf(target);
                if (index >= 0)
                {
                    ViewModelLocator.Main.CurrentPlaylist.Remove(source);
                    ViewModelLocator.Main.CurrentPlaylist.Insert(index, source);
                }
            }
        }

        #endregion
    }
}
