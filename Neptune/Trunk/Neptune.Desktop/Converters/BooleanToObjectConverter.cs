using System;
using System.Globalization;
#if DESKTOP
using System.Windows.Data;
#elif MODERN
using Windows.UI.Xaml.Data;

#elif PHONE
using System.Windows.Data;
#endif

namespace Neptune.UI.Converters
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object TrueObject { get; set; }

        public object FalseObject { get; set; }

#if MODERN
        public object Convert(object value, Type targetType, object parameter, string culture)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            bool flag = value is bool ? (bool)value : (value != null && !string.IsNullOrWhiteSpace(value.ToString()));

            return flag ? TrueObject : FalseObject;
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
