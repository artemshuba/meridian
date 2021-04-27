using System;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var t = (TimeSpan)value;
            return t.ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotSupportedException();
        }
    }
}
