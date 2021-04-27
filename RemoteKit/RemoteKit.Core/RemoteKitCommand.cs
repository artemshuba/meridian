using System.Collections.Generic;

namespace RemoteKit.Core
{
    public class RemoteKitCommand
    {
        public string Command { get; set; }

        public Dictionary<string, object> Params { get; set; }
    }
}