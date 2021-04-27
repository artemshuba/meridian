using System;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Converters
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object TrueObject { get; set; }

        public object FalseObject { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is string)
            {
                Boolean.TryParse((string)value, out flag);
            }
            if (parameter != null)
            {
                bool bParam;
                if (bool.TryParse((string)parameter, out bParam) && bParam)
                {
                    flag = !flag;
                }
            }
            if (flag)
            {
                return TrueObject;
            }
            else
            {
                return FalseObject;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
