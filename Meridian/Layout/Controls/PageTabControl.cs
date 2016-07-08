using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Neptune.Extensions;

namespace Meridian.Layout.Controls
{
    /// <summary>
    /// Tab control which hides tabs panel if there are only one tab
    /// </summary>
    public class PageTabControl : TabControl
    {
        private bool _isTabsVisible = true;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            ShowHideTabsPanel(!(Items.IsNullOrEmpty() || Items.Count == 1));
        }

        private void ShowHideTabsPanel(bool show)
        {
            if (_isTabsVisible == show)
                return;

            var tabPanel = GetTemplateChild("headerPanel") as TabPanel;
            if (tabPanel != null)
            {
                tabPanel.Height = show ? double.NaN : 0;

                _isTabsVisible = show;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ShowHideTabsPanel(!(Items.IsNullOrEmpty() || Items.Count == 1));
        }
    }
}
