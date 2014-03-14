using System;
using System.Globalization;
using System.Windows.Data;

namespace Meridian.Converters
{
    public class AudioTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan) value;
            if (timeSpan.Hours > 0)
                return timeSpan.ToString("h\\:mm\\:ss");
            return timeSpan.ToString("m\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
