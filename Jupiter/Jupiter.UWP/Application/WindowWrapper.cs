using System;
using Jupiter.Services.Navigation;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;

namespace Jupiter.Application
{
    public class WindowWrapper
    {
        public static readonly List<WindowWrapper> ActiveWrappers = new List<WindowWrapper>();

        public static WindowWrapper Default() => ActiveWrappers.FirstOrDefault();

        public static WindowWrapper Current() => ActiveWrappers.FirstOrDefault(x => x.Window == Window.Current) ?? Default();

        public Window Window { get; }

        public NavigationService NavigationService { get; set; }

        public WindowWrapper(Window window)
        {
            if (ActiveWrappers.Any(x => x.Window == window))
                throw new Exception("Windows already has a wrapper; use Current(window) to fetch.");

            Window = window;
            ActiveWrappers.Add(this);
            window.Closed += (s, e) => { ActiveWrappers.Remove(this); };
        }
    }
}
