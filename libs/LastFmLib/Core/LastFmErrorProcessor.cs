using System.Diagnostics;
using LastFmLib.Error;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core
{
    internal static class LastFmErrorProcessor
    {
        public static bool ProcessError(JToken response)
        {
            if (response["error"] != null)
            {                     
                Debug.WriteLine("Last FM: " + response["message"].Value<string>());

                switch (response["error"].Value<string>())
                {
                    case "9":
                    case "4": //login error
                        throw new LastFmLoginException();
                    case "6": //artist not found
                        return false;
                }

                return false;
            }

            return true;
        }
    }
}
