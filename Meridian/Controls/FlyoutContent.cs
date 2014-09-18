using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using Neptune.UI.Extensions;

namespace Meridian.Controls
{
    public class FlyoutContent : UserControl
    {
        #region Commands

        public RelayCommand CloseCommand { get; private set; }

        #endregion

        public FlyoutContent()
        {
            InitializeCommand();
        }

        private void InitializeCommand()
        {
            CloseCommand = new RelayCommand(Close);
        }

        protected void Close()
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close();
        }
    }
}
