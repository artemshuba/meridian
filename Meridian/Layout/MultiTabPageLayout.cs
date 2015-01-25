using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Meridian.Layout
{
    public class MultiTabPageLayout : LayoutBase
    {
        public static readonly DependencyProperty TabsProperty = DependencyProperty.Register(
            "Tabs", typeof(List<object>), typeof(MultiTabPageLayout), new PropertyMetadata(new List<object>(), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            
        }

        public List<object> Tabs
        {
            get { return (List<object>)GetValue(TabsProperty); }
            set
            {
                SetValue(TabsProperty, value);
            }
        }

        public MultiTabPageLayout()
        {
            Style = (Style)Application.Current.Resources["MultiTabPageLayoutStyle"];
        }
    }
}
