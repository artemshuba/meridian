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
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
#if !MODERN
 CultureInfo culture)
#else
 string culture)
#endif
        {
            var t = (TimeSpan)value;
            return t.ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if !MODERN
 CultureInfo culture)
#else
 string culture)
#endif
        {
            throw new NotSupportedException();
        }
    }
}
