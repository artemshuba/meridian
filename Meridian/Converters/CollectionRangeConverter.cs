using Meridian.Interfaces;
using System;
using System.Collections;
using System.Linq;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class CollectionRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var list = value as IList;

            if (list == null)
                return null;

            var count = System.Convert.ToInt32(parameter);
            return list.OfType<IAudio>().Take(count);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
