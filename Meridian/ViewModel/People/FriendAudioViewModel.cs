using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using VkLib.Core.Audio;
using VkLib.Core.Users;
using VkLib.Error;

namespace Meridian.ViewModel.People
{
    public class FriendAudioViewModel : ViewModelBase
    {
        private const int MAX_WALL_AUDIOS = 100;

        private VkProfile _selectedFriend;
        private ObservableCollection<Audio> _tracks;
        private ObservableCollection<VkAudioAlbum> _albums;
        private VkAudioAlbum _selectedAlbum;
        private CancellationTokenSource _cancellationToken;
        private int _totalAlbumsCount;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand LoadMoreAlbumsCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        public RelayCommand<VkAudioAlbum> PlayAlbumCommand { get; private set; }

        public RelayCommand<VkAudioAlbum> AddAlbumToNowPlayingCommand { get; private set; }

        public RelayCommand<VkAudioAlbum> CopyAlbumCommand { get; private set; } 

        #endregion

        public VkProfile SelectedFriend
        {
            get { return _selectedFriend; }
            set { Set(ref _selectedFriend, value); }
        }

        public ObservableCollection<VkAudioAlbum> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        public ObservableCollection<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public VkAudioAlbum SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                if (Set(ref _selectedAlbum, value))
                {
                    CancelAsync();

                    if (value != null)
                    {
                        switch ((int)value.Id)
                        {
                            case -101:
                                LoadWallAudios(_cancellationToken.Token);
                                break;
                            default:
                                LoadTracks(_cancellationToken.Token);
                                break;
                        }
                    }
                }
            }
        }

        public async void Activate()
        {
            await LoadAlbums();
        }

        public void Deactivate()
        {
            CancelAsync();
        }

        public FriendAudioViewModel()
        {
            _cancellationToken = new CancellationTokenSource();

            RegisterTasks("audio", "albums");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            LoadMoreAlbumsCommand = new RelayCommand(() => LoadMoreAlbums());

            RefreshCommand = new RelayCommand(() => LoadTracks(_cancellationToken.Token));

            PlayAlbumCommand = new RelayCommand<VkAudioAlbum>(PlayAlbum);

            AddAlbumToNowPlayingCommand = new RelayCommand<VkAudioAlbum>(AddAlbumToNowPlaying);

            CopyAlbumCommand = new RelayCommand<VkAudioAlbum>(CopyAlbum);
        }

        private async Task LoadAlbums()
        {
            OnTaskStarted("albums");

            try
            {
                var response = await DataService.GetUserAlbums(SelectedFriend.Id, 0, 0);

                var albums = response.Items;

                _totalAlbumsCount = response.TotalCount;

                if (albums == null)
                    albums = new List<VkAudioAlbum>();

                albums.Insert(0, new VkAudioAlbum() { Id = -1, Title = MainResources.MyMusicAllTracks });
                albums.Insert(1, new VkAudioAlbum() { Id = -101, Title = MainResources.MyMusicWall });
                albums.Insert(2, new VkAudioAlbum() { Id = int.MinValue }); //separator

                Albums = new ObservableCollection<VkAudioAlbum>(albums);

                SelectedAlbum = albums.First();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("albums", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("albums");
        }

        private async Task LoadMoreAlbums()
        {
            if (Albums == null || Albums.Count - 3 >= _totalAlbumsCount)
                return;

            IsWorking = true;

            try
            {
                var response = await DataService.GetUserAlbums(SelectedFriend.Id, 0, Albums.Count - 3);

                if (response.Items != null)
                {
                    foreach (var album in response.Items)
                    {
                        Albums.Add(album);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void LoadTracks(CancellationToken token)
        {
            if (SelectedAlbum == null)
                return;

            OnTaskStarted("audio");

            try
            {
                var response = await DataService.GetUserTracks(0, 0, SelectedAlbum.Id != 0 ? SelectedAlbum.Id : 0, SelectedFriend.Id);
                if (response.Items != null && response.Items.Count > 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Tracks load cancelled");
                        return;
                    }

                    Tracks = new ObservableCollection<Audio>(response.Items);

                    if (AudioService.CurrentAudio == null)
                    {
                        AudioService.CurrentAudio = Tracks.First();
                        AudioService.SetCurrentPlaylist(Tracks);
                    }
                }
                else
                {
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
                }
            }
            catch (VkAccessDeniedException ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorAccessDenied);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("audio");
        }

        private async void LoadWallAudios(CancellationToken token)
        {
            IsWorking = true;
            OnTaskStarted("audio");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_WALL_AUDIOS)
                {
                    var a = await DataService.GetWallAudio(count, offset, SelectedFriend.Id, token);
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("audio");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Wall audios cancelled");
                        break;
                    }

                    offset += count;

                    foreach (var audio in a)
                    {
                        Tracks.Add(audio);
                    }

                    requestsCount++;

                    if (requestsCount >= 2) //не больше 2-х запросов в секунду
                    {
                        requestsCount = 0;
                        await Task.Delay(1000, token);
                    }

                    Debug.WriteLine("Loading more audios from wall");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("audio");
        }

        private async void PlayAlbum(VkAudioAlbum album)
        {
            try
            {
                var audio = await DataService.GetUserTracks(albumId: album.Id, ownerId: album.OwnerId);
                if (audio.Items != null && audio.Items.Count > 0)
                {
                    AudioService.Play(audio.Items.First());
                    AudioService.SetCurrentPlaylist(audio.Items);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private async void AddAlbumToNowPlaying(VkAudioAlbum album)
        {
            try
            {
                var audio = await DataService.GetUserTracks(albumId: album.Id, ownerId: album.OwnerId);
                if (audio.Items != null && audio.Items.Count > 0)
                {
                    foreach (var track in audio.Items)
                    {
                        AudioService.Playlist.Add(track);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private async void CopyAlbum(VkAudioAlbum album)
        {
            try
            {
                await DataService.CopyAlbum(album.Title, album.Id, album.OwnerId);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }
    }
}
