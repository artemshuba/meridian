using System.Collections.Generic;
using System.Text;

namespace RemoteKit.Server.Response
{
    public class HttpResponse
    {
        public string Status { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public virtual string Body { get; set; }

        public virtual byte[] Data { get; set; }

        public string Connection
        {
            get
            {
                if (!Headers.ContainsKey("Connection"))
                    return null;

                return Headers["Connection"];
            }
            set
            {
                Headers["Connection"] = value;
            }
        }

        public HttpResponse()
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

            if (!string.IsNullOrEmpty(Body))
            {
                sb.AppendLine(Body);
            }

            return sb.ToString();
        }
    }
}
