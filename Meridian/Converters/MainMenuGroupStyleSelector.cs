using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Meridian.Converters
{
    public class MainMenuGroupStyleSelector : StyleSelector
    {
        public Style EmptyHeaderGroupStyle { get; set; }

        public Style NormalGroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var group = item as CollectionViewGroup;
            if (group == null)
                return null;

            if (string.IsNullOrEmpty((string) group.Name))
                return EmptyHeaderGroupStyle;

            return NormalGroupStyle;
        }
    }
}
