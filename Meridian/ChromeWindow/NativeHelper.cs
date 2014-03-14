using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Meridian.ChromeWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }

    public class NativeHelper
    {
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        public static bool IsDwmAvailable()
        {
            return (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Build >= 5600) && File.Exists(Environment.SystemDirectory + @"\dwmapi.dll");
        }

        public static bool IsGlassEnabled()
        {
            bool result = false;
            DwmIsCompositionEnabled(out result);
            return result;
        }

        public static void HideCloseButton(IntPtr handle)
        {
            SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
