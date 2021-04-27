using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Meridian.Interfaces;
using Meridian.Services;
using Meridian.Utils.Helpers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.View.Controls
{
    public sealed partial class TrackControl : UserControl
    {
        private Brush _foregroundBrush;

        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(
            "Track", typeof(IAudio), typeof(TrackControl), new PropertyMetadata(default(IAudio), OnTrackChanged));

        private static void OnTrackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TrackControl)d;
            control.IsPlaying = e.NewValue == AudioService.Instance.CurrentPlaylist.CurrentItem;
        }

        public IAudio Track
        {
            get { return (IAudio)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof(bool), typeof(TrackControl), new PropertyMetadata(default(bool), IsPlayingChanged));

        private static void IsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TrackControl)d;
            if ((bool)e.NewValue == true)
            {
                control.TitleTextBlock.Opacity = 1;
                control.ArtistTextBlock.Opacity = 1;
                control.DurationTextBlock.Opacity = 1;

                control._foregroundBrush = control.Foreground;
                control.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
            }
            else
            {
                control.TitleTextBlock.Opacity = 0.8;
                control.ArtistTextBlock.Opacity = 0.5;
                control.DurationTextBlock.Opacity = 0.5;


                control.Foreground = control._foregroundBrush;
            }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        //Command dependency property
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(TrackControl), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Command that called on tap
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(TrackControl), new PropertyMetadata(default(object)));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Context.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(ContextMenuContext), typeof(TrackControl), new PropertyMetadata(ContextMenuContext.Common));

        public ContextMenuContext Context
        {
            get { return (ContextMenuContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public TrackControl()
        {
            this.InitializeComponent();
        }

        private void TrackControl_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Command?.CanExecute(CommandParameter) == true)
                Command?.Execute(CommandParameter);
        }
    }
}