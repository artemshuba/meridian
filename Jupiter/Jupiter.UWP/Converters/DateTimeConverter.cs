using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                return null;

            var d = (DateTime)value;
            return d.ToString((string)parameter, !string.IsNullOrEmpty(culture) ? new CultureInfo(culture) : null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotSupportedException();
        }
    }
}
