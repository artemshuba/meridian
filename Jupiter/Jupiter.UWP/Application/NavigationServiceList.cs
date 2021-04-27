using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Jupiter.Services.Navigation;

namespace Jupiter.Application
{
    public class NavigationServiceList
    {
        private readonly Dictionary<string, NavigationService> _services = new Dictionary<string, NavigationService>();

        public NavigationService Default => _services["Default"];

        public NavigationService this[string key] => _services[key];

        public void Register(string key, NavigationService service)
        {
            _services.Add(key, service);
        }

        public void Register(string key, Frame frame)
        {
            var service = new NavigationService(frame);
            if (!IsRegistered(key))
                _services.Add(key, service);
            else
                _services[key] = service;
        }


        public void Unregister(string key)
        {
            if (IsRegistered(key))
                _services.Remove(key);
        }

        public bool IsRegistered(string key)
        {
            return _services.ContainsKey(key);
        }
    }
}