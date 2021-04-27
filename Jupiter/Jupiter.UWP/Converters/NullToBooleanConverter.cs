using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
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

            if (value is IList)
            {
                bool empty = ((IList)value).Count == 0;
                if (invert)
                    empty = !empty;
                if (empty)
                    return false;
                else
                    return true;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
