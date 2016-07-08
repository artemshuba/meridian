using System;
using System.Net;
using Meridian.Domain;
using Meridian.Services;
using Meridian.ViewModel;
using Newtonsoft.Json.Linq;

namespace Meridian.RemotePlay
{
    public class RemotePlayService
    {
        private static RemotePlayService _instance = new RemotePlayService();
        private SimpleWebServer _server;

        public static RemotePlayService Instance
        {
            get { return _instance; }
        }

        public async void Start()
        {
            try
            {
                //save hostname and port to vk server
                await ViewModelLocator.Vkontakte.Storage.Set("remoteServer", Settings.Instance.RemotePlayAddress + ":" + Settings.Instance.RemotePlayPort);

                if (_server != null)
                    _server.Stop();

                _server = new SimpleWebServer();
                _server.OnReceivedData = OnReceveData;
                _server.Start(IPAddress.Any, Settings.Instance.RemotePlayPort);
            }
            catch (Exception ex)
            {
                LoggingService.Log("Unable to start remote service: " + ex);
            }
        }

        public async void Stop()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }

            try
            {
                //clear hostname and port to vk server
                await ViewModelLocator.Vkontakte.Storage.Set("remoteServer", "");
            }
            catch (Exception ex)
            {
                LoggingService.Log("Unable to stop remote service: " + ex);
            }
        }

        private void OnReceveData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            try
            {
                var json = JObject.Parse(data);
                if (json["command"] != null)
                {
                    switch (json["command"].Value<string>())
                    {
                        case "play":
                            if (!ViewModelLocator.Main.IsPlaying)
                                ViewModelLocator.Main.PlayPauseCommand.Execute(null);
                            break;

                        case "pause":
                            if (ViewModelLocator.Main.IsPlaying)
                                ViewModelLocator.Main.PlayPauseCommand.Execute(null);
                            break;

                        case "next":
                            ViewModelLocator.Main.NextAudioCommand.Execute(null);
                            break;

                        case "prev":
                            ViewModelLocator.Main.PrevAudioCommand.Execute(null);
                            break;

                        case "seek":
                            if (json["commandParam"] != null)
                            {
                                ViewModelLocator.Main.CurrentAudioPositionSeconds = json["commandParam"].Value<double>();
                            }
                            break;

                        case "volume":
                            if (json["commandParam"] != null)
                            {
                                ViewModelLocator.Main.Volume = json["commandParam"].Value<float>();
                            }
                            break;

                        case "shuffle":
                            if (json["commandParam"] != null)
                            {
                                ViewModelLocator.Main.Shuffle = json["commandParam"].Value<bool>();
                            }
                            break;

                        case "repeat":
                            if (json["commandParam"] != null)
                            {
                                ViewModelLocator.Main.Repeat = json["commandParam"].Value<bool>();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

        }
    }
}
