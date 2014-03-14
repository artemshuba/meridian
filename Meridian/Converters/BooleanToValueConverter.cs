using System;
using System.Globalization;
using System.Windows.Data;

namespace Meridian.Converters
{
    public class BooleanToValueConverter : IValueConverter
    {
        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (bool) value;
            if (v)
                return TrueValue;
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
