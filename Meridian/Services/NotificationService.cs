using Meridian.Controls;

namespace Meridian.Services
{
    public static class NotificationService
    {
        private static NotificationControl _control;

        public static void Initialize(NotificationControl control)
        {
            _control = control;
        }

        public static void Notify(string message)
        {
            _control.Status = message;
        }

        public static void NotifyProgressStarted(string message = null)
        {
            _control.Progress = 0;

            if (!string.IsNullOrEmpty(message))
                _control.Status = message;
        }

        public static void NotifyProgressChanged(int progress, string message = null)
        {
            _control.Progress += progress;

            if (!string.IsNullOrEmpty(message))
                _control.Status = message;
        }

        public static void NotifyProgressFinished(string message = null)
        {
            _control.Progress = 100;

            if (!string.IsNullOrEmpty(message))
                _control.Status = message;
        }
    }
}
