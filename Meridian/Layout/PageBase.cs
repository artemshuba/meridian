using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Meridian.Layout
{
    public class PageBase : Page
    {
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            "Layout", typeof(LayoutBase), typeof(PageBase), new PropertyMetadata(default(LayoutBase)));

        public LayoutBase Layout
        {
            get { return (LayoutBase)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(List<object>), typeof(PageBase), new PropertyMetadata(new List<object>()));

        public new List<object> Content
        {
            get { return (List<object>)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(PageBase), new PropertyMetadata(default(object)));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderMenuItemsProperty = DependencyProperty.Register(
            "HeaderMenuItems", typeof(List<MenuItem>), typeof(PageBase), new PropertyMetadata(new List<MenuItem>()));

        public List<MenuItem> HeaderMenuItems
        {
            get { return (List<MenuItem>)GetValue(HeaderMenuItemsProperty); }
            set { SetValue(HeaderMenuItemsProperty, value); }
        }

        public PageBase()
        {
            Style = (Style)Application.Current.Resources["PageBaseStyle"];

            HeaderMenuItems = new List<MenuItem>();
            Content = new List<object>();
        }
    }
}
