using System.Collections.Generic;
using System.Net;

namespace Meridian.RemotePlay
{
    public class SimpleHttpRequest
    {
        public string RequestTarget { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public SimpleHttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}
