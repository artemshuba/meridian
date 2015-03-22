using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Meridian.Services;
using Meridian.ViewModel;
using Neptune.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Meridian.RemotePlay
{
    public class SimpleWebServer
    {
        private const string DefaultResponseHeaders = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length: {0}\r\n\r\n{1}";
        private TcpListener _listener;
        private bool _run;

        public Action<string> OnReceivedData;

        public void Start(IPAddress address, int port)
        {
            if (_listener != null)
                _listener.Stop();

            _listener = new TcpListener(address, port);
            _listener.Start();

            _run = true;

            Listen();
        }

        public void Stop()
        {
            _run = false;
            _listener.Stop();
        }

        private async void Listen()
        {
            if (!_run)
                return;

            TcpClient client = null;

            try
            {
                client = await _listener.AcceptTcpClientAsync();
            }
            catch (Exception)
            {
            }

            if (!_run || client == null)
                return;

            var buffer = new byte[client.ReceiveBufferSize];
            var stream = client.GetStream();

            SimpleHttpRequest simpleRequest = null;

            if (stream.CanRead)
            {
                try
                {
                    var received = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (received > 0)
                    {
                        var requestString = Encoding.UTF8.GetString(buffer, 0, received);
                        if (!string.IsNullOrEmpty(requestString))
                        {
                            Debug.WriteLine("Request: " + requestString);

                            simpleRequest = ProcessRequest(requestString);

                            if (!string.IsNullOrEmpty(simpleRequest.Body))
                            {
                                if (OnReceivedData != null)
                                    OnReceivedData(simpleRequest.Body);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            }

            if (simpleRequest != null)
            {
                string target = "/index.html";
                if (simpleRequest.RequestTarget.Length > 1)
                {
                    target = simpleRequest.RequestTarget;
                }

                if (target.StartsWith("/api"))
                {
                    await ApiResponse(simpleRequest, target, stream);
                }
                else
                {
                    await FileResponse(simpleRequest, target, stream);
                }
            }

            Listen();
        }

        private SimpleHttpRequest ProcessRequest(string request)
        {
            var requestTarget = Regex.Match(request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|").Groups[1].Value;

            //if(requestTarget == "/api")
            //    Debugger.Break();
            var lines = request.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var i = 1;

            var headers = new Dictionary<string, string>();
            var body = string.Empty;

            for (i = 1; i < lines.Length; i++) //first line contains mehtod like GET or POST, skip it
            {
                var line = lines[i];
                if (!line.Contains(":"))
                {
                    //headers ended
                    break;
                }

                var headerName = line.Substring(0, line.IndexOf(":"));
                var headerValue = line.Substring(line.IndexOf(":") + 1);
                if (!headers.ContainsKey(headerName))
                    headers.Add(headerName, headerValue);
            }

            if (lines.Length > i)
            {
                var sb = new StringBuilder();
                for (i = i + 1; i < lines.Length; i++)
                    sb.AppendLine(lines[i]);

                body = sb.ToString();
            }

            //Safari on iOS and IE on Windows Phone doesn't most of time doesn't send data in the post
            //so using x-data header to pass commands instead of request body
            if (headers.ContainsKey("x-data"))
            {
                body = headers["x-data"];
            }

            return new SimpleHttpRequest() { RequestTarget = requestTarget, Headers = headers, Body = body };
        }

        private async Task ApiResponse(SimpleHttpRequest request, string target, Stream responseStream)
        {
            try
            {
                var method = target.Substring(target.LastIndexOf("/"));
                var responseText = string.Empty;
                var jsonSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                switch (method)
                {
                    case "/api":
                        //just return ok
                        responseText = JsonConvert.SerializeObject(new { response = 1 });
                        break;

                    case "/currentTrack":
                        //return current track
                        responseText = JsonConvert.SerializeObject(new { response = new { track = ViewModelLocator.Main.CurrentAudio, currentTime = ViewModelLocator.Main.CurrentAudioPositionSeconds, duration = ViewModelLocator.Main.CurrentAudioDuration.TotalSeconds, shuffle = ViewModelLocator.Main.Shuffle, repeat = ViewModelLocator.Main.Repeat } }, Formatting.None, jsonSettings);
                        break;

                    case "/isPlaying":
                        //return isPlaying
                        responseText = JsonConvert.SerializeObject(new { response = ViewModelLocator.Main.IsPlaying }, Formatting.None, jsonSettings);
                        break;

                    case "/volume":
                        //return volume
                        responseText = JsonConvert.SerializeObject(new { response = ViewModelLocator.Main.Volume }, Formatting.None, jsonSettings);
                        break;
                }

                var response = new SimpleHttpResponse();
                var contentType = "application/json";

                var responseBytes = Encoding.UTF8.GetBytes(responseText.ToString());

                response.Headers.Add("Content-Length", responseBytes.Length.ToString());
                response.Headers.Add("Content-Type", contentType);

                responseStream.WriteText(response.ToString() + responseText);
                responseStream.Dispose();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private async Task FileResponse(SimpleHttpRequest request, string target, Stream responseStream)
        {
            var streamResourceInfo = Application.GetContentStream(new Uri("pack://application:,,,/Meridian;component/RemotePlay/web" + target));
            if (streamResourceInfo != null)
            {
                try
                {
                    var response = new SimpleHttpResponse();

                    var length = streamResourceInfo.Stream.Length;
                    var extension = Path.GetExtension(target);
                    var contentType = "application/" + extension;

                    switch (extension)
                    {
                        case ".html":
                        case ".htm":
                            contentType = "text/html";
                            break;

                        case ".js":
                            contentType = "application/javascript";
                            break;

                        case ".css":
                            contentType = "text/css";
                            break;

                        case ".png":
                            contentType = "image/png";
                            break;

                        case ".svg":
                            contentType = "image/svg+xml";
                            break;
                    }

                    response.Headers.Add("Content-Length", length.ToString());
                    response.Headers.Add("Content-Type", contentType);

                    var headerBytes = Encoding.UTF8.GetBytes(response.ToString());
                    responseStream.Write(headerBytes, 0, headerBytes.Length);
                    streamResourceInfo.Stream.CopyTo(responseStream);
                    await responseStream.FlushAsync();
                    responseStream.Close();
                    responseStream.Dispose();
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }
            else
            {
                var response = new SimpleHttpResponse();
                response.Status = "HTTP/1.1 404 NOT FOUND";
                response.Headers.Add("Content-Length", "0");
                responseStream.WriteText(response.ToString());
                responseStream.Dispose();
            }
        }
    }
}
