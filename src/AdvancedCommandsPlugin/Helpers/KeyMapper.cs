namespace Loupedeck.AdvancedCommandsPlugin.Helpers
{
    using System;
    using System.Collections.Generic;

    public static class KeyMapper
    {
        public static List<WindowsInput.Native.VirtualKeyCode> MapModifiers(String key, Boolean useRightModifiers = false)
        {
            var keyBind = key.Split("___")[0];
            KeyboardExtensions.TryParseKeyboardKey(keyBind.Split("___")[0], out var keyboardKey);

            var modifierKeys = keyboardKey.ModifierKey.ToString().Split(",");            

            var modifierKeysList = new List<WindowsInput.Native.VirtualKeyCode>();

            foreach (var modifier in modifierKeys)
            {
                ModifierKey modifierKey = KeyboardExtensions.GetModifierKey(modifier.Trim());
                switch (modifierKey)
                {
                    case ModifierKey.Control:
                    case ModifierKey.ControlOrCommand:
                        if (useRightModifiers)
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.RCONTROL);
                        } else
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.LCONTROL);
                        }

                        break;
                    case ModifierKey.Alt:
                        if (useRightModifiers)
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.RMENU);
                        }
                        else
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.LMENU);
                        }

                        break;
                    case ModifierKey.Shift:
                        if (useRightModifiers)
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.RSHIFT);
                        }
                        else
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.LSHIFT);
                        }

                        break;
                    case ModifierKey.Windows:
                        if (useRightModifiers)
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.RWIN);
                        }
                        else
                        {
                            modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.LWIN);
                        }

                        break;
                    case ModifierKey.CapsLock:
                        modifierKeysList.Add(WindowsInput.Native.VirtualKeyCode.CAPITAL);
                        break;
                }
            }

            return modifierKeysList;
        }

        public static WindowsInput.Native.VirtualKeyCode MapKeys(String key)
        {
            var keyBind = key.Split("___")[0];
            KeyboardExtensions.TryParseKeyboardKey(keyBind.Split("___")[0], out var keyboardKey);

            return (WindowsInput.Native.VirtualKeyCode)keyboardKey.VirtualKeyCode;
        }
    }
}
