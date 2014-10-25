using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.Messages;
using VkLib.Core.Groups;

namespace Meridian.ViewModel.People
{
    public class SocietiesViewModel : ViewModelBase
    {
        private List<VkGroup> _societies;

        #region

        public RelayCommand<VkGroup> GoToSocietyCommand { get; private set; }

        #endregion

        public List<VkGroup> Societies
        {
            get { return _societies; }
            set
            {
                if (_societies == value)
                    return;

                _societies = value;
                RaisePropertyChanged("Societies");
            }
        }

        public void Activate()
        {
            if (Societies == null || Societies.Count == 0)
                LoadSocieties();
        }

        public SocietiesViewModel()
        {
            RegisterTasks("groups");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GoToSocietyCommand = new RelayCommand<VkGroup>(society =>
            {
                MessengerInstance.Send(new NavigateToPageMessage()
                {
                    Page = "/People.SocietyAudioView",
                    Parameters = new Dictionary<string, object>()
                    {
                        {"society", society}
                    }
                });
            });
        }

        private async void LoadSocieties()
        {
            OnTaskStarted("groups");

            try
            {
                var societies = await DataService.GetSocieties(0, 0, 0, "photo,photo_100");
                if (societies.Items != null && societies.Items.Count > 0)
                {
                    Societies = societies.Items;
                }
                else
                {
                    OnTaskError("groups", ErrorResources.LoadSocietiesErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("groups", ErrorResources.LoadSocietiesErrorCommon);

            }

            OnTaskFinished("groups");
        }
    }
}
