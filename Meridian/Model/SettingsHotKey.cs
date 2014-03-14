using System.ComponentModel;

namespace Meridian.Model
{
    /// <summary>
    /// Модель горячей клавиши для экрана настроек
    /// </summary>
    public class SettingsHotkey : INotifyPropertyChanged
    {
        private bool _ctrl;
        private bool _alt;
        private bool _shift;
        private string _title;
        private string _key;

        public string Id { get; set; }

        public bool Ctrl
        {
            get { return _ctrl; }
            set
            {
                if (_ctrl == value)
                    return;

                _ctrl = value;
                OnPropertyChanged("Ctrl");
            }
        }


        public bool Shift
        {
            get { return _shift; }
            set
            {
                if (_shift == value)
                    return;

                _shift = value;
                OnPropertyChanged("Shift");
            }
        }


        public bool Alt
        {
            get { return _alt; }
            set
            {
                if (_alt == value)
                    return;

                _alt = value;
                OnPropertyChanged("Alt");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                if (_key == value)
                    return;

                _key = value;
                OnPropertyChanged("Key");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
