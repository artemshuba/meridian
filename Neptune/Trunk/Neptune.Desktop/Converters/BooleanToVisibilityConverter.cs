using System;
using System.Globalization;
using System.Windows;

#if DESKTOP
using System.Windows.Data;
#elif MODERN
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#elif PHONE
using System.Windows.Data;
#endif

// ReSharper disable once CheckNamespace
namespace Neptune.UI.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
#if MODERN
        public object Convert(object value, Type targetType, object parameter, string culture)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            var flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is string)
            {
                Boolean.TryParse((string)value, out flag);
            }
            if (parameter != null)
            {
                bool bParam;
                if (bool.TryParse((string)parameter, out bParam) && bParam)
                {
                    flag = !flag;
                }
            }
            if (flag)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

#if MODERN
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif

        {
            var back = ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
            if (parameter != null)
            {
                if ((bool)parameter)
                {
                    back = !back;
                }
            }
            return back;
        }
    }
}
