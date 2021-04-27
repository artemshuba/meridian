using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Windows.ApplicationModel;

namespace Jupiter.Mvvm
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [IgnoreDataMember]
        public bool IsInDesignMode
        {
            get { return DesignMode.DesignModeEnabled; }
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}