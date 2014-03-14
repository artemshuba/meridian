using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VkLib.Core.Messages
{
    public class VkLongPollService
    {
        private readonly Vkontakte _vkontakte;
        private bool _stop;

        public VkLongPollService(Vkontakte vkontakte)
        {
            _vkontakte = vkontakte;
        }

        public async Task Start(Action<List<VkLongPollMessage>> onMessage = null)
        {
            if (_vkontakte.AccessToken == null || string.IsNullOrEmpty(_vkontakte.AccessToken.Token) || _vkontakte.AccessToken.HasExpired)
                throw new Exception("Access token is not valid.");

            var parametres = new Dictionary<string, string>();

            _vkontakte.SignMethod(parametres);

            var response = await new VkRequest(new Uri(VkConst.MethodBase + "messages.getLongPollServer"), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response["response"] != null)
            {
                var key = (string)response["response"]["key"];
                var server = (string)response["response"]["server"];
                var ts = (string)response["response"]["ts"];

                Debug.WriteLine("Long poll service started: " + response);

                await Connect(key, server, ts, onMessage);
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        private async Task Connect(string key, string server, string ts, Action<List<VkLongPollMessage>> onMessage = null)
        {
            var parametres = new Dictionary<string, string>();

            parametres.Add("act", "a_check");
            parametres.Add("key", key);
            parametres.Add("ts", ts);
            parametres.Add("wait", "25");
            parametres.Add("mode", "2");

            var response = await new VkRequest(new Uri("http://" + server), parametres).Execute();

            VkErrorProcessor.ProcessError(response);

            if (response != null)
            {
                Debug.WriteLine("Long poll service response: " + response);

                ts = (string)response["ts"];

                var result = new List<VkLongPollMessage>();

                foreach (JArray update in response["updates"])
                {
                    var m = VkLongPollMessage.FromJson(update);
                    if (m != null)
                        result.Add(m);
                }

                if (onMessage != null)
                    onMessage(result);

                if (!_stop)
                    await Connect(key, server, ts, onMessage);
            }
        }
    }
}
