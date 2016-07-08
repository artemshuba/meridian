using System.Windows;
using System.Windows.Controls;

namespace Meridian.Layout
{
    public class TwoColumnPageLayout : LayoutBase
    {
        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
               "MainContent", typeof(object), typeof(TwoColumnPageLayout), new PropertyMetadata(default(object)));

        public object MainContent
        {
            get { return (object)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            "RightContent", typeof (object), typeof (TwoColumnPageLayout), new PropertyMetadata(default(object)));

        public object RightContent
        {
            get { return (object) GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public TwoColumnPageLayout()
        {
            Style = (Style)Application.Current.Resources["TwoColumnPageLayoutStyle"];
        }
    }
}
