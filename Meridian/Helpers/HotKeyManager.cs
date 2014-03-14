using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Meridian.Domain;
using Meridian.Services;
using Meridian.ViewModel;
using Application = System.Windows.Application;

namespace Meridian.Helpers
{
    public sealed class HotKey : IDisposable
    {
        public event Action<HotKey> HotKeyPressed;

        private readonly int _id;
        private bool _isKeyRegistered;
        readonly IntPtr _handle;

        public HotKey(ModifierKeys modifierKeys, Key key, Window window)
            : this(modifierKeys, key, new WindowInteropHelper(window))
        {

        }

        public HotKey(ModifierKeys modifierKeys, Key key, WindowInteropHelper window)
            : this(modifierKeys, key, window.Handle)
        {

        }

        public HotKey(ModifierKeys modifierKeys, Key key, IntPtr windowHandle)
        {
            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            _handle = windowHandle;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        ~HotKey()
        {
            Dispose();
        }

        public Key Key { get; private set; }

        public ModifierKeys KeyModifier { get; private set; }

        public void RegisterHotKey()
        {
            if (Key == Key.None)
                return;
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = NativeMethods.RegisterHotKey(_handle, _id, KeyModifier, KeyInterop.VirtualKeyFromKey(Key));
        }

        public void UnregisterHotKey()
        {
            _isKeyRegistered = !NativeMethods.UnregisterHotKey(_handle, _id);
        }

        public void Dispose()
        {
            if (_isKeyRegistered)
            {
                try
                {
                    ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
                }
                catch
                {

                }

                UnregisterHotKey();
            }
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == NativeMethods.WM_HOTKEY
                    && (int)(msg.wParam) == _id)
                {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }

        private void OnHotKeyPressed()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this);
        }
    }

    public class HotKeyManager : IDisposable
    {
        private readonly IntPtr handle;
        private List<HotKey> hotKeys;

        public HotKeyManager(IntPtr handle)
        {
            this.handle = handle;

            RegisterHotkey(ModifierKeys.None, Key.MediaPlayPause);
            RegisterHotkey(ModifierKeys.None, Key.MediaNextTrack);
            RegisterHotkey(ModifierKeys.None, Key.MediaPreviousTrack);
            RegisterHotkey(ModifierKeys.None, Key.MediaStop);
            RegisterHotkey(ModifierKeys.None, Key.Play);
            RegisterHotkey(ModifierKeys.None, Key.Pause);
            RegisterHotkey(ModifierKeys.None, Key.SelectMedia);
        }

        public void RegisterHotkey(ModifierKeys modifier, Key key)
        {
            if (key == Key.None)
                return;

            if (hotKeys == null)
                hotKeys = new List<HotKey>();
            var hotkey = new HotKey(modifier, key, handle);
            hotkey.HotKeyPressed += HotKeyPressed;

            hotKeys.Add(hotkey);
        }

        public void RegisterHotkey(ModifierKeys modifier, Key key, Action<HotKey> action)
        {
            if (key == Key.None)
                return;
            if (hotKeys == null)
                hotKeys = new List<HotKey>();
            var hotkey = new HotKey(modifier, key, handle);
            hotkey.HotKeyPressed += action;

            hotKeys.Add(hotkey);
        }

        public void UnregisterHotkey(ModifierKeys modifier, Key key)
        {
            if (key == Key.None)
                return;
            foreach (var hotKey in hotKeys)
            {
                if (hotKey.Key == key && hotKey.KeyModifier == modifier)
                {
                    hotKey.HotKeyPressed -= HotKeyPressed;
                    hotKey.Dispose();
                    hotKeys.Remove(hotKey);
                    break;
                }
            }
        }

        public bool IsRegistered(ModifierKeys modifier, Key key)
        {
            if (key == Key.None)
                return false;
            foreach (var hotKey in hotKeys)
            {
                if (hotKey.Key == key && hotKey.KeyModifier == modifier)
                {
                    return true;
                }
            }

            return false;
        }

        public void InitializeHotkeys()
        {
            RegisterHotkey(Settings.Instance.NextHotKeyModifier, Settings.Instance.NextHotKey, h => AudioService.SkipNext());
            RegisterHotkey(Settings.Instance.PrevHotKeyModifier, Settings.Instance.PrevHotKey, h => AudioService.Prev());
            RegisterHotkey(Settings.Instance.PlayPauseHotKeyModifier, Settings.Instance.PlayPauseHotKey, h =>
            {
                if (AudioService.IsPlaying)
                    AudioService.Pause();
                else
                    AudioService.Play();
            });

            RegisterHotkey(Settings.Instance.LikeDislikeHotKeyModifier, Settings.Instance.LikeDislikeHotKey, h =>
            {
                ViewModelLocator.Main.AddRemoveAudioCommand.Execute(ViewModelLocator.Main.CurrentAudio);
            });

            RegisterHotkey(Settings.Instance.ShuffleHotKeyModifier, Settings.Instance.ShuffleHotKey, h => ViewModelLocator.Main.Shuffle = !ViewModelLocator.Main.Shuffle);
            RegisterHotkey(Settings.Instance.RepeatHotKeyModifier, Settings.Instance.RepeatHotKey, h => ViewModelLocator.Main.Repeat = !ViewModelLocator.Main.Repeat);
            RegisterHotkey(Settings.Instance.IncreaseVolumeHotKeyModifier, Settings.Instance.IncreaseVolumeHotKey, h => ViewModelLocator.Main.Volume += 5);
            RegisterHotkey(Settings.Instance.DecreaseVolumeHotKeyModifier, Settings.Instance.DecreaseVolumeHotKey, h => ViewModelLocator.Main.Volume -= 5);

            RegisterHotkey(Settings.Instance.ShowHideHotKeyModifier, Settings.Instance.ShowHideHotKey, h =>
            {
                if (Application.Current.MainWindow.WindowState != WindowState.Minimized)
                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                else
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
            });

            RegisterHotkey(Settings.Instance.FastForwardHotKeyModifier, Settings.Instance.FastForwardHotKey, h => AudioService.FastForward(7));

            RegisterHotkey(Settings.Instance.RewindHotKeyModifier, Settings.Instance.RewindHotKey, h => AudioService.Rewind(7));
        }

        void HotKeyPressed(HotKey obj)
        {
            switch (obj.Key)
            {
                case Key.MediaPlayPause:
                    if (AudioService.IsPlaying)
                        AudioService.Pause();
                    else
                        AudioService.Play();
                    break;
                case Key.MediaNextTrack:
                    AudioService.SkipNext();
                    break;
                case Key.MediaPreviousTrack:
                    AudioService.Prev();
                    break;
                case Key.MediaStop:
                    AudioService.Stop();
                    break;
                case Key.SelectMedia:
                    if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                    else if (Application.Current.MainWindow.IsActive)
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                    else
                    {
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Activate();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            foreach (var hotKey in hotKeys)
            {
                hotKey.HotKeyPressed -= HotKeyPressed;
                hotKey.Dispose();
                break;
            }

            hotKeys.Clear();
        }
    }
}
