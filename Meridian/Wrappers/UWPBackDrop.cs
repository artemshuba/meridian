using Microsoft.Toolkit.Wpf.UI.XamlHost;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Meridian.Wrappers
{
    public class UWPBackDrop : WindowsXamlHost
    {
        internal Meridian.WrappedControls.BackDrop UwpControl => ChildInternal as Meridian.WrappedControls.BackDrop;

        public UWPBackDrop() : this(typeof(Meridian.WrappedControls.BackDrop).FullName)

        {

        }

        protected UWPBackDrop(string typeName)
        {
            InitialTypeName = typeName;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += UWPBackDrop_Loaded;
        }

        private void UWPBackDrop_Loaded(object sender, RoutedEventArgs e)
        {
            UwpControl.UseHostBackDrop = true;
        }
    }
}
