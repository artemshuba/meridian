using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meridian.ViewModel;

namespace Meridian.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsHotkeys.xaml
    /// </summary>
    public partial class SettingsHotkeysView : Page
    {
        private SettingsViewModel _viewModel;

        public SettingsHotkeysView()
        {
            InitializeComponent();
        }

        private void SettingsHotkeysView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (SettingsViewModel)this.DataContext;
        }

        private void HotkeyTextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System || e.Key == Key.LeftAlt || e.Key == Key.LeftCtrl || e.Key == Key.LeftShift || e.Key == Key.RightAlt || e.Key == Key.RightCtrl || e.Key == Key.RightShift
                || e.Key == Key.Escape)
            {
                return;
            }

            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            _viewModel.CanSave = true;

            if (e.Key == Key.Back)
            {
                textBox.Text = "None";
                e.Handled = true;
                return;
            }

            textBox.Text = e.Key.ToString();
            e.Handled = true;
        }

        private void CheckBoxOnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.CanSave = true;
        }
    }
}
