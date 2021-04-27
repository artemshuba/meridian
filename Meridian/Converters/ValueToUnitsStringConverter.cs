using Jupiter.Utils.Helpers;
using Meridian.Utils.Helpers;
using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace Meridian.Converters
{
    public class ValueToUnitsStringConverter : IValueConverter
    {
        public string ResourceNameBase { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var count = (int)value;

            if (count != 0)
                return count + " " + StringHelper.LocalizeNumerals(count, 
                    Resources.GetStringByKey(ResourceNameBase + "Singular"),
                    Resources.GetStringByKey(ResourceNameBase + "Dual"),
                    Resources.GetStringByKey(ResourceNameBase + "Plural"), CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
