using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Meridian.Domain;
using Newtonsoft.Json.Linq;

namespace Meridian.Services
{
    public class UpdateService : INotifyPropertyChanged
    {
        private const string BASE_URL = "http://store.meridianvk.com/update/";
        private const string MANIFEST_PATH = "update-v4.js";
        private const string MANIFEST_PATH_DEV = "update-v4-dev.js";

        private bool _isCheckingUpdates;
        private bool _isUpdating;
        private int _updateProgress;
        private bool _isUpdateInstalled;

        public bool IsCheckingUpdates
        {
            get { return _isCheckingUpdates; }
            private set
            {
                if (_isCheckingUpdates == value)
                    return;

                _isCheckingUpdates = value;
                OnPropertyChanged("IsCheckingUpdates");
            }
        }

        public bool IsUpdating
        {
            get { return _isUpdating; }
            private set
            {
                if (_isUpdating == value)
                    return;

                _isUpdating = value;
                OnPropertyChanged("IsUpdating");
            }
        }

        public int UpdateProgress
        {
            get { return _updateProgress; }
            private set
            {
                if (_updateProgress == value)
                    return;

                _updateProgress = value;
                OnPropertyChanged("UpdateProgress");
            }
        }

        public bool IsUpdateInstalled
        {
            get { return _isUpdateInstalled; }
            private set
            {
                if (_isUpdateInstalled == value)
                    return;

                _isUpdateInstalled = value;
                OnPropertyChanged("IsUpdateInstalled");
            }
        }

        public async void CheckUpdates()
        {
            IsCheckingUpdates = true;

            await Task.Delay(2000);

            var httpClient = new HttpClient();
            JObject json = null;
            try
            {
                var manifestResponse = await httpClient.GetAsync(BASE_URL + (Settings.Instance.InstallDevUpdates ? MANIFEST_PATH_DEV : MANIFEST_PATH));
                var manifestContent = await manifestResponse.Content.ReadAsStringAsync();

                json = JObject.Parse(manifestContent);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsCheckingUpdates = false;

            if (json == null || json["build"] == null)
            {
                return;
            }

            var lastestBuild = json["build"].Value<int>();
            if (lastestBuild > Assembly.GetExecutingAssembly().GetName().Version.Build)
            {
                var path = json["path"].Value<string>();
                UpdateInternal(path);
            }
        }

        //delete .old files
        public void Clean()
        {
            foreach (var file in Directory.GetFiles(App.Root, "*.old", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.IsReadOnly)
                        info.IsReadOnly = false;

                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }
        }

        private void UpdateInternal(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            IsUpdating = true;

            var webClient = new WebClient();
            webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;

            var file = Path.GetTempFileName();
            webClient.DownloadFileAsync(new Uri(BASE_URL + path), file, file);
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var webClient = sender as WebClient;
            if (webClient == null)
                return;

            webClient.DownloadProgressChanged -= webClient_DownloadProgressChanged;

            var file = e.UserState as string;
            if (file == null)
                return;

            try
            {
                Unpack(App.Root, file);
                Settings.Instance.NeedClean = true;

                IsUpdateInstalled = true;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsUpdating = false;
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateProgress = e.ProgressPercentage;
        }

        private void Unpack(string path, string file)
        {
            using (var fileStreamIn = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var zipInStream = new ZipInputStream(fileStreamIn))
                {
                    while (true)
                    {
                        ZipEntry entry = zipInStream.GetNextEntry();
                        if (entry == null)
                            break;
                        if (!entry.IsDirectory)
                        {
                            if (File.Exists(path + "\\" + entry.Name))
                            {
                                string name = path + "\\" + entry.Name;
                                while (File.Exists(name))
                                {
                                    name += ".old";
                                }

                                File.Move(path + "\\" + entry.Name, name);

                                var info = new FileInfo(name);
                                if (info.IsReadOnly)
                                    info.IsReadOnly = false;
                            }

                            using (var fileStreamOut = new FileStream(string.Format(@"{0}\{1}", path, entry.Name), FileMode.Create, FileAccess.Write))
                            {
                                int size;
                                var buffer = new byte[1024];
                                do
                                {
                                    size = zipInStream.Read(buffer, 0, buffer.Length);
                                    fileStreamOut.Write(buffer, 0, size);
                                } while (size > 0);
                                fileStreamOut.Close();
                            }
                        }
                        else
                            if (!Directory.Exists(string.Format(@"{0}\{1}", path, entry.Name)))
                                Directory.CreateDirectory(string.Format(@"{0}\{1}", path, entry.Name));
                    }

                    zipInStream.Close();
                }
                fileStreamIn.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
