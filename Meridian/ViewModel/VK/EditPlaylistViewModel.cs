using System.Collections.Generic;
using Meridian.Model;
using Meridian.ViewModel.Common;
using Microsoft.UI.Xaml.Navigation;
using Jupiter.Mvvm;
using System;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.Utils.Helpers;

namespace Meridian.ViewModel.VK
{
    public class EditPlaylistViewModel : PopupViewModelBase
    {
        private readonly VkTracksService _tracksService;

        private PlaylistVk _playlist;

        private string _title;

        #region Commands

        public DelegateCommand SaveCommand { get; private set; }

        #endregion

        public PlaylistVk Playlist
        {
            get { return _playlist; }
            set
            {
                Set(ref _playlist, value);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        public EditPlaylistViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            if (parameters == null)
                Playlist = new PlaylistVk() { Title = Resources.GetStringByKey("EditPlaylist_NewPlaylistTitle") };
            else
                Playlist = (PlaylistVk)parameters["playlist"];

            Title = Playlist.Title;

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            SaveCommand = new DelegateCommand(Save);
        }
        private async void Save()
        {
            //TODO errors
            try
            {
                if (Playlist.Id == null)
                {
                    //create new playlist
                    var newId = await _tracksService.AddPlaylist(Title);

                    Playlist.Id = newId.ToString();
                    Playlist.Title = Title;

                    Close(Playlist);
                }
                else
                {
                    //edit playlist
                    if (await _tracksService.EditPlaylist(Playlist, Title))
                    {
                        Playlist.Title = Title;

                        Close(Playlist);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to edit playlist");
            }
        }
    }
}