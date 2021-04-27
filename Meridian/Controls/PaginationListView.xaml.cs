using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using System.Linq;
using System.Windows.Input;
using Windows.Devices.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.Controls
{
    public sealed partial class PaginationListView : UserControl
    {
        private ScrollViewer _scrollViewer;
        private bool _supportsTouch;

        #region ItemsSource

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(PaginationListView), new PropertyMetadata(null));

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        #endregion

        #region ItemTemplate

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(PaginationListView), new PropertyMetadata(null));


        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        #endregion

        #region ItemContainerStyle

        // Using a DependencyProperty as the backing store for ItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(PaginationListView), new PropertyMetadata(null));

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        #endregion

        // Using a DependencyProperty as the backing store for ItemClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(PaginationListView), new PropertyMetadata(null));

        public ICommand ItemClickCommand
        {
            get { return (ICommand)GetValue(ItemClickCommandProperty); }
            set { SetValue(ItemClickCommandProperty, value); }
        }

        public PaginationListView()
        {
            this.InitializeComponent();

            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateInertia;
        }

        private void ListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Disabled);
        }

        private void ListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //disable horizontal scroll for mouse
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Disabled);

                if (_scrollViewer != null)
                {
                    if (_scrollViewer.ScrollableWidth > 0 && _scrollViewer.HorizontalOffset != _scrollViewer.ScrollableWidth)
                        NextButton.Visibility = Visibility.Visible;
                    else
                        NextButton.Visibility = Visibility.Collapsed;
                }

                PrevButton.IsHitTestVisible = true;
                NextButton.IsHitTestVisible = true;

                var anim = (Storyboard)Resources["ShowControlButtonsAnim"];
                anim.Stop();
                anim.Begin();
            }
        }

        private void ListView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Disabled);
            else if (_supportsTouch)
                ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Auto);
        }

        //enable scroll back after mouse leave
        private void ListView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_supportsTouch)
                ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Auto);

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PrevButton.IsHitTestVisible = false;
                NextButton.IsHitTestVisible = false;

                var anim = (Storyboard)Resources["HideControlButtonsAnim"];
                anim.Stop();
                anim.Begin();
            }
        }


        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ChangeView(_scrollViewer.HorizontalOffset - _scrollViewer.ViewportWidth / 1.5, null, null);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ChangeView(_scrollViewer.HorizontalOffset + _scrollViewer.ViewportWidth / 1.5, null, null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            //not working, always null

            //_scrollViewer = ListView.GetVisualDescendents().OfType<ScrollViewer>().FirstOrDefault();
            //if (_scrollViewer != null)
            //    _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            var touchCapabilities = new TouchCapabilities();
            _supportsTouch = touchCapabilities.TouchPresent > 0 || DeviceHelper.IsMobile(); //on the emulator TouchCapabilities doesn't work correctly

            if (!_supportsTouch)
                ScrollViewer.SetHorizontalScrollMode(ListView, ScrollMode.Disabled);
        }

        private void ListView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_scrollViewer.HorizontalOffset == 0)
                PrevButton.Visibility = Visibility.Collapsed;
            else
                PrevButton.Visibility = Visibility.Visible;

            if (_scrollViewer.HorizontalOffset == _scrollViewer.ScrollableWidth)
                NextButton.Visibility = Visibility.Collapsed;
            else
                NextButton.Visibility = Visibility.Visible;
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            if (_scrollViewer == null)
            {
                //not working in Loaded event, so trying to get ScrollViewer here
                _scrollViewer = ListView.GetVisualDescendents().OfType<ScrollViewer>().FirstOrDefault();
                if (_scrollViewer != null)
                {
                    _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClickCommand?.Execute(e.ClickedItem);
        }
    }
}
