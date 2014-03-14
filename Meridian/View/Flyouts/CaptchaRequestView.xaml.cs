using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Meridian.Controls;
using Neptune.UI.Extensions;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for CaptchaRequestView.xaml
    /// </summary>
    public partial class CaptchaRequestView : UserControl
    {
        private string _captchaSid;
        private string _captchaImg;
        private string _captchaKey;

        public CaptchaRequestView(string captchaSid, string captchaImg)
        {
            _captchaImg = captchaImg;
            _captchaSid = captchaSid;

            InitializeComponent();


            Image.Source = new BitmapImage(new Uri(_captchaImg));

        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CaptchaTextBox.Text))
                Close(CaptchaTextBox.Text);
        }

        private void Close(string result = null)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close(result);
        }

        private void CaptchaTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(CaptchaTextBox.Text))
            {
                Close(CaptchaTextBox.Text);
            }
        }

        private void CaptchaRequestView_OnLoaded(object sender, RoutedEventArgs e)
        {
            CaptchaTextBox.Focus();
        }
    }
}
