using Jupiter.Mvvm;
using Meridian.Controls;
using System;

namespace Meridian.ViewModel.Common
{
    public class PopupViewModelBase : ViewModelBase, IPopupContent
    {
        public event EventHandler<object> CloseRequested;

        #region Commands

        public DelegateCommand<object> CloseCommand { get; protected set; }

        #endregion

        protected override void InitializeCommands()
        {
            CloseCommand = new DelegateCommand<object>(result =>
            {
                CloseRequested?.Invoke(this, result);
            });
        }

        protected void Close(object result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}