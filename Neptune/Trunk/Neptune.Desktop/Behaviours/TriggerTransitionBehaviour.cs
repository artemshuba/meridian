#if DESKTOP || PHONE
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
#elif MODERN
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#endif

// ReSharper disable once CheckNamespace
namespace Neptune.UI.Behaviours
{
    public class TriggerTransitionBehaviour : Behavior<FrameworkElement>
    {

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(TriggerTransitionBehaviour), new PropertyMetadata(default(bool), PropertyChangedCallback));

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = (TriggerTransitionBehaviour)d;

            if ((bool)e.NewValue)
                b.TransitionIn();
            else
                b.TransitionOut();
        }


        public static readonly DependencyProperty AnimationOutProperty =
            DependencyProperty.Register("AnimationOut", typeof(Storyboard), typeof(TriggerTransitionBehaviour), new PropertyMetadata(default(Storyboard)));

        public Storyboard AnimationOut
        {
            get { return (Storyboard)GetValue(AnimationOutProperty); }
            set { SetValue(AnimationOutProperty, value); }
        }

        public static readonly DependencyProperty AnimationInProperty =
            DependencyProperty.Register("AnimationIn", typeof(Storyboard), typeof(TriggerTransitionBehaviour), new PropertyMetadata(default(Storyboard)));

        public Storyboard AnimationIn
        {
            get { return (Storyboard)GetValue(AnimationInProperty); }
            set { SetValue(AnimationInProperty, value); }
        }

        private void TransitionOut()
        {
            if (AssociatedObject == null)
                return;

            if (AnimationOut == null)
            {
                return;
            }

            AnimationOut.Stop();
            Storyboard.SetTarget(AnimationOut, AssociatedObject);
            AnimationOut.Begin();
        }

        private void TransitionIn()
        {
            if (AssociatedObject == null)
                return;

            if (AnimationIn != null)
            {
#if DESKTOP
                if (Storyboard.GetTarget(AnimationIn) == AssociatedObject)
#endif
                    AnimationIn.Stop();

                Storyboard.SetTarget(AnimationIn, AssociatedObject);
                AnimationIn.Begin();

            }
        }
    }
}
