/*
 * BindableKeys.cs is part of Cosmetris.
 *
 * Copyright (c) 2023 CKProductions, https://ckproductions.dev/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Cosmetris.Settings;

public class BindableKeys
{
    public static List<BindableKeyboardKey> KeyboardKeys = new()
    {
        new BindableKeyboardKey { Name = "A", Key = Keys.A },
        new BindableKeyboardKey { Name = "B", Key = Keys.B },
        new BindableKeyboardKey { Name = "C", Key = Keys.C },
        new BindableKeyboardKey { Name = "D", Key = Keys.D },
        new BindableKeyboardKey { Name = "E", Key = Keys.E },
        new BindableKeyboardKey { Name = "F", Key = Keys.F },
        new BindableKeyboardKey { Name = "G", Key = Keys.G },
        new BindableKeyboardKey { Name = "H", Key = Keys.H },
        new BindableKeyboardKey { Name = "I", Key = Keys.I },
        new BindableKeyboardKey { Name = "J", Key = Keys.J },
        new BindableKeyboardKey { Name = "K", Key = Keys.K },
        new BindableKeyboardKey { Name = "L", Key = Keys.L },
        new BindableKeyboardKey { Name = "M", Key = Keys.M },
        new BindableKeyboardKey { Name = "N", Key = Keys.N },
        new BindableKeyboardKey { Name = "O", Key = Keys.O },
        new BindableKeyboardKey { Name = "P", Key = Keys.P },
        new BindableKeyboardKey { Name = "Q", Key = Keys.Q },
        new BindableKeyboardKey { Name = "R", Key = Keys.R },
        new BindableKeyboardKey { Name = "S", Key = Keys.S },
        new BindableKeyboardKey { Name = "T", Key = Keys.T },
        new BindableKeyboardKey { Name = "U", Key = Keys.U },
        new BindableKeyboardKey { Name = "V", Key = Keys.V },
        new BindableKeyboardKey { Name = "W", Key = Keys.W },
        new BindableKeyboardKey { Name = "X", Key = Keys.X },
        new BindableKeyboardKey { Name = "Y", Key = Keys.Y },
        new BindableKeyboardKey { Name = "Z", Key = Keys.Z },

        new BindableKeyboardKey { Name = "0", Key = Keys.D0 },
        new BindableKeyboardKey { Name = "1", Key = Keys.D1 },
        new BindableKeyboardKey { Name = "2", Key = Keys.D2 },
        new BindableKeyboardKey { Name = "3", Key = Keys.D3 },
        new BindableKeyboardKey { Name = "4", Key = Keys.D4 },
        new BindableKeyboardKey { Name = "5", Key = Keys.D5 },
        new BindableKeyboardKey { Name = "6", Key = Keys.D6 },
        new BindableKeyboardKey { Name = "7", Key = Keys.D7 },
        new BindableKeyboardKey { Name = "8", Key = Keys.D8 },
        new BindableKeyboardKey { Name = "9", Key = Keys.D9 },

        new BindableKeyboardKey { Name = "NumPad0", Key = Keys.NumPad0 },
        new BindableKeyboardKey { Name = "NumPad1", Key = Keys.NumPad1 },
        new BindableKeyboardKey { Name = "NumPad2", Key = Keys.NumPad2 },
        new BindableKeyboardKey { Name = "NumPad3", Key = Keys.NumPad3 },
        new BindableKeyboardKey { Name = "NumPad4", Key = Keys.NumPad4 },
        new BindableKeyboardKey { Name = "NumPad5", Key = Keys.NumPad5 },
        new BindableKeyboardKey { Name = "NumPad6", Key = Keys.NumPad6 },
        new BindableKeyboardKey { Name = "NumPad7", Key = Keys.NumPad7 },
        new BindableKeyboardKey { Name = "NumPad8", Key = Keys.NumPad8 },
        new BindableKeyboardKey { Name = "NumPad9", Key = Keys.NumPad9 },

        new BindableKeyboardKey { Name = "LeftShift", Key = Keys.LeftShift },
        new BindableKeyboardKey { Name = "RightShift", Key = Keys.RightShift },
        new BindableKeyboardKey { Name = "LeftControl", Key = Keys.LeftControl },
        new BindableKeyboardKey { Name = "RightControl", Key = Keys.RightControl },
        new BindableKeyboardKey { Name = "LeftAlt", Key = Keys.LeftAlt },
        new BindableKeyboardKey { Name = "RightAlt", Key = Keys.RightAlt },

        new BindableKeyboardKey { Name = "Space", Key = Keys.Space },
        new BindableKeyboardKey { Name = "Enter", Key = Keys.Enter },
        new BindableKeyboardKey { Name = "Escape", Key = Keys.Escape },
        new BindableKeyboardKey { Name = "Tab", Key = Keys.Tab },
        new BindableKeyboardKey { Name = "Backspace", Key = Keys.Back },
        new BindableKeyboardKey { Name = "Delete", Key = Keys.Delete },
        new BindableKeyboardKey { Name = "Insert", Key = Keys.Insert },
        new BindableKeyboardKey { Name = "Home", Key = Keys.Home },
        new BindableKeyboardKey { Name = "End", Key = Keys.End },
        new BindableKeyboardKey { Name = "PageUp", Key = Keys.PageUp },
        new BindableKeyboardKey { Name = "PageDown", Key = Keys.PageDown },

        new BindableKeyboardKey { Name = "Up", Key = Keys.Up },
        new BindableKeyboardKey { Name = "Down", Key = Keys.Down },
        new BindableKeyboardKey { Name = "Left", Key = Keys.Left },
        new BindableKeyboardKey { Name = "Right", Key = Keys.Right }
    };

    public static List<BindableControllerKey> ControllerKeys = new()
    {
        new BindableControllerKey { Name = "A", Button = Buttons.A },
        new BindableControllerKey { Name = "B", Button = Buttons.B },
        new BindableControllerKey { Name = "X", Button = Buttons.X },
        new BindableControllerKey { Name = "Y", Button = Buttons.Y },
        new BindableControllerKey { Name = "Back", Button = Buttons.Back },
        new BindableControllerKey { Name = "Start", Button = Buttons.Start },
        new BindableControllerKey { Name = "LeftShoulder", Button = Buttons.LeftShoulder },
        new BindableControllerKey { Name = "RightShoulder", Button = Buttons.RightShoulder },
        new BindableControllerKey { Name = "LeftStick", Button = Buttons.LeftStick },
        new BindableControllerKey { Name = "RightStick", Button = Buttons.RightStick },
        new BindableControllerKey { Name = "DPadUp", Button = Buttons.DPadUp },
        new BindableControllerKey { Name = "DPadDown", Button = Buttons.DPadDown },
        new BindableControllerKey { Name = "DPadLeft", Button = Buttons.DPadLeft },
        new BindableControllerKey { Name = "DPadRight", Button = Buttons.DPadRight },
        new BindableControllerKey { Name = "LeftTrigger", Button = Buttons.LeftTrigger },
        new BindableControllerKey { Name = "RightTrigger", Button = Buttons.RightTrigger },
        new BindableControllerKey { Name = "LeftThumbstickLeft", Button = Buttons.LeftThumbstickLeft },
        new BindableControllerKey { Name = "LeftThumbstickRight", Button = Buttons.LeftThumbstickRight },
        new BindableControllerKey { Name = "LeftThumbstickUp", Button = Buttons.LeftThumbstickUp },
        new BindableControllerKey { Name = "LeftThumbstickDown", Button = Buttons.LeftThumbstickDown },
        new BindableControllerKey { Name = "RightThumbstickLeft", Button = Buttons.RightThumbstickLeft },
        new BindableControllerKey { Name = "RightThumbstickRight", Button = Buttons.RightThumbstickRight },
        new BindableControllerKey { Name = "RightThumbstickUp", Button = Buttons.RightThumbstickUp },
        new BindableControllerKey { Name = "RightThumbstickDown", Button = Buttons.RightThumbstickDown }
    };
    
    public static List<BindableDebugKeys> DebugKeys = new()
    {
        new BindableDebugKeys { Name = "F1", Key = Keys.F1 },
        new BindableDebugKeys { Name = "F2", Key = Keys.F2 },
        new BindableDebugKeys { Name = "F3", Key = Keys.F3 },
        new BindableDebugKeys { Name = "F4", Key = Keys.F4 },
        new BindableDebugKeys { Name = "F5", Key = Keys.F5 },
        new BindableDebugKeys { Name = "F6", Key = Keys.F6 },
        new BindableDebugKeys { Name = "F7", Key = Keys.F7 },
        new BindableDebugKeys { Name = "F8", Key = Keys.F8 },
        new BindableDebugKeys { Name = "F9", Key = Keys.F9 },
        new BindableDebugKeys { Name = "F10", Key = Keys.F10 },
    };

    static BindableKeys()
    {
    }
}

public class BindableKeyboardKey
{
    public string Name { get; set; }
    public Keys Key { get; set; }
}

public class BindableControllerKey
{
    public string Name { get; set; }
    public Buttons Button { get; set; }
}

public class BindableDebugKeys 
{
    public string Name { get; set; }
    public Keys Key { get; set; }
}