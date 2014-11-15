using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Neptune.UI.Extensions;

namespace Meridian.Controls
{
    /// <summary>
    /// Interaction logic for LocalSearchControl.xaml
    /// </summary>
    public partial class LocalSearchControl : UserControl
    {
        #region IsActive property

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(LocalSearchControl), new PropertyMetadata(default(bool), IsActiveChanged));

        private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LocalSearchControl)d;
            if ((bool)e.NewValue)
            {
                //c.Visibility = Visibility.Visible;
            }
            else
            {
                //c.Visibility = Visibility.Collapsed;
                c.LocalSearchBox.Text = string.Empty;
            }
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        #endregion

        #region Source property

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(CollectionViewSource), typeof(LocalSearchControl), new PropertyMetadata(default(CollectionViewSource)));

        public CollectionViewSource Source
        {
            get { return (CollectionViewSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Filter property

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(Predicate<object>), typeof(LocalSearchControl), new PropertyMetadata(default(Predicate<object>)));

        public Predicate<object> Filter
        {
            get { return (Predicate<object>)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        #endregion

        #region Query property

        public static readonly DependencyProperty QueryProperty = DependencyProperty.Register(
            "Query", typeof(string), typeof(LocalSearchControl), new PropertyMetadata(default(string)));

        public string Query
        {
            get { return (string)GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }

        #endregion

        public bool IsFiltering { get; set; }

        public LocalSearchControl()
        {
            InitializeComponent();
        }

        public static LocalSearchControl GetForCurrentView()
        {
            return (Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is LocalSearchControl) as LocalSearchControl);
        }

        private void LocalSearchControl_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void LocalSearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Query = LocalSearchBox.Text;

            if (Source != null && Source.View != null)
            {
                Source.IsLiveFilteringRequested = !string.IsNullOrEmpty(Query);
                IsFiltering = !string.IsNullOrEmpty(Query);
                if (Source.View.Filter == null)
                    Source.View.Filter = Filter;
                else
                    Source.View.Refresh();
            }
        }

        private void CloseLocalSearchBoxButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsActive = false;
        }

        private void LocalSearchControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                IsActive = false;
        }

        private void LocalSearchControl_OnGotFocus(object sender, RoutedEventArgs e)
        {
            LocalSearchBox.Focus();
        }

        private void LocalSearchControl_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                //майкрософт, убей себя пожалуйста об стену
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => LocalSearchBox.Focus()));
            }
        }
    }
}
