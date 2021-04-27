using Meridian.Utils.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class PostTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var date = (DateTime)value;
            var hours = (DateTime.Now.ToUniversalTime() - date).TotalHours;

            if (date.Date == DateTime.Today)
                return date.ToString("t");

            if (DateTime.Today - date.Date == TimeSpan.FromDays(1))
                return $"{Resources.GetStringByKey("Yesterday").ToLower()} {date.ToString(Resources.GetStringByKey("Post_ShortTimeFormat"))}";

            return date.ToString(Resources.GetStringByKey("Post_FullTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}