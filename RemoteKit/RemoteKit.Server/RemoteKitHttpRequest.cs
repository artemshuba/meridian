using System.Collections.Generic;

namespace RemoteKit.Server
{
    internal class RemoteKitHttpRequest
    {
        public string RequestTarget { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public RemoteKitHttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}
