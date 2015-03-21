using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Meridian.Helpers
{
    public static class NetworkHelper
    {
        public static List<string> GetLocalIpAddresses()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }


            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Select(a => a.ToString()).ToList();
        }
    }
}
