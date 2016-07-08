using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Meridian.RemotePlay
{
    public class SimpleHttpResponse
    {
        public string Status { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public SimpleHttpResponse()
        {
            Status = "HTTP/1.1 200 OK";
            Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Status);

            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    sb.AppendLine(header.Key + ": " + header.Value);
                }
            }

            sb.AppendLine("");
            return sb.ToString();
        }
    }
}
