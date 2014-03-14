using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EchonestApi.Core.Artist;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for CreateRadioStationView.xaml
    /// </summary>
    public partial class CreateRadioStationView : UserControl, INotifyPropertyChanged
    {
        private bool _isWorking;
        private bool _canSave;
        private ObservableCollection<EchoArtist> _artists = new ObservableCollection<EchoArtist>();

        public RelayCommand<EchoArtist> RemoveArtistCommand { get; private set; }

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (_isWorking == value)
                    return;

                _isWorking = value;
                OnPropertyChanged("IsWorking");
            }
        }

        public bool CanSave
        {
            get { return _canSave; }
            set
            {
                if (_canSave == value)
                    return;

                _canSave = value;
                OnPropertyChanged("CanSave");
            }
        }

        public ObservableCollection<EchoArtist> Artists
        {
            get { return _artists; }
            set
            {
                if (_artists == value)
                    return;

                _artists = value;
                OnPropertyChanged("Artists");
            }
        }

        public CreateRadioStationView()
        {
            InitializeComponent();

            RemoveArtistCommand = new RelayCommand<EchoArtist>(artist => Artists.Remove(artist));
        }

        private void CreateRadioStationView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Artists.CollectionChanged += Artists_CollectionChanged;

            TitleTextBox.Focus();

            UpdateCanSave();

            if (Artists.Count > 0)
                Title.Text = MainResources.CreateStationEdit;
        }

        void Artists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCanSave();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            Close(Artists);
        }

        private void Close(object result = null)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close(result);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddArtist();
        }

        private void TitleTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddArtist();
            }
        }

        private async void AddArtist()
        {
            if (string.IsNullOrEmpty(TitleTextBox.Text))
                return;

            IsWorking = true;

            try
            {
                var artist = await RadioService.FindArtist(TitleTextBox.Text);
                if (artist != null)
                {
                    Artists.Add(artist);
                    TitleTextBox.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private void UpdateCanSave()
        {
            if (Artists.Count == 0)
                CanSave = false;
            else
                CanSave = true;
        }
    }
}
