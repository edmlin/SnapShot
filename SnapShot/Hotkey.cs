using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SnapShot
{
    public class Hotkey
    {
        public bool SettingHotkey=false;
        public string HotkeyString { get { return string.Join("+", Hotkeys); } }
        private GlobalKeyboardHook _globalKeyboardHook;
        public Hotkey()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }
        public void SetHotkey(string keyString)
        {
            var keys =keyString.Split('+').Select(k => (Key)Enum.Parse(typeof(Key), k));
            SetHotkey(keys);
        }
        public void SetHotkey(IEnumerable<Key>keys)
        {
            Hotkeys.Clear();
            Hotkeys.AddRange(keys);
            if (OnHotkeySet != null)
            {
                OnHotkeySet(this, null);
            }
            Debug.WriteLine("Hotkey set to: " + string.Join("+", Hotkeys));
        }
        List<Key> DownKeys = new List<Key>();
        List<Key> Hotkeys = new List<Key>();
        public event EventHandler OnHotkey;
        public event EventHandler OnHotkeySet;
        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            // EDT: No need to filter for VkSnapshot anymore. This now gets handled
            // through the constructor of GlobalKeyboardHook(...).

            Key key = e.KeyboardData.Key;
            int vkCode = e.KeyboardData.VirtualCode;
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown || e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown)
            {
                // Now you can access both, the key and virtual code
                if (!DownKeys.Contains(key)) DownKeys.Add(key);
                if (SettingHotkey)
                {
                    SetHotkey(DownKeys);
                    e.Handled = true;
                }
                else
                {
                    Debug.WriteLine("Pressed keys: " + string.Join("+", DownKeys));
                    if (Hotkeys.Except(DownKeys).Count() == 0 && DownKeys.Except(Hotkeys).Count() == 0)
                    {
                        if (OnHotkey != null)
                        {
                            OnHotkey(this, null);
                        }
                    }
                }
            }
            else
            {
                if (DownKeys.Contains(key)) DownKeys.Remove(key);
            }
        }
    }
}
