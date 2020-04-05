using Microsoft.Toolkit.Wpf.UI.XamlHost;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Meridian.Wrappers
{
    [ContentProperty(nameof(Items))]
    public class UWPListView : WindowsXamlHost
    {
        internal Windows.UI.Xaml.Controls.ListView UwpControl => ChildInternal as Windows.UI.Xaml.Controls.ListView;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

        public ObservableCollection<DependencyObject> Items { get; }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(UWPListView), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UWPListView)d;
            control.UwpControl.ItemsSource = e.NewValue;
        }


        public UWPListView() : this(typeof(Windows.UI.Xaml.Controls.ListView).FullName)

        {

        }

        protected UWPListView(string typeName)
        {
            InitialTypeName = typeName;
            Items = new ObservableCollection<DependencyObject>();
        }

        private void RelocateChildToUwpControl(WindowsXamlHostBase obj)

        {
            if (obj.GetUwpInternalObject() is Windows.UI.Xaml.UIElement child)
            {

                UwpControl.Items.Add(child);
            }
        }

        protected override void OnInitialized(EventArgs e)

        {

            // Bind dependency properties across controls

            // properties of FrameworkElement

            /*Bind(nameof(Style), StyleProperty, Windows.UI.Xaml.Controls.ListView.StyleProperty);

            Bind(nameof(MaxHeight), MaxHeightProperty, Windows.UI.Xaml.Controls.ListView.MaxHeightProperty);

            Bind(nameof(FlowDirection), FlowDirectionProperty, Windows.UI.Xaml.Controls.ListView.FlowDirectionProperty);

            Bind(nameof(Margin), MarginProperty, Windows.UI.Xaml.Controls.ListView.MarginProperty);

            Bind(nameof(HorizontalAlignment), HorizontalAlignmentProperty, Windows.UI.Xaml.Controls.ListView.HorizontalAlignmentProperty);

            Bind(nameof(VerticalAlignment), VerticalAlignmentProperty, Windows.UI.Xaml.Controls.ListView.VerticalAlignmentProperty);

            Bind(nameof(MinHeight), MinHeightProperty, Windows.UI.Xaml.Controls.ListView.MinHeightProperty);

            Bind(nameof(Height), HeightProperty, Windows.UI.Xaml.Controls.ListView.HeightProperty);

            Bind(nameof(MinWidth), MinWidthProperty, Windows.UI.Xaml.Controls.ListView.MinWidthProperty);

            Bind(nameof(MaxWidth), MaxWidthProperty, Windows.UI.Xaml.Controls.ListView.MaxWidthProperty);

            Bind(nameof(UseLayoutRounding), UseLayoutRoundingProperty, Windows.UI.Xaml.Controls.ListView.UseLayoutRoundingProperty);

            Bind(nameof(Name), NameProperty, Windows.UI.Xaml.Controls.ListView.NameProperty);

            Bind(nameof(Tag), TagProperty, Windows.UI.Xaml.Controls.ListView.TagProperty);

            Bind(nameof(DataContext), DataContextProperty, Windows.UI.Xaml.Controls.ListView.DataContextProperty);

            Bind(nameof(Width), WidthProperty, Windows.UI.Xaml.Controls.ListView.WidthProperty); */


            Items.OfType<WindowsXamlHostBase>().ToList().ForEach(RelocateChildToUwpControl);

            base.OnInitialized(e);

            Loaded += UWPListView_Loaded;
        }

        private void UWPListView_Loaded(object sender, RoutedEventArgs e)
        {
            var template = Meridian.WrappedControls.App.Current.Resources["AudioTemplate"] as Windows.UI.Xaml.DataTemplate;

            UwpControl.ItemTemplate = template;
            UwpControl.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
        }
    }
}
