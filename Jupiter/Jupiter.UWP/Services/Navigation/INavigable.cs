using Jupiter.Application;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace Jupiter.Services.Navigation
{
    public interface INavigable
    {
        void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode);

        void OnNavigatingFrom(NavigatingEventArgs e);

        NavigationService NavigationService { get; set; }

        IStateItems SessionState { get; set; }
    }
}
