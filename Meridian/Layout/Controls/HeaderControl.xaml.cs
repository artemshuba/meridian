using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Meridian.Layout.Controls
{
    /// <summary>
    /// Interaction logic for HeaderControl.xaml
    /// </summary>
    public partial class HeaderControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(HeaderControl), new PropertyMetadata(default(object)));

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty SubHeaderProperty = DependencyProperty.Register(
            "SubHeader", typeof (string), typeof (HeaderControl), new PropertyMetadata(default(string)));

        public string SubHeader
        {
            get { return (string) GetValue(SubHeaderProperty); }
            set { SetValue(SubHeaderProperty, value); }
        }

        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
            "MenuItems", typeof(List<MenuItem>), typeof(HeaderControl), new PropertyMetadata(new List<MenuItem>(), OnMenuItemsPropertyChanged));

        private static void OnMenuItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HeaderControl)d;
            if (e.NewValue != null && ((List<MenuItem>)e.NewValue).Count > 0)
                control.HeaderButton.IsEnabled = true;
            else
                control.HeaderButton.IsEnabled = false;
        }

        public List<MenuItem> MenuItems
        {
            get { return (List<MenuItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public HeaderControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach (var menuItem in MenuItems)
            {
                menuItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(MenuItemClicked));
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (var menuItem in MenuItems)
            {
                menuItem.RemoveHandler(MenuItem.ClickEvent, new RoutedEventHandler(MenuItemClicked));
            }
        }

        private void MenuItemClicked(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetValue(Popup.IsOpenProperty, false);
        }
    }
}
