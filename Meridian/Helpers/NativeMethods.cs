using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Meridian.Helpers
{
    public static class NativeMethods
    {
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void SizeWindow(IntPtr handle)
        {
            SendMessage(handle, 274, 61448, IntPtr.Zero);
        }

        public static void SizeWindowLeft(IntPtr handle)
        {
            SendMessage(handle, 274, 61441, IntPtr.Zero);
        }

        public static void SizeWindowTop(IntPtr handle)
        {
            SendMessage(handle, 274, 61443, IntPtr.Zero);
        }

        public static void SizeWindowBottom(IntPtr handle)
        {
            SendMessage(handle, 274, 61446, IntPtr.Zero);
        }

        public static void SizeWindowRight(IntPtr handle)
        {
            SendMessage(handle, 274, 61442, IntPtr.Zero);
        }
    }
}
