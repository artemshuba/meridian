using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Meridian.ChromeWindow
{
    public class ChromeWindowBehaviour : Behavior<Window>
    {
        private const int WM_ERASEBKGND = 0x14;
        private const int WM_NCCALCSIZE = 0x83;
        private const int WM_NCPAINT = 0x85;
        private const int WM_WINDOWPOSCHANGING = 0x46;
        private const int WM_DWMCOMPOSITIONCHANGED = 0x31e;

        private HwndSource _hwndSource;
        private IntPtr _handle;

        protected override void OnAttached()
        {
            if (!NativeHelper.IsDwmAvailable() || !NativeHelper.IsGlassEnabled())
            {
                base.AssociatedObject.WindowStyle = WindowStyle.None;
                base.AssociatedObject.ResizeMode = ResizeMode.CanMinimize;
                base.AssociatedObject.AllowsTransparency = true;
                base.AssociatedObject.Background = Brushes.Transparent;
            }

            if (base.AssociatedObject.IsInitialized)
            {
                if (!NativeHelper.IsDwmAvailable() || !NativeHelper.IsGlassEnabled())
                {
                    ApplyFakeShadow();
                }
                else
                {
                    if (base.AssociatedObject.MinWidth > 30)
                        base.AssociatedObject.MinWidth -= 30;
                    if (base.AssociatedObject.MinHeight > 30)
                        base.AssociatedObject.MinHeight -= 30;
                    this.AddHwndHook();
                }
            }
            else
            {
                base.AssociatedObject.SourceInitialized += this.AssociatedObjectSourceInitialized;
            }

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.RemoveHwndHook();
            base.OnDetaching();
        }

        void AddHwndHook()
        {
            _hwndSource = PresentationSource.FromVisual(base.AssociatedObject) as HwndSource;
            if (_hwndSource != null)
            {
                _handle = _hwndSource.Handle;
                NativeHelper.HideCloseButton(_handle);
                _hwndSource.AddHook(this.WndProc);
            }
        }

        void RemoveHwndHook()
        {
            base.AssociatedObject.SourceInitialized -= this.AssociatedObjectSourceInitialized;
            this._hwndSource.RemoveHook(this.WndProc);
        }

        private void AssociatedObjectSourceInitialized(object sender, EventArgs e)
        {
            if (!NativeHelper.IsDwmAvailable() || !NativeHelper.IsGlassEnabled())
            {
                ApplyFakeShadow();
            }
            else
            {
                if (base.AssociatedObject.MinWidth > 30)
                    base.AssociatedObject.MinWidth -= 30;
                if (base.AssociatedObject.MinHeight > 30)
                    base.AssociatedObject.MinHeight -= 30;
                this.AddHwndHook();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ERASEBKGND:
                    System.Drawing.Graphics.FromHdc(wParam).Clear(System.Drawing.Color.White);
                    handled = true;
                    break;

                case WM_NCCALCSIZE:
                    if (wParam == new IntPtr(1))
                    {
                        handled = true;
                    }
                    break;

                case 0x86:
                    {
                        IntPtr ptr = NativeHelper.DefWindowProc(hwnd, msg, wParam, new IntPtr(-1));
                        handled = true;
                        return ptr;
                    }


                case WM_NCPAINT:
                    if (NativeHelper.IsDwmAvailable() && NativeHelper.IsGlassEnabled())
                        this.TryApplyNativeShadow();
                    break;

                case WM_WINDOWPOSCHANGING:
                    if (!_nativeShadowApplied)
                    {
                        TryApplyNativeShadow();
                    }
                    break;

                case WM_DWMCOMPOSITIONCHANGED:
                    if (NativeHelper.IsDwmAvailable() && NativeHelper.IsGlassEnabled())
                        TryApplyNativeShadow();
                    handled = true;
                    break;
                case 800:
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private bool _nativeShadowApplied;
        private void TryApplyNativeShadow()
        {
            if (NativeHelper.IsDwmAvailable())
            {
                base.AssociatedObject.WindowStyle = WindowStyle.SingleBorderWindow;

                if (_fakeShadowApplied)
                {
                    var rootPanel = base.AssociatedObject.Content as Panel;
                    if (rootPanel != null)
                    {
                        rootPanel.Margin = new Thickness(0);
                        rootPanel.Children.RemoveAt(0);
                    }

                    _fakeShadowApplied = false;
                }

                Margins margins;
                margins.bottomHeight = 1;
                margins.leftWidth = 0;
                margins.rightWidth = 0;
                margins.topHeight = 0;
                var helper = new WindowInteropHelper(base.AssociatedObject);
                NativeHelper.DwmExtendFrameIntoClientArea(helper.Handle, ref margins);
            }

            _nativeShadowApplied = true;
        }

        private bool _fakeShadowApplied;
        private void ApplyFakeShadow()
        {
            if (!_fakeShadowApplied)
            {
                //base.AssociatedObject.Width += 30;
                //base.AssociatedObject.Height += 30;

                var rootPanel = base.AssociatedObject.Content as Panel;
                if (rootPanel != null)
                {
                    var border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.Gray);
                    border.BorderThickness = new Thickness(1);

                    base.AssociatedObject.Content = null;

                    border.Child = rootPanel;
                    base.AssociatedObject.Content = border;

                    //    rootPanel.Margin = new Thickness(15);

                    //    var fakeShadowRectangle = new Rectangle();
                    //    fakeShadowRectangle.Fill = Brushes.Black;
                    //    fakeShadowRectangle.Opacity = 0.8;
                    //    fakeShadowRectangle.Margin = new Thickness(-1);
                    //    Grid.SetRowSpan(fakeShadowRectangle, 999);
                    //    Grid.SetColumnSpan(fakeShadowRectangle, 999);

                    //    var fakeShadowBlur = new BlurEffect();
                    //    fakeShadowBlur.Radius = 15;

                    //    fakeShadowRectangle.Effect = fakeShadowBlur;

                    //    rootPanel.Children.Insert(0, fakeShadowRectangle);
                }

                _fakeShadowApplied = true;
            }
        }
    }
}
