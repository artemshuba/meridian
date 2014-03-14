using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.View.Flyouts;
using Neptune.UI.Extensions;
using VkLib.Core.Attachments;
using VkLib.Core.Groups;
using VkLib.Core.Users;
using Application = System.Windows.Application;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace Meridian.ViewModel.Flyouts
{
    public class ShareViewModel : ViewModelBase, IDropTarget
    {
        private ObservableCollection<Audio> _tracks;
        private bool _canGoNext;
        private int _progress;
        private int _progressMaximum;
        private ImageSource _image;
        private CancellationTokenSource _cancellationToken;

        private bool _shareToUser;
        private bool _shareToSociety;
        private bool _shareAsSociety;
        private bool _shareSigned;

        private List<VkGroup> _societies;
        private VkGroup _selectedSociety;

        private List<VkProfile> _friends;
        private VkProfile _selectedFriend;

        #region Commands

        public RelayCommand CancelCommand { get; private set; }

        public RelayCommand CloseCommand { get; private set; }

        public RelayCommand GoNextCommand { get; private set; }

        public RelayCommand<Audio> RemoveTrackCommand { get; private set; }

        public RelayCommand PublishCommand { get; private set; }

        public RelayCommand ClearImageCommand { get; private set; }

        public RelayCommand AddImageCommand { get; private set; }

        #endregion

        public ObservableCollection<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public bool CanGoNext
        {
            get { return _canGoNext; }
            set { Set(ref _canGoNext, value); }
        }

        public string ImagePath { get; set; }

        public ImageSource Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        public int Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }


        public int ProgressMaximum
        {
            get { return _progressMaximum; }
            set { Set(ref _progressMaximum, value); }
        }

        public bool ShareToUser
        {
            get { return _shareToUser; }
            set
            {
                if (_shareToUser == value)
                    return;

                _shareToUser = value;
                RaisePropertyChanged("ShareToUser");
            }
        }

        public bool ShareToSociety
        {
            get { return _shareToSociety; }
            set { Set(ref _shareToSociety, value); }
        }

        public List<VkGroup> Societies
        {
            get { return _societies; }
            set { Set(ref _societies, value); }
        }

        public bool ShareSigned
        {
            get { return _shareSigned; }
            set { Set(ref _shareSigned, value); }
        }

        public bool ShareAsSociety
        {
            get { return _shareAsSociety; }
            set
            {
                if (Set(ref _shareAsSociety, value))
                    ShareSigned = false;
            }
        }

        public VkGroup SelectedSociety
        {
            get { return _selectedSociety; }
            set
            {
                if (Set(ref _selectedSociety, value))
                {
                    if (value != null && value.IsAdmin)
                        ShareAsSociety = true;
                    else
                        ShareAsSociety = false;
                }
            }
        }

        public VkProfile SelectedFriend
        {
            get { return _selectedFriend; }
            set { Set(ref _selectedFriend, value); }
        }

        public List<VkProfile> Friends
        {
            get { return _friends; }
            set { Set(ref _friends, value); }
        }

        public ShareViewModel()
        {
            _tracks = new ObservableCollection<Audio>();
            _tracks.CollectionChanged += Tracks_CollectionChanged;

            _cancellationToken = new CancellationTokenSource();

            InitializeCommands();
        }

        public void InitializeCommands()
        {
            CancelCommand = new RelayCommand(() =>
            {
                Tracks.Clear();
                ViewModelLocator.Main.ShowShareBar = false;
            });

            CloseCommand = new RelayCommand(() =>
            {
                Close();
            });

            GoNextCommand = new RelayCommand(() =>
            {
                ViewModelLocator.Main.ShowShareBar = false;

                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new ShareView() { DataContext = this };
                flyout.Show();

                Activate();
            });

            RemoveTrackCommand = new RelayCommand<Audio>(track => Tracks.Remove(track));

            PublishCommand = new RelayCommand(() =>
            {
                if (ShareToSociety && SelectedSociety == null)
                    return;

                if (ShareToUser && SelectedFriend == null)
                    return;

                var progress = new Progress<int>(p =>
                {
                    Progress += p;
                });

                Share(progress, _cancellationToken.Token);
            });

            ClearImageCommand = new RelayCommand(() =>
            {
                Image = null;
                ImagePath = null;
            });

            AddImageCommand = new RelayCommand(() =>
            {
                var fileOpenDialog = new OpenFileDialog();
                fileOpenDialog.Filter = "Images|*.png;*.jpg";
                if (fileOpenDialog.ShowDialog() == DialogResult.OK)
                {
                    ImagePath = fileOpenDialog.FileName;
                    Image = new BitmapImage(new Uri(ImagePath));
                }
            });
        }

        public void Activate()
        {
            LoadFriends();
            LoadSocieties();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio && Tracks.Count < 15 && Tracks.All(t => t.Id != ((Audio)dropInfo.Data).Id))
                dropInfo.Effects = DragDropEffects.All;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio && Tracks.Count < 15 && Tracks.All(t => t.Id != ((Audio)dropInfo.Data).Id))
            {
                Tracks.Add((Audio)dropInfo.Data);
            }
        }

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCanGoNext();
        }

        private void UpdateCanGoNext()
        {
            if (Tracks.Count == 0)
                CanGoNext = false;
            else
                CanGoNext = true;
        }

        private async void LoadFriends()
        {
            try
            {
                var response = await ViewModelLocator.Vkontakte.Friends.Get(0, "photo", null, 0, 0);
                if (response.Items != null)
                    Friends = response.Items;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private void Close()
        {
            CancelAsync();
            Tracks.Clear();

            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
            {
                flyout.Close();
            }
        }

        private async void LoadSocieties()
        {
            try
            {
                var response = await ViewModelLocator.Vkontakte.Groups.Get(0, "is_admin", null, 0, 0);
                if (response.Items != null)
                    Societies = response.Items;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private async void Share(IProgress<int> progress, CancellationToken token)
        {
            Progress = 0;
            if (Tracks != null)
                ProgressMaximum = Tracks.Count;

            IsWorking = true;
            CanGoNext = false;

            VkPhotoAttachment photoAttachment = null;
            if (!string.IsNullOrEmpty(ImagePath))
            {
                try
                {
                    var server = await ViewModelLocator.Vkontakte.Photos.GetWallUploadServer(0, ShareToSociety ? _selectedSociety.Id : 0);
                    var o = await ViewModelLocator.Vkontakte.Photos.UploadWallPhoto(server, Path.GetFileName(ImagePath), File.OpenRead(ImagePath));
                    if (o != null)
                    {
                        var photo = await ViewModelLocator.Vkontakte.Photos.SaveWallPhoto(o.Server, o.Photo, o.Hash, ShareToUser ? _selectedFriend.Id : 0, ShareToSociety ? _selectedSociety.Id : 0);
                        if (photo != null)
                            photoAttachment = new VkPhotoAttachment(photo);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }

            if (token.IsCancellationRequested)
                return;

            progress.Report(1);
            try
            {
                var attachments = new List<VkAttachment>();
                if (photoAttachment != null)
                    attachments.Add(photoAttachment);

                if (Tracks != null)
                {
                    var audioAttachments = await GetAudioList(progress, token);
                    if (audioAttachments != null)
                        attachments.AddRange(audioAttachments);
                }

                if (token.IsCancellationRequested)
                    return;

                long targetId = 0;
                if (ShareToSociety)
                    targetId = -_selectedSociety.Id;
                else if (ShareToUser)
                    targetId = _selectedFriend.Id;

                var postId = await ViewModelLocator.Vkontakte.Wall.Post(targetId, null, attachments, _shareAsSociety, _shareSigned);
                if (postId > 0)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
            CanGoNext = true;
        }

        private async Task<List<VkAudioAttachment>> GetAudioList(IProgress<int> progress, CancellationToken token)
        {
            if (Tracks == null)
                return null;

            var result = new List<VkAudioAttachment>();
            int requestsCount = 0;
            foreach (var audio in Tracks)
            {
                if (token.IsCancellationRequested)
                    return null;

                if (audio.Url == null)
                {
                    Audio vkAudio = null;
                    try
                    {
                        vkAudio = await DataService.GetAudioByArtistAndTitle(audio.Artist, audio.Title);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log(ex);
                    }

                    if (vkAudio != null)
                    {
                        var audioAttachment = new VkAudioAttachment();
                        audioAttachment.Id = vkAudio.Id;
                        audioAttachment.OwnerId = vkAudio.OwnerId;
                        result.Add(audioAttachment);
                    }
                    else
                    {
                        LoggingService.Log("Failed to find audio " + audio.Artist + " - " + audio.Title);
                    }

                    requestsCount++;

                    if (requestsCount >= 2) //не больше 2-х запросов в секунду
                    {
                        requestsCount = 0;
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    var audioAttachment = new VkAudioAttachment();
                    audioAttachment.Id = audio.Id;
                    audioAttachment.OwnerId = audio.OwnerId;
                    result.Add(audioAttachment);
                }

                if (token.IsCancellationRequested)
                    return null;

                progress.Report(1);
            }

            return result;
        }

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            IsWorking = false;
            CanGoNext = true;

            _cancellationToken = new CancellationTokenSource();
        }
    }
}
