using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace Meridian.View.Settings
{
    /// <summary>
    /// Interaction logic for SettingsAboutView.xaml
    /// </summary>
    public partial class SettingsAboutView : Page
    {
        public SettingsAboutView()
        {
            InitializeComponent();
        }

        private void SiteLink_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://meridianvk.com");
        }

        private void LastFmLink_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://last.fm");
        }

        private void EchonestLink_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://echonest.com");
        }

        private void NAudioLink_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://naudio.codeplex.com");
        }
    }
}
