//using Meridian.RemotePlay;
//using RemoteKit.Server;
using System;

namespace Meridian.Services
{
    public class RemotePlayService
    {
        private static RemotePlayService _instance = new RemotePlayService();

        //private RemoteKitWebServer _remoteKitServer;

        public static RemotePlayService Instance
        {
            get { return _instance; }
        }

        public bool IsRunning => false; /*_remoteKitServer != null;*/

        public void Start()
        {
            try
            {
                //save hostname and port to vk server
                //await ServiceLocator.Vk.Storage.Set("remoteServer", $"{Settings.RemotePlayAddress}:{Settings.RemotePlayPort}");

                //if (_remoteKitServer != null)
                //    _remoteKitServer.Dispose();

                //_remoteKitServer = new RemoteKitWebServer();
                //_remoteKitServer.RegisterApiEndpoint("/remote", new RemotePlayApiClient());
                //_remoteKitServer.Start(/*AppState.RemotePlayPort*/9999);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to start RemotePlay service");
            }
        }
    }
}