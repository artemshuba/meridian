using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Jupiter.Services.Navigation
{
    public class NavigatingEventArgs : NavigatedEventArgs
    {
        public Type TargetPageType { get; set; }

        public NavigatingEventArgs() { }

        public NavigatingEventArgs(NavigatingCancelEventArgs e, Page page)
        {
            this.Page = page;
            this.NavigationMode = e.NavigationMode;
            this.PageType = e.SourcePageType;
            this.Parameter = e.Parameter?.ToString();
        }
        public bool Cancel { get; set; } = false;
        public bool Suspending { get; set; } = false;
    }
}
