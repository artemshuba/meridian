using Meridian.Enum;
using Meridian.Utils.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class SortTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sortType = (TracksSortType)value;

            switch (sortType)
            {
                case TracksSortType.DateAdded:
                    return Resources.GetStringByKey("Toolbar_SortByDateAdded");
                case TracksSortType.Title:
                    return Resources.GetStringByKey("Toolbar_SortByTitle");
                case TracksSortType.Artist:
                    return Resources.GetStringByKey("Toolbar_SortByArtist");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
