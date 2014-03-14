using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EchonestApi.Core.Artist;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.View.Flyouts;
using Neptune.Messages;

namespace Meridian.ViewModel.Main
{
    public class RadioViewModel : ViewModelBase
    {
        private ObservableCollection<RadioStation> _stations;

        #region Commands

        public RelayCommand CreateStationCommand { get; private set; }

        public RelayCommand<RadioStation> PlayStationCommand { get; private set; }

        public RelayCommand<RadioStation> EditStationCommand { get; private set; }

        public RelayCommand<RadioStation> RemoveStationCommand { get; private set; }

        #endregion

        public ObservableCollection<RadioStation> Stations
        {
            get { return _stations; }
            set { Set(ref _stations, value); }
        }

        public RadioViewModel()
        {
            _stations = new ObservableCollection<RadioStation>();

            InitializeCommands();

            Activate();
        }

        public async void Activate()
        {
            IsWorking = true;

            var stations = await RadioService.LoadStations();
            if (stations != null)
                Stations = new ObservableCollection<RadioStation>(stations);

            IsWorking = false;
        }

        private void InitializeCommands()
        {
            CreateStationCommand = new RelayCommand(async () =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new CreateRadioStationView();
                var artists = await flyout.ShowAsync() as IList<EchoArtist>;

                if (artists != null)
                {
                    AddStation(artists);
                }
            });

            PlayStationCommand = new RelayCommand<RadioStation>(station =>
            {
                RadioService.PlayRadio(station);
                MessengerInstance.Send(new NavigateToPageMessage() { Page = "/Main.NowPlayingView" });
            });

            RemoveStationCommand = new RelayCommand<RadioStation>(station =>
            {
                Stations.Remove(station);
                RadioService.SaveStations(Stations);
            });

            EditStationCommand = new RelayCommand<RadioStation>(async station =>
            {
                var createRadioStationView = new CreateRadioStationView();
                createRadioStationView.Artists = new ObservableCollection<EchoArtist>(station.Artists);

                var flyout = new FlyoutControl();
                flyout.FlyoutContent = createRadioStationView;
                var artists = await flyout.ShowAsync() as IList<EchoArtist>;

                if (artists != null)
                {
                    var titleArtist = station.Artists.First();
                    station.Artists = artists.ToList();


                    //update image and title
                    station.Title = string.Join(", ", station.Artists.Select(s => s.Name));

                    if (station.Artists.First().Name != titleArtist.Name)
                    {
                        station.ImageUrl = null;

                        try
                        {
                            var artistImage = await DataService.GetArtistImage(station.Artists.First().Name, false);
                            if (artistImage != null)
                                station.ImageUrl = artistImage.OriginalString;
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Log(ex);
                        }
                    }

                    RadioService.SaveStations(Stations);
                }
            });
        }

        private async void AddStation(IEnumerable<EchoArtist> artists)
        {
            var titleArtist = artists.First();

            var newStation = new RadioStation()
            {
                Artists = artists.ToList(),
                Title = string.Join(", ", artists.Select(s => s.Name)),
            };

            Stations.Add(newStation);

            try
            {
                var artistImage = await DataService.GetArtistImage(titleArtist.Name, false);
                if (artistImage != null)
                    newStation.ImageUrl = artistImage.OriginalString;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            RadioService.SaveStations(Stations);
        }
    }
}
