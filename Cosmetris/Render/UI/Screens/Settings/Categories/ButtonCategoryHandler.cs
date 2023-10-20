/*
 * ButtonCategoryHandler.cs is part of Cosmetris.
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
using System.Linq;
using Cosmetris.Input;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Screens.Settings.Categories;

public class ButtonCategoryHandler : ICategoryHandler
{
    public int Size => 1;
    public bool OptionWasChanged { get; set; }
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);
    
    private SettingsScreen _screen;
    
    private string _lastButtonName;
    private BindableKeyboardKey _lastKeyboardKey;
    private BindableControllerKey _lastControllerButton;
    private Button _waitingForButton;
    private bool _waitingForControllerInput;
    private bool _waitingForKeyboardInput;
    private float _waitTimer;

    public void Initialize()
    {
        Controller.Instance.RawKeyboardKeyReleased += HandleKeyboardBind;
        Controller.Instance.RawControllerButtonReleased += HandleControllerBind;
        // Need a Update event so we can update the wait timer.
        Window.Instance.UpdateEvent += OnUpdate;
    }

    public void Handle(SettingsScreen screen, GameOptionBase option, float xOffset, float yOffset, int currentPage,
        SettingsCategory currentCategory)
    {
        _screen ??= screen;
        var controllerButtonOption = (GameOption<Controller.ControllerButton>)option;
        // Create a head label for the controller button option
        var label = new Label(controllerButtonOption.Name, new Vector2(0, yOffset), _defaultFont,
            Microsoft.Xna.Framework.Color.White, Label.Align.Center);
        label.Position = new Vector2(xOffset, label.Position.Y);

        yOffset += (int)label.Size.Y + 2;

        screen.AddControlToPage(currentCategory, label, currentPage);

        var curKeyboardButtonOffset = 0;
        var curControllerButtonOffset = 0;

        foreach (var keyboardKey in controllerButtonOption.Value.KeyboardKeys)
        {
            var button = new Button(keyboardKey.ToString(), xOffset - label.Size.X - 80, yOffset,
                (sender, args) =>
                {
                    _waitingForKeyboardInput = true;
                    _lastButtonName = keyboardKey.ToString();
                    _lastKeyboardKey =
                        BindableKeys.KeyboardKeys.FirstOrDefault(k => k.Key == keyboardKey);
                    _waitingForButton = (Button)sender;
                    _waitingForButton?.SetText("{blue}Press a key...");
                    _waitTimer = 0;

                    _screen.DisableControls();
                    //_optionsChanged = true;
                }, _defaultFont);

            button.Position = new Vector2(button.Position.X,
                button.Position.Y + curKeyboardButtonOffset);

            screen.AddControlToPage(currentCategory, button, currentPage);

            curKeyboardButtonOffset += (int)button.Size.Y + 2;
        }

        // The controller button counterparts will be to the right, but same y pos
        foreach (var controllerKey in controllerButtonOption.Value.GamePadButtons)
        {
            var button = new Button(controllerKey.ToString(), xOffset + label.Size.X + 80, yOffset,
                (sender, args) =>
                {
                    _waitingForControllerInput = true;
                    _lastButtonName = controllerKey.ToString();
                    _lastControllerButton =
                        BindableKeys.ControllerKeys.FirstOrDefault(k => k.Button == controllerKey);
                    _waitingForButton = (Button)sender;
                    _waitingForButton?.SetText("{blue}Press a button...");
                    _waitTimer = 0;
                    _screen.DisableControls();
                    //_optionsChanged = true;
                }, _defaultFont);

            button.Position = new Vector2(button.Position.X,
                button.Position.Y + curControllerButtonOffset);

            screen.AddControlToPage(currentCategory, button, currentPage);

            curControllerButtonOffset += (int)button.Size.Y + 2;
        }
    }
    
    private void OnUpdate(object sender, GameTime gameTime)
    {
        if(_screen == null) return;
        
        if (_waitingForControllerInput || _waitingForKeyboardInput)
        {
            _waitTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_waitTimer >= 5000)
            {
                if (_waitingForKeyboardInput)
                {
                    _waitingForButton.SetText(_lastButtonName);
                    _waitingForButton = null;
                    _waitingForKeyboardInput = false;
                    _waitingForControllerInput = false;
                    _lastKeyboardKey = null;
                }
                else if (_waitingForControllerInput)
                {
                    _waitingForButton.SetText(_lastButtonName);
                    _waitingForButton = null;
                    _waitingForControllerInput = false;
                    _waitingForKeyboardInput = false;
                    _lastControllerButton = null;
                }

                _screen.EnableControls();
            }
        }
    }
    
    private void HandleKeyboardBind(object sender, BindableKeyboardKey button)
    {
        if (_screen == null) return;
        
        var alreadyBound = Controller.Instance.IsBound(button.Key);
        if (_waitingForKeyboardInput && !_waitingForControllerInput && !alreadyBound)
        {
            Controller.Instance.SetKeyboardKey(_lastKeyboardKey.Key, button.Key);
            _waitingForButton.SetText(button.Name);
            _waitingForButton = null;
            _waitingForKeyboardInput = false;
            _lastKeyboardKey = null;
            OptionWasChanged = true;
            _screen.EnableControls();
        }
        else if (alreadyBound && _waitingForKeyboardInput)
        {
            _waitingForButton.SetText(_lastButtonName);
            _waitingForButton = null;
            _waitingForKeyboardInput = false;
            _lastKeyboardKey = null;
            _screen.EnableControls();
        }
    }

    private void HandleControllerBind(object sender, BindableControllerKey button)
    {
        if (_screen == null) return;
        
        // Make sure button is not already bound
        var alreadyBound = Controller.Instance.IsBound(button.Button);

        if (_waitingForControllerInput && !_waitingForKeyboardInput && !alreadyBound)
        {
            Controller.Instance.SetControllerButton(_lastControllerButton.Button, button.Button);
            _waitingForButton.SetText(button.Name);
            _waitingForButton = null;
            _waitingForControllerInput = false;
            _lastControllerButton = null;
            OptionWasChanged = true;
            _screen.EnableControls();
        }
        else if (alreadyBound && _waitingForControllerInput)
        {
            _waitingForButton.SetText(_lastButtonName);
            _waitingForButton = null;
            _waitingForControllerInput = false;
            _lastControllerButton = null;
            _screen.EnableControls();
        }
    }

    public void Dispose()
    {
        Controller.Instance.RawKeyboardKeyReleased -= HandleKeyboardBind;
        Controller.Instance.RawControllerButtonReleased -= HandleControllerBind;
        Window.Instance.UpdateEvent -= OnUpdate;
    }
}