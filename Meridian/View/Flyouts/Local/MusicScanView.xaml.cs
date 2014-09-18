using System.ComponentModel;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Services;
using Microsoft;

namespace Meridian.View.Flyouts.Local
{
    /// <summary>
    /// Interaction logic for MusicScanView.xaml
    /// </summary>
    public partial class MusicScanView : FlyoutContent, INotifyPropertyChanged
    {
        private double _progress;

        #region Commands

        /// <summary>
        /// Cancel scanning command
        /// </summary>
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        /// <summary>
        /// Scan progress
        /// </summary>
        public double Progress
        {
            get { return _progress; }
            set
            {
                if (_progress == value)
                    return;

                _progress = value;
                OnPropertyChanged();
            }
        }

        public MusicScanView()
        {
            InitializeComponent();
            InitializeCommands();

            Load();
        }

        private void InitializeCommands()
        {
            CancelCommand = new RelayCommand(() =>
            {
                ServiceLocator.LocalMusicService.ScanMusicCancel();
                Close();
            });
        }

        private async void Load()
        {
            await ServiceLocator.LocalMusicService.ScanMusic(new Progress<double>(UpdateProgress));
            Close();
        }

        private void UpdateProgress(double progress)
        {
            Progress = progress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
