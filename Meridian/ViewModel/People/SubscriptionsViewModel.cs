using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.Messages;
using VkLib.Core.Users;

namespace Meridian.ViewModel.People
{
    public class SubscriptionsViewModel : ViewModelBase
    {
        private List<VkProfile> _subscrptions;

        #region

        public RelayCommand<VkProfile> GoToSubscriptionCommand { get; private set; }

        #endregion

        public List<VkProfile> Subscriptions
        {
            get { return _subscrptions; }
            set
            {
                if (_subscrptions == value)
                    return;

                _subscrptions = value;
                RaisePropertyChanged("Subscriptions");
            }
        }

        public void Activate()
        {
            if (Subscriptions == null || Subscriptions.Count == 0)
                LoadSubscriptions();
        }

        public SubscriptionsViewModel()
        {
            RegisterTasks("subscriptions");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GoToSubscriptionCommand = new RelayCommand<VkProfile>(subscription =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/People.FriendAudioView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"friend", subscription}
                    }
                });
            });
        }

        private async void LoadSubscriptions()
        {
            OnTaskStarted("subscriptions");

            try
            {
                var subscriptions = await DataService.GetSubscriptions(0, 0, "photo,photo_100,photo_400_orig");
                if (subscriptions.Items != null && subscriptions.Items.Count > 0)
                {
                    Subscriptions = subscriptions.Items;
                }
                else
                {
                    OnTaskError("subscriptions", ErrorResources.LoadSubscriptionsErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                OnTaskError("subscriptions", ErrorResources.LoadSubscriptionsErrorCommon);
            }

            OnTaskFinished("subscriptions");
        }
    }
}
