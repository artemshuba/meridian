using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.Messages;
using VkLib.Core.Users;

namespace Meridian.ViewModel.People
{
    public class FriendsViewModel : ViewModelBase
    {
        private List<VkProfile> _friends;

        #region Commands

        public RelayCommand<VkProfile> GoToFriendCommand { get; private set; }

        #endregion

        public List<VkProfile> Friends
        {
            get { return _friends; }
            set { Set(ref _friends, value); }
        }

        public void Activate()
        {
            if (Friends == null || Friends.Count == 0)
                LoadFriends();
        }

        public FriendsViewModel()
        {
            RegisterTasks("friends");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GoToFriendCommand = new RelayCommand<VkProfile>(friend =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/People.FriendAudioView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"friend", friend}
                    }
                });
            });
        }

        private async void LoadFriends()
        {
            OnTaskStarted("friends");

            try
            {
                var friends = await DataService.GetFriends(0, 0, 0, "photo,photo_100,photo_400_orig");
                if (friends.Items != null && friends.Items.Count > 0)
                {
                    Friends = friends.Items;
                }
                else
                {
                    OnTaskError("friends", ErrorResources.LoadFriendsErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("friends", ErrorResources.LoadFriendsErrorCommon);
            }

            OnTaskFinished("friends");
        }
    }
}
