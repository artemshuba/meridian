using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Helpers;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.ViewModel;
using Neptune.UI.Extensions;
using VkLib.Core.Groups;

namespace Meridian.Flyouts
{
    /// <summary>
    /// Interaction logic for AddSocietyCommand.xaml
    /// </summary>
    public partial class AddSocietyFlyout : UserControl, INotifyPropertyChanged
    {
        private List<VkGroup> _societies;
        private readonly Dictionary<string, LongRunningOperation> _tasks = new Dictionary<string, LongRunningOperation>();

        #region Commands

        public RelayCommand<VkGroup> SelectSocietyCommand { get; set; }

        #endregion


        public Dictionary<string, LongRunningOperation> Tasks
        {
            get { return _tasks; }
        }

        public List<VkGroup> Societies
        {
            get { return _societies; }
            set
            {
                if (_societies == value)
                    return;

                _societies = value;
                OnPropertyChanged("Societies");
            }
        }

        public AddSocietyFlyout()
        {
            _tasks.Add("societies", new LongRunningOperation());

            InitializeComponent();

            InitializeCommands();
            LoadSocieties();
        }

        private void InitializeCommands()
        {
            SelectSocietyCommand = new RelayCommand<VkGroup>(society =>
            {
                Close(society);
            });
        }

        private async void LoadSocieties()
        {
            OnTaskStarted("societies");

            try
            {
                var response = await DataService.GetSocieties();
                if (response != null && response.TotalCount > 0)
                    Societies = response.Items.Where(s => Settings.Instance.FeedSocieties.All(x => x.Id != s.Id)).ToList();
                else
                {
                    OnTaskError("societies", ErrorResources.LoadSocietiesErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("societies", ErrorResources.LoadSocietiesErrorCommon);
            }

            OnTaskFinished("societies");
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close(object result = null)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close(result);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTaskStarted(string id)
        {
            _tasks[id].Error = null;
            _tasks[id].IsWorking = true;
        }

        private void OnTaskFinished(string id)
        {
            _tasks[id].IsWorking = false;
        }

        private void OnTaskError(string id, string error)
        {
            _tasks[id].Error = error;
            _tasks[id].IsWorking = false;
        }
    }
}
