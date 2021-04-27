using System;
using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class IsEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var invert = false;
            if (parameter != null)
            {
                Boolean.TryParse(parameter.ToString(), out invert);
            }
            if (value == null) return invert ? Visibility.Visible : Visibility.Collapsed;

            if (value is string)
                return string.IsNullOrWhiteSpace((string)value) || invert ? Visibility.Collapsed : Visibility.Visible;

            if (value is IList)
            {
                bool empty = ((IList)value).Count == 0;
                if (invert)
                    empty = !empty;
                if (empty)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

            if (value is TimeSpan)
            {
                if (!invert)
                    return (TimeSpan)value != TimeSpan.Zero ? Visibility.Visible : Visibility.Collapsed;
                else
                    return (TimeSpan)value == TimeSpan.Zero ? Visibility.Collapsed : Visibility.Visible;
            }

            decimal number;
            if (Decimal.TryParse(value.ToString(), out number))
            {
                if (!invert)
                    return number > 0 ? Visibility.Visible : Visibility.Collapsed;
                else
                    return number > 0 ? Visibility.Collapsed : Visibility.Visible;

            }

            return invert ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
