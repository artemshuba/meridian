using System.Text;

namespace RemoteKit.Server.Response
{
    public class JsonResponse : HttpResponse
    {
        public override string Body
        {
            get { return base.Body; }
            set
            {
                var length = Encoding.UTF8.GetByteCount(value).ToString();

                Headers["Content-Length"] = length;

                base.Body = value;
            }
        }

        public JsonResponse()
        {
            Headers.Add("Content-Type", "application/json");
        }
    }
}
