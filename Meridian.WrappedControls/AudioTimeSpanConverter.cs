using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Meridian.WrappedControls
{
    public class AudioTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            if (value is TimeSpan)
            {
                timeSpan = (TimeSpan)value;
            }
            else
            {
                double num;
                if (double.TryParse(value.ToString(), out num))
                    timeSpan = TimeSpan.FromSeconds(num);
            }
            if (timeSpan.Hours > 0)
                return timeSpan.ToString("h\\:mm\\:ss");
            return timeSpan.ToString("m\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
