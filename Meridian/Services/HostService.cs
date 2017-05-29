using Meridian.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Meridian.Services
{
    public class HostService
    {
        public static async Task Update()
        {
            try
            {
                LoggingService.Log("Getting host info from server.");

                var httpClient = new HttpClient();
                var hostString = await httpClient.GetStringAsync("http://meridianvk.com/host.js");

                LoggingService.Log("Got host info: " + hostString);

                var host = JObject.Parse(hostString);
                var clientId = host["clientId"].Value<string>();
                var clientSecret = host["clientSecret"].Value<string>();
                var userAgent = host["userAgent"].Value<string>();

                var p = host["params"];

                ViewModelLocator.Vkontakte.AppId = clientId;
                ViewModelLocator.Vkontakte.ClientSecret = clientSecret;
                ViewModelLocator.Vkontakte.UserAgent = userAgent;
                ViewModelLocator.Vkontakte.LoginParams = JsonConvert.DeserializeObject<Dictionary<string,string>>(p.ToString());
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }
    }
}
