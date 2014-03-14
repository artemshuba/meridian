using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Meridian.Resources.Localization;
using Neptune.UI.Extensions;
using VkLib.Core.Audio;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for EditAlbumView.xaml
    /// </summary>
    public partial class EditAlbumView : UserControl
    {
        private VkAudioAlbum _album;

        public EditAlbumView(VkAudioAlbum album)
        {
            InitializeComponent();

            _album = album;

            TitleTextBox.Text = _album.Title;

            if (album.Id != 0)
                Title.Text = MainResources.EditAlbumTitle;
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close(bool result = false)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close(result);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                return;

            _album.Title = TitleTextBox.Text;

            Close(true);
        }
    }
}
