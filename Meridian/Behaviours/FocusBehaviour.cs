using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Meridian.Behaviours
{
    public class FocusBehavior : Behavior<Control>
    {
        /// <summary>
        /// IsFocused dependency property
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
            "IsFocused",
            typeof(bool),
            typeof(FocusBehavior),
            new PropertyMetadata(false, (d, e) =>
            {
                if ((bool)e.NewValue && ((FocusBehavior)d).AssociatedObject != null)
                    ((FocusBehavior)d).AssociatedObject.Focus();
            }));

        /// <summary>
        /// HasInitialFocus dependency property
        /// </summary>
        public static readonly DependencyProperty HasInitialFocusProperty =
            DependencyProperty.Register(
            "HasInitialFocus",
            typeof(bool),
            typeof(FocusBehavior),
            new PropertyMetadata(false, null));

        /// <summary>
        /// Имеет ли контрол фокус
        /// </summary>
        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        /// <summary>
        /// Имеет ли контрол фокус при инициализации
        /// </summary>
        public bool HasInitialFocus
        {
            get { return (bool)GetValue(HasInitialFocusProperty); }
            set { SetValue(HasInitialFocusProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += (sender, args) => IsFocused = true;
            AssociatedObject.LostFocus += (sender, a) => IsFocused = false;
            AssociatedObject.Loaded += (o, a) =>
            {
                if (HasInitialFocus || IsFocused)
                    AssociatedObject.Focus();
            };

            base.OnAttached();
        }
    }
}
