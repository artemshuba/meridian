using System;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value != null ? ((string)value).ToUpperInvariant() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
