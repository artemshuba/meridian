using Meridian.Enum;
using Meridian.Utils.Helpers;
using System;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class StartPageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var startPage = (StartPage)value;

            switch (startPage)
            {
                case StartPage.Explore:
                    return Resources.GetStringByKey("Settings_StartPageExplore");
                case StartPage.Mymusic:
                    return Resources.GetStringByKey("Settings_StartPageMyMusic");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
