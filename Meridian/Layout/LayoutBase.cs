using System.Windows;
using System.Windows.Controls;

namespace Meridian.Layout
{
    public class LayoutBase : Control
    {
        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            "HeaderHeight", typeof (double), typeof (LayoutBase), new PropertyMetadata(default(double)));

        public double HeaderHeight
        {
            get { return (double) GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }
    }
}
