using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Meridian.Controls
{
    public sealed class NavigationContext
    {
        private readonly Dictionary<string, object> _parameters;

        public Dictionary<string, object> Parameters
        {
            get { return _parameters; }
            set
            {
                if (value == null)
                    return;

                foreach (var kp in value)
                {
                    _parameters.Add(kp.Key, kp.Value);
                }
            }
        }

        public NavigationContext()
        {
            _parameters = new Dictionary<string, object>();
        }
    }

    public class PageBase : Page
    {
        public NavigationContext NavigationContext { get; set; }

        public PageBase()
        {
            NavigationContext = new NavigationContext();

            this.Loaded += PageBase_Loaded;
            this.Unloaded += PageBase_Unloaded;
        }

        public virtual void OnNavigatedTo()
        {

        }

        public virtual void OnNavigatedFrom()
        {

        }

        private void PageBase_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PageBase_Loaded;
            this.Unloaded -= PageBase_Unloaded;

            OnNavigatedFrom();
        }

        private void PageBase_Loaded(object sender, RoutedEventArgs e)
        {
            OnNavigatedTo();
        }
    }
}
