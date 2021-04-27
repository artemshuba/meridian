using System;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class StringToLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value != null ? ((string)value).ToLowerInvariant() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
