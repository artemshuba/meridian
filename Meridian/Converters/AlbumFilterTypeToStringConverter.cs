using Meridian.Enum;
using Meridian.Utils.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class AlbumFilterTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var filterType = (AlbumFilterType)value;

            switch (filterType)
            {
                case AlbumFilterType.All:
                    return Resources.GetStringByKey("Albums_FilterAll");
                case AlbumFilterType.Unsorted:
                    return Resources.GetStringByKey("Albums_FilterUnsorted");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}