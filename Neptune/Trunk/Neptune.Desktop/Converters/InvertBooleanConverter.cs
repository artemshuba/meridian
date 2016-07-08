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
    public class InvertBooleanConverter : IValueConverter
    {
#if DESKTOP || PHONE
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return false;
        }

#if DESKTOP || PHONE
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return false;
        }
    }
}
