using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var d = (double)value;

            var gridLength = new GridLength(d);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var gridLength = (GridLength)value;

            return gridLength.Value;
        }
    }
}
