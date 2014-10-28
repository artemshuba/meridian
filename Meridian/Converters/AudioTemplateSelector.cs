using System.Windows;
using System.Windows.Controls;
using Meridian.Model;

namespace Meridian.Converters
{
    public class AudioTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate LocalAudioTemplate { get; set; }

        public DataTemplate PostTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AudioPost)
                return PostTemplate;

            if (item is LocalAudio)
                return LocalAudioTemplate;

            if (item is Audio)
                return AudioTemplate;

            return AudioTemplate;
        }
    }
}
