using System.Windows;
using System.Windows.Controls;
using Meridian.Model;

namespace Meridian.Converters
{
    public class AudioTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate PostTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var audio = item as Audio;
            if (audio == null)
                return null;

            if (audio is AudioPost)
                return PostTemplate;

            return AudioTemplate;
        }
    }
}
