using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RemoteKit.Core;
using RemoteKit.Server.Notifications;
using RemoteKit.Server.Response;

namespace RemoteKit.Server
{
    /// <summary>
    /// Simple web server that will handle remote requests
    /// </summary>
    public class RemoteKitWebServer : IDisposable
    {
        private const int LongPollTimeOut = 10;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private StreamSocketListener _listener;

        private readonly List<StreamSocket> _activeSockets = new List<StreamSocket>();

        private string _apiEndpoint = "/remote";
        private IRemoteKitApiClient _apiClient;

        public RemoteKitWebServer()
        {
            _listener = new StreamSocketListener();
            _listener.Control.KeepAlive = false;
            _listener.ConnectionReceived += Listener_ConnectionReceived;
        }

        public void Dispose()
        {
            _listener.Dispose();
            _listener = null;
        }

        public void RegisterApiEndpoint(string endpoint, IRemoteKitApiClient client)
        {
            _apiEndpoint = endpoint;
            _apiClient = client;
        }

        public async void Start(int port)
        {
            await _listener.BindServiceNameAsync(port.ToString());
        }

        public async Task Notify(RemoteKitNotification notification)
        {
            var json = SerializeJson(notification);

            var response = new JsonResponse() { Body = json };

            while (_activeSockets.Count > 0)
            {
                var s = _activeSockets[_activeSockets.Count - 1];
                _activeSockets.Remove(s);

                await WriteResponse(s, response);
            }
        }

        private void Listener_ConnectionReceived(StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs e)
        {
            try
            {
                ProcessRequestAsync(e.Socket);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "RemoteKit: Unable to process request. " + ex);
            }
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            Debug.WriteLine("RemoteKit: connected " + socket.Information.RemoteAddress);

            RemoteKitHttpRequest _request = null;
            using (IInputStream input = socket.InputStream)
            {
                const int bufferSize = 8192;
                byte[] data = new byte[bufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = bufferSize;
                var requestString = new StringBuilder();
                while (dataRead == bufferSize)
                {
                    await input.ReadAsync(buffer, bufferSize, InputStreamOptions.Partial);
                    requestString.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }

                _request = ParseRequest(requestString.ToString());
            }

            if (_request != null)
            {
                //longpoll endpoint
                if (_request.RequestTarget == "/notifier")
                {
                    var activeSocket = _activeSockets.FirstOrDefault(
                        s => s.Information.RemoteAddress.RawName == socket.Information.RemoteAddress.RawName);
                    if (activeSocket != null)
                    {
                        activeSocket.Dispose();
                        _activeSockets.Remove(activeSocket);
                    }

                    _activeSockets.Add(socket);

                    var token = _cts.Token;

                    await Task.Delay(TimeSpan.FromSeconds(LongPollTimeOut), token).ContinueWith(async t =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        while (_activeSockets.Count > 0)
                        {
                            var s = _activeSockets[_activeSockets.Count - 1];
                            _activeSockets.Remove(s);

                            //write ok response
                            await WriteResponse(s, new JsonResponse() { Body = "{}" });
                        }
                    }, token);
                }
                //api endpoint
                else if (_request.RequestTarget == _apiEndpoint)
                {
                    ProcessApiResponse(socket, _request);
                }
                //file endpoint
                else
                {
                    ProcessFileResponse(socket, _request);
                }
            }
        }

        private async void ProcessApiResponse(StreamSocket socket, RemoteKitHttpRequest request)
        {
            try
            {
                var command = JsonConvert.DeserializeObject<RemoteKitCommand>(request.Body);

                if (_apiClient != null)
                {
                    var response = await _apiClient.ProcessCommandAsync(command);
                    if (!string.IsNullOrEmpty(response))
                    {
                        await WriteResponse(socket, new JsonResponse() { Body = response });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RemoteKit: Unable to process request. " + ex);
            }

            //write ok response
            await WriteResponse(socket, new JsonResponse() { Body = "{}" });
        }

        private async void ProcessFileResponse(StreamSocket socket, RemoteKitHttpRequest request)
        {
            try
            {
                if (_apiClient != null)
                {
                    var stream = await _apiClient.ProcessFileAsync(request.RequestTarget);

                    if (stream != null)
                    {
                        var response = new HttpResponse();

                        var length = stream.Length;
                        var extension = Path.GetExtension(request.RequestTarget);
                        var contentType = "application/" + extension;

                        switch (extension)
                        {
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

                            case ".html":
                            case ".htm":
                            default:
                                contentType = "text/html";
                                break;
                        }

                        response.Headers.Add("Content-Length", length.ToString());
                        response.Headers.Add("Content-Type", contentType);
                        response.Headers.Add("Connection", "close");

                        var buffer = new byte[length];
                        await stream.ReadAsync(buffer, 0, (int)length);
                        stream.Dispose();
                        response.Data = buffer;

                        await WriteResponse(socket, response);

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            //write 404 response
            var failResponse = new HttpResponse();
            failResponse.Status = "HTTP/1.1 404 Not found";
            failResponse.Headers.Add("Content-Length", "0");
            await WriteResponse(socket, failResponse);
        }

        private async Task WriteResponse(StreamSocket socket, HttpResponse response)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            using (IOutputStream output = socket.OutputStream)
            {
                using (var stream = output.AsStreamForWrite())
                {
                    Debug.WriteLine("RemoteKit Sending Response to " + socket.Information.RemoteAddress + ":\r\n" + response);
                    var responseString = response.ToString();

                    var bytes = Encoding.UTF8.GetBytes(responseString);

                    await stream.WriteAsync(bytes, 0, bytes.Length);

                    if (response.Data != null && response.Data.Length > 0)
                    {
                        await stream.WriteAsync(response.Data, 0, response.Data.Length);
                    }

                    await stream.FlushAsync();

                    //using (var writer = new StreamWriter(stream))
                    //{
                        //Debug.WriteLine("RemoteKit Sending Response to " + socket.Information.RemoteAddress +
                        //                ":\r\n" + response);
                        //var responseString = response.ToString();

                        //var bytes = Encoding.UTF8.GetBytes(responseString);

                        //await writer.WriteAsync(responseString);

                        //if (response.Data != null && response.Data.Length > 0)
                        //{
                        //    await stream.WriteAsync(response.Data, bytes.Length - 1, response.Data.Length);
                        //}

                        //await writer.FlushAsync();
                    //}
                }
            }
        }

        private string SerializeJson(object o)
        {
            return JsonConvert.SerializeObject(o,
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        private RemoteKitHttpRequest ParseRequest(string request)
        {
            var requestTarget = Regex.Match(request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|").Groups[1].Value;

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

            //Safari on iOS and IE on Windows Phone most of the time doesn't send data in the post
            //so using x-data header to pass commands instead of request body
            if (headers.ContainsKey("x-data"))
            {
                body = headers["x-data"];
            }

            return new RemoteKitHttpRequest() { RequestTarget = requestTarget, Headers = headers, Body = body };
        }
    }
}