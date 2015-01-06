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
    public class NullToBooleanConverter : IValueConverter
    {
#if MODERN
        public object Convert(object value, Type targetType, object parameter, string culture)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            var invert = false;

            if (parameter != null)
            {
                Boolean.TryParse(parameter.ToString(), out invert);
            }

            if (value == null) return invert;

            if (value is string)
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                    return invert;
                else
                    return !invert;
            }

            return true;
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
