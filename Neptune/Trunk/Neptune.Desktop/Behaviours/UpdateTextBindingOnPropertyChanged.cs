#if DESKTOP || PHONE
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
#elif MODERN
using Windows.UI.Interactivity;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#endif

// ReSharper disable once CheckNamespace
namespace Neptune.UI.Behaviours
{
    /// <summary>
    /// Custom behavior that updates the source of a binding on a text box as the text changes.
    /// </summary>
    public class UpdateTextBindingOnPropertyChanged : Behavior<TextBox>
    {
        private BindingExpression _expression;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            this._expression = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            this.AssociatedObject.TextChanged += this.OnTextChanged;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.TextChanged -= this.OnTextChanged;
            this._expression = null;
        }

#if DESKTOP || PHONE
        private void OnTextChanged(object sender, EventArgs args)
#elif MODERN
       private void OnTextChanged(object sender, TextChangedEventArgs args)
#endif
        {
            if (_expression != null)
                this._expression.UpdateSource();
        }
    }
}
