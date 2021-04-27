using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.Controls
{
    public class ToolbarItem : DependencyObject
    {
        public string Title { get; set; }
    }

    /// <summary>
    /// Toolbar button
    /// </summary>
    public class ToolbarButton : ToolbarItem
    {
        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public IconElement Icon { get; set; }
    }

    /// <summary>
    /// Control for picking items in toolbar
    /// </summary>
    public class ToolbarPicker : ToolbarItem
    {
        public List<ToolbarButton> Items { get; set; } = new List<ToolbarButton>();

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ToolbarButton), typeof(ToolbarPicker), new PropertyMetadata(null, (d, e) =>
            {
                var control = (ToolbarPicker)d;

                control.OnSelectedItemChanged?.Invoke(control.Items.IndexOf(e.NewValue as ToolbarButton));
            }));

        public ToolbarButton SelectedItem
        {
            get { return (ToolbarButton)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public Action<int> OnSelectedItemChanged;
    }

    [ContentProperty(Name = "Items")]
    public sealed partial class ToolbarControl : UserControl
    {
        //Used to store last known width of item
        private class ToolbarItemWrapper
        {
            public double ActualWidth { get; set; }

            public ToolbarItem Item { get; set; }
        }

        // Using a DependencyProperty as the backing store for Buttons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<ToolbarItem>), typeof(ToolbarControl), new PropertyMetadata(new ObservableCollection<ToolbarItem>(), (d, e) =>
            {
                var control = (ToolbarControl)d;

                var oldCollection = e.OldValue as ObservableCollection<ToolbarItem>;

                if (oldCollection != null)
                {
                    oldCollection.CollectionChanged -= control.Items_CollectionChanged;

                    foreach (var item in oldCollection)
                        control.RemoveItem(item);
                }

                var newCollection = e.NewValue as ObservableCollection<ToolbarItem>;

                if (newCollection != null)
                {
                    newCollection.CollectionChanged += control.Items_CollectionChanged;

                    foreach (var item in newCollection)
                        control.PlaceItem(item);
                }
            }));

        public ObservableCollection<ToolbarItem> Items
        {
            get { return (ObservableCollection<ToolbarItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public ToolbarControl()
        {
            this.InitializeComponent();

            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void rootElement_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Items != null)
                Items.CollectionChanged -= Items_CollectionChanged;
        }


        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems.OfType<ToolbarItem>().ToList();
            var oldItems = e.OldItems.OfType<ToolbarItem>().ToList();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in newItems)
                        AddItemToPanel(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in oldItems)
                        RemoveItemFromPanel(item);
                    break;
            }
        }

        private void PlaceItem(ToolbarItem item)
        {
            var control = AddItemToPanel(item);
            control.Opacity = 0;
            control.Loaded += Control_Loaded;
        }

        private void RemoveItem(ToolbarItem item)
        {
            int i = ItemsPanel.Children.Count - 1;
            while (i >= 0)
            {
                var control = (ContentControl)ItemsPanel.Children[i];
                if (control.Content == item)
                {
                    RemoveItemFromPanel(item);
                    break;
                }

                i--;
            }

            i = MenuFlyout.Items.Count - 1;
            while (i >= 0)
            {
                var menuItem = MenuFlyout.Items[i];

                var itemWrapper = (ToolbarItemWrapper)menuItem.DataContext;
                if (itemWrapper.Item == item)
                {
                    RemoveItemFromMenu(itemWrapper);
                    break;
                }

                i--;
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var control = (FrameworkElement)sender;
            control.Loaded -= Control_Loaded;

            UpdateItems();

            control.Opacity = 1;
        }

        private Control AddItemToPanel(ToolbarItem item)
        {
            var contentControl = new ContentControl();
            contentControl.Content = item;
            contentControl.VerticalContentAlignment = VerticalAlignment.Stretch;

            if (item is ToolbarButton)
                contentControl.ContentTemplate = (DataTemplate)Resources["ToolbarButtonTemplate"];
            else if (item is ToolbarPicker)
                contentControl.ContentTemplate = (DataTemplate)Resources["ToolbarPickerTemplate"];

            ItemsPanel.Children.Add(contentControl);

            return contentControl;
        }

        private void RemoveItemFromPanel(ToolbarItem item)
        {
            var control = ItemsPanel.Children.OfType<ContentControl>().FirstOrDefault(c => c.Content == item);
            control.ContentTemplate = null; //doesn't remove correctly without this
            ItemsPanel.Children.Remove(control);
        }

        private void UpdateItems()
        {
            var totalItemsWidth = ItemsPanel.Children.OfType<ContentControl>().Sum(c => c.ActualWidth);
            if (totalItemsWidth > this.ActualWidth - MenuButton.ActualWidth)
            {
                int i = ItemsPanel.Children.Count - 1;
                while (i >= 0)
                {
                    var control = (ContentControl)ItemsPanel.Children[i];

                    var position = control.TransformToVisual(ItemsPanel).TransformPoint(new Point());

                    if (position.X + control.ActualWidth < this.ActualWidth - MenuButton.ActualWidth)
                    {
                        break;
                    }
                    else
                    {
                        var item = (ToolbarItem)control.Content;
                        var itemWrapper = new ToolbarItemWrapper() { ActualWidth = control.ActualWidth, Item = item };
                        control.ContentTemplate = null; //doesn't remove correctly without this
                        ItemsPanel.Children.Remove(control);
                        AddItemToMenu(itemWrapper);
                    }

                    i--;
                }
            }
            else
            {
                int i = MenuFlyout.Items.Count - 1;
                while (i >= 0)
                {
                    var menuItem = MenuFlyout.Items[i];
                    var itemWrapper = (ToolbarItemWrapper)menuItem.DataContext;

                    if (totalItemsWidth + itemWrapper.ActualWidth <= this.ActualWidth - MenuButton.ActualWidth)
                    {
                        RemoveItemFromMenu(itemWrapper);

                        AddItemToPanel(itemWrapper.Item);

                        totalItemsWidth += itemWrapper.ActualWidth;
                    }
                    else
                        break;

                    i--;
                }
            }
        }

        private void rootElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItems();
        }

        private void AddItemToMenu(ToolbarItemWrapper item)
        {
            if (item.Item is ToolbarPicker)
            {
                var pickerItem = (ToolbarPicker)item.Item;
                var menuItem = new MenuFlyoutSubItem();
                menuItem.DataContext = item;
                menuItem.Text = item.Item.Title;

                foreach (var toolbarSubItem in ((ToolbarPicker)item.Item).Items)
                {
                    var menuSubItem = new ToggleMenuFlyoutItem();
                    menuSubItem.DataContext = toolbarSubItem;
                    menuSubItem.Text = toolbarSubItem.Title;
                    menuSubItem.Click += MenuSubItem_Click;
                    menuSubItem.IsChecked = toolbarSubItem == pickerItem.SelectedItem;

                    menuItem.Items.Add(menuSubItem);
                }

                MenuFlyout.Items.Add(menuItem);
            }
            else
            {
                var menuItem = new MenuFlyoutItem();
                menuItem.DataContext = item;
                menuItem.Text = item.Item.Title;
                menuItem.Command = ((ToolbarButton)item.Item).Command;

                MenuFlyout.Items.Add(menuItem);
            }

            MenuButton.Opacity = 1;
        }

        private void RemoveItemFromMenu(ToolbarItemWrapper item)
        {
            var menuItem = MenuFlyout.Items.FirstOrDefault(i => i.DataContext == item);

            if (menuItem is MenuFlyoutSubItem)
            {
                foreach (MenuFlyoutItem menuSubItem in ((MenuFlyoutSubItem)menuItem).Items)
                {
                    menuSubItem.Click -= MenuSubItem_Click;
                }
            }

            MenuFlyout.Items.Remove(menuItem);

            if (MenuFlyout.Items.Count > 0)
                MenuButton.Opacity = 1;
            else
                MenuButton.Opacity = 0;
        }

        private void MenuSubItem_Click(object sender, RoutedEventArgs e)
        {
            var menuSubItem = (MenuFlyoutItem)sender;
            var pickerItem = (ToolbarButton)menuSubItem.DataContext;

            MenuFlyoutSubItem menuItem = null;

            foreach (MenuFlyoutItemBase item in MenuFlyout.Items)
            {
                var itemWrapper = (ToolbarItemWrapper)item.DataContext;
                if (itemWrapper.Item is ToolbarPicker)
                {
                    var picker = (ToolbarPicker)itemWrapper.Item;
                    if (picker.Items.Any(i => i == pickerItem))
                    {
                        menuItem = (MenuFlyoutSubItem)item;
                        break;
                    }
                }
            }

            if (menuItem == null)
                return;

            var toolbarPicker = (ToolbarPicker)((ToolbarItemWrapper)menuItem.DataContext).Item;

            toolbarPicker.SelectedItem = (ToolbarButton)menuSubItem.DataContext;

            foreach (var item in menuItem.Items)
            {
                if (item is ToggleMenuFlyoutItem)
                {
                    ((ToggleMenuFlyoutItem)item).IsChecked = item.DataContext == toolbarPicker.SelectedItem;
                }
            }
        }
    }
}