using System;
using System.Globalization;

#if DESKTOP
using System.Windows.Data;
#elif MODERN
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#elif PHONE
using System.Windows.Data;
#endif

namespace Neptune.UI.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
#if MODERN
        public object Convert(object value, Type targetType, object parameter, string culture)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            return value != null ? ((string)value).ToUpperInvariant() : null;
        }

#if MODERN
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif

        {
            throw new NotImplementedException();
        }
    }
}
