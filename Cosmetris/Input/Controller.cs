/*
 * Controller.cs is part of Cosmetris.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Cosmetris.Render;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cosmetris.Input;

public class Controller
{
    // Private instance of the class.
    private static Controller _instance;

    private readonly List<ControllerButton> _buttons = new();

    private GamePadState _gamePadState;

    private KeyboardState _keyboardState;
    private GamePadState _lastGamePadState;

    private KeyboardState _lastKeyboardState;

    // Called while button is continually being pressed
    public EventHandler<ControllerButton> OnButtonDown;

    // Called when button is pressed and released
    public EventHandler<ControllerButton> OnButtonPress;

    // Called when button is released
    public EventHandler<ControllerButton> OnButtonRelease;

    private Controller()
    {
        _keyboardState = Keyboard.GetState();
        _lastKeyboardState = _keyboardState;

        _gamePadState = GamePad.GetState(PlayerIndex.One);
        _lastGamePadState = _gamePadState;

        Up = new ControllerButton("Up", new[] { Keys.W, Keys.Up }, new[] { Buttons.LeftThumbstickUp, Buttons.DPadUp },
            165f, 15f, 150f);
        Down = new ControllerButton("Down", new[] { Keys.S, Keys.Down },
            new[] { Buttons.LeftThumbstickDown, Buttons.DPadDown }, 165f, 35f, 60f);
        Left = new ControllerButton("Left", new[] { Keys.A, Keys.Left },
            new[] { Buttons.LeftThumbstickLeft, Buttons.DPadLeft }, 165f, 35f, 85f);
        Right = new ControllerButton("Right", new[] { Keys.D, Keys.Right },
            new[] { Buttons.LeftThumbstickRight, Buttons.DPadRight }, 165f, 35f, 85f);
        RotateRight = new ControllerButton("Rotate Right", Keys.X, Buttons.B, 200f, 95f, 185f);
        RotateLeft = new ControllerButton("Rotate Left", Keys.Z, Buttons.A, 200f, 95f, 185f);
        HardDrop = new ControllerButton("Hard Drop", Keys.Space, Buttons.RightShoulder, 200f, 85f, 150f);
        Hold = new ControllerButton("Hold", Keys.LeftShift, Buttons.LeftShoulder, 200f, 0f, 200f);
        Pause = new ControllerButton("Pause", Keys.Escape, Buttons.Start, 200f, 0f, 200f);

        _buttons.AddRange(new[]
        {
            Up, Down, Left, Right, RotateRight, RotateLeft, HardDrop, Hold, Pause
        });
    }

    // A raw event that will pass any button press
    public EventHandler<BindableKeyboardKey> RawKeyboardKeyReleased { get; set; }
    public EventHandler<BindableDebugKeys> DebugKeyReleased { get; set; }
    public EventHandler<BindableControllerKey> RawControllerButtonReleased { get; set; }

    public static Controller Instance => _instance ??= new Controller();

    public ControllerButton Up { get; }
    public ControllerButton Down { get; }
    public ControllerButton Left { get; }
    public ControllerButton Right { get; }
    public ControllerButton RotateRight { get; }
    public ControllerButton RotateLeft { get; }
    public ControllerButton HardDrop { get; }
    public ControllerButton Hold { get; }
    public ControllerButton Pause { get; }

    public float LeftStickSensitivity { get; set; } = 0.5f;
    public float RightStickSensitivity { get; set; } = 0.5f;

    public void LoadSettings()
    {
        foreach (var button in _buttons)
        {
            button.KeyboardKeys = GameSettings.Instance.GetValue<ControllerButton>("Input", button.Name).KeyboardKeys;
            button.GamePadButtons =
                GameSettings.Instance.GetValue<ControllerButton>("Input", button.Name).GamePadButtons;
            button.Delay.SetDelay(GameSettings.Instance.GetValue<ButtonDelay>("Input Delay", button.Name).Delay);
            button.Delay.SetDelayReductionRate(
                GameSettings.Instance.GetValue<ButtonDelay>("Input Delay", button.Name).DelayReductionRate);
            button.Delay.SetMinDelay(GameSettings.Instance.GetValue<ButtonDelay>("Input Delay", button.Name).MinDelay);
        }

        LeftStickSensitivity = GameSettings.Instance.GetValue<float>("Input", "Left Stick Sensitivity");
        RightStickSensitivity = GameSettings.Instance.GetValue<float>("Input", "Right Stick Sensitivity");
    }

    public GameOptionCategory GetCategory()
    {
        return new GameOptionCategory("Inputs");
    }

    public List<ControllerButton> GetButtons()
    {
        return _buttons;
    }

    public void Update(GameTime gameTime)
    {
        _lastKeyboardState = _keyboardState;
        _keyboardState = Keyboard.GetState();

        _lastGamePadState = _gamePadState;
        _gamePadState = GamePad.GetState(PlayerIndex.One);

        foreach (var button in _buttons)
        {
            var isPressed = IsPressed(button);
            button.Update(isPressed, this);

            if (button.IsTriggered)
                OnButtonPress?.Invoke(this, button);
            if (button.IsReleased)
            {
                button.Delay.Reset();
                button.Delay.ResetDelay();
                OnButtonRelease?.Invoke(this, button);
            }

            if (button.IsDown && button.JustPressed)
                OnButtonDown?.Invoke(this, button);
        }

        foreach (var button in BindableKeys.KeyboardKeys)
            if (_lastKeyboardState.IsKeyDown(button.Key) && _keyboardState.IsKeyUp(button.Key))
                RawKeyboardKeyReleased?.Invoke(this, button);
        
        foreach (var button in BindableKeys.DebugKeys)
            if (_lastKeyboardState.IsKeyDown(button.Key) && _keyboardState.IsKeyUp(button.Key))
                DebugKeyReleased?.Invoke(this, button);

        foreach (var button in BindableKeys.ControllerKeys)
            if (_lastGamePadState.IsButtonDown(button.Button) && _gamePadState.IsButtonUp(button.Button))
            {
                if (button.Button == Buttons.RightTrigger && _gamePadState.Triggers.Right < 0.85f)
                    continue;

                if (button.Button == Buttons.LeftTrigger && _gamePadState.Triggers.Left < 0.85f)
                    continue;

                RawControllerButtonReleased?.Invoke(this, button);
            }
    }

    private bool IsPressed(ControllerButton button)
    {
        if (!_buttons.Contains(button))
            _buttons.Add(button);

        foreach (var key in button.KeyboardKeys)
            if (_keyboardState.IsKeyDown(key))
                return true;

        foreach (var gamePadButton in button.GamePadButtons)
            if (_gamePadState.IsButtonDown(gamePadButton))
                return true;

        return false;
    }

    public void ResetState()
    {
        foreach (var button in _buttons) button.Reset();
    }

    public void ClearEvents()
    {
        OnButtonDown = null;
        OnButtonRelease = null;
        OnButtonPress = null;
    }

    public void SetKeyboardKey(Keys lastKeyboardKey, Keys buttonKey)
    {
        // Find the button that contains the last keyboard key
        var button = _buttons.Find(b => b.KeyboardKeys.Contains(lastKeyboardKey));
        if (button == null) return;

        button.SetKeyboardKey(lastKeyboardKey, buttonKey);
    }

    public void SetControllerButton(Buttons lastControllerButton, Buttons buttonKey)
    {
        // Find the button that contains the last controller button
        var button = _buttons.Find(b => b.GamePadButtons.Contains(lastControllerButton));
        if (button == null) return;

        button.SetGamePadButton(lastControllerButton, buttonKey);
    }

    public bool IsBound(Buttons buttonButton)
    {
        return _buttons.Any(b => b.GamePadButtons.Contains(buttonButton));
    }

    public bool IsBound(Keys keyboardKey)
    {
        return _buttons.Any(b => b.KeyboardKeys.Contains(keyboardKey));
    }

    public class ControllerButton
    {
        public ControllerButton()
        {
        }

        public ControllerButton(string name, Keys keyboardKey, Buttons gamePadButton, float buttonDelay = 2f,
            float delayReductionRate = 0f, float minDelay = 0f)
        {
            Name = name;
            KeyboardKeys.Add(keyboardKey);
            GamePadButtons.Add(gamePadButton);
            Delay = new ButtonDelay(name, buttonDelay, delayReductionRate, minDelay);
        }

        public ControllerButton(string name, Keys[] keyboardKey, Buttons[] gamePadButton, float buttonDelay = 2f,
            float delayReductionRate = 0f, float minDelay = 0f)
        {
            Name = name;
            KeyboardKeys.AddRange(keyboardKey);
            GamePadButtons.AddRange(gamePadButton);
            Delay = new ButtonDelay(name, buttonDelay, delayReductionRate, minDelay);
        }

        public ControllerButton(string name, Keys keyboardKey, Buttons[] gamePadButton, float buttonDelay = 2f,
            float delayReductionRate = 0f, float minDelay = 0f)
        {
            Name = name;
            KeyboardKeys.Add(keyboardKey);
            GamePadButtons.AddRange(gamePadButton);
            Delay = new ButtonDelay(name, buttonDelay, delayReductionRate, minDelay);
        }

        public ControllerButton(string name, Keys[] keyboardKey, Buttons gamePadButton, float buttonDelay = 2f,
            float delayReductionRate = 0f, float minDelay = 0f)
        {
            Name = name;
            KeyboardKeys.AddRange(keyboardKey);
            GamePadButtons.Add(gamePadButton);
            Delay = new ButtonDelay(name, buttonDelay, delayReductionRate, minDelay);
        }

        [XmlIgnore] public bool IsTriggered { get; private set; }

        [XmlIgnore] public bool IsReleased { get; private set; }

        [XmlIgnore] public bool IsDown { get; private set; }

        [XmlIgnore] public bool IsUp { get; private set; }

        [XmlIgnore] public ButtonDelay Delay { get; set; }

        [XmlIgnore] public bool JustPressed { get; private set; }

        public string Name { get; }
        public List<Keys> KeyboardKeys { get; set; } = new();
        public List<Buttons> GamePadButtons { get; set; } = new();

        public void Update(bool isPressed, Controller controller)
        {
            if (isPressed)
            {
                if (IsUp)
                {
                    JustPressed = true;
                    IsTriggered = true;
                    IsReleased = false;
                    IsDown = true;
                    IsUp = false;
                    Delay.Reset(); // Reset the delay when the button is initially pressed
                }
                else
                {
                    IsTriggered = false;
                    IsReleased = false;
                    IsDown = true;
                    IsUp = false;

                    if (JustPressed) JustPressed = false;

                    // Check if the delay is ready, and if so, reset the delay and invoke OnButtonDown
                    if (Delay.IsReady)
                    {
                        Delay.Reset();
                        controller.OnButtonDown?.Invoke(controller, this);
                    }
                }

                // Apply the delay reduction while the button is being held
                Delay.ApplyDelayReduction((float)Window.Instance.GetGameTime().ElapsedGameTime.TotalMilliseconds);
            }
            else
            {
                if (IsDown)
                {
                    // Reset the delay when the button is released
                    Delay.Reset();
                    IsReleased = true;
                }
                else
                {
                    IsReleased = false;
                }

                IsTriggered = false;
                IsDown = false;
                IsUp = true;

                // Reset the delay when the button is released
                Delay.ResetDelay();
            }

            Delay.Update(Window.Instance.GetGameTime());
        }

        public void SetKeyboardKey(Keys oldKey, Keys newKey)
        {
            if (!KeyboardKeys.Contains(oldKey)) return;

            KeyboardKeys.Remove(oldKey);
            KeyboardKeys.Add(newKey);
        }

        public void SetGamePadButton(Buttons oldButton, Buttons newButton)
        {
            if (!GamePadButtons.Contains(oldButton)) return;

            GamePadButtons.Remove(oldButton);
            GamePadButtons.Add(newButton);
        }

        public void Reset()
        {
            JustPressed = false;
            IsTriggered = false;
            IsReleased = false;
            IsDown = false;
            IsUp = false;
        }
    }

    public class ButtonDelay
    {
        public ButtonDelay()
        {
        }

        public ButtonDelay(string name, float delay, float delayReductionRate = 0f, float minDelay = 0f)
        {
            Name = name;
            InitialDelay = delay;
            Delay = delay;
            DelayReductionRate = delayReductionRate;
            IsReady = false;
            ElapsedTime = 0;
            MinDelay = minDelay;
        }

        [XmlIgnore] public float ElapsedTime { get; private set; }

        [XmlIgnore] public bool IsReady { get; private set; }

        [XmlIgnore] public float InitialDelay { get; }

        public float Delay { get; set; }

        public float DelayReductionRate { get; set; }
        public float MinDelay { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime)
        {
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ElapsedTime >= Delay && DelayReductionRate > 0f)
            {
                IsReady = true;
                ElapsedTime = 0;
                Delay = Math.Max(0f, Delay - DelayReductionRate * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            else
            {
                IsReady = false;
            }
        }

        public void Reset()
        {
            IsReady = false;
            ElapsedTime = 0;
        }

        public void ResetDelay()
        {
            Delay = InitialDelay;
        }

        public void SetDelay(float delay)
        {
            Delay = delay;
        }

        public void ApplyDelayReduction(float elapsedMilliseconds)
        {
            var elapsedSeconds = elapsedMilliseconds / 1000f;
            var reductionAmount = DelayReductionRate * elapsedSeconds;
            Delay = Math.Max(MinDelay, Delay - reductionAmount); // Ensure the delay does not go below MinDelay
        }

        public void SetDelayReductionRate(float sliderValue)
        {
            DelayReductionRate = sliderValue;
        }

        public void SetMinDelay(float sliderValue)
        {
            MinDelay = sliderValue;
        }
    }
}