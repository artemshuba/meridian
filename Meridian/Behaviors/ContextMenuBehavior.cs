using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Utils.Helpers;
using Microsoft.Xaml.Interactivity;
using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace Meridian.Behaviors
{
    public class ContextMenuBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public ContextMenuContext Context { get; set; } = ContextMenuContext.Common;

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            var control = associatedObject as FrameworkElement;
            if (control != null)
            {
                control.Holding += ControlOnHolding;
                control.RightTapped += ControlOnRightTapped;
            }
        }


        public void Detach()
        {
            var control = AssociatedObject as FrameworkElement;
            if (control != null)
            {
                control.Holding -= ControlOnHolding;
                control.RightTapped -= ControlOnRightTapped;
            }

            AssociatedObject = null;
        }


        private void ControlOnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch)
            {

                ShowFlyout(e.GetPosition(null));
            }
        }

        private void ControlOnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                Debug.WriteLine("Context menu: holding");
                ShowFlyout();
            }
        }

        private void ShowFlyout(Point? position = null)
        {
            var control = (FrameworkElement)AssociatedObject;
            MenuFlyout flyout = null;
            if (control.DataContext is IAudio)
            {
                flyout = ContextMenuHelper.GetTrackMenu(control.DataContext as IAudio, Context);
            }
            else if (control.DataContext is PlaylistVk)
            {
                flyout = ContextMenuHelper.GetPlaylistMenu(control.DataContext as PlaylistVk);
            }
            else
                flyout = FlyoutBase.GetAttachedFlyout(control) as MenuFlyout;
            if (flyout != null)
            {
                foreach (var menuFlyoutItemBase in flyout.Items)
                {
                    menuFlyoutItemBase.DataContext = control.DataContext;
                }

                try
                {
                    if (position != null)
                        flyout.ShowAt(null, position.Value);
                    else
                        flyout.ShowAt(control);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to open context menu. " + ex);
                }
            }
        }
    }
}
