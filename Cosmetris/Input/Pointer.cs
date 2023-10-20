/*
 * Pointer.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Cosmetris.Input;

/// <summary>
///     This class will translate mouse/gamepad input into a pointer.
///     If using mouse this really does not make much a difference, but is used so
///     when using a gamepad we can share the same code.
/// </summary>
public class Pointer
{
    // Cursor will be 8x8 pixels
    private readonly int _size = 24;

    private readonly UIScalingManager _uiScalingManager = Window.Instance.ScalingManager;

    private GamePadState _gamePadState;

    private MouseState _mouseState;

    private Vector2 _pointerPosition;

    private Texture2D _pointerTexture;
    private GamePadState _previousGamePadState;
    private MouseState _previousMouseState;
    private Vector2 _untranslatedPosition;
    private bool isUsingController;

    public EventHandler OnPrimaryClick;
    public EventHandler OnPrimaryClickRelease;
    public EventHandler OnSecondaryClick;

    private bool wasUsingController;

    public void ContentLoad()
    {
        TextureManager.Instance.AddTexture("cursor", "Input/cursor");
        _pointerTexture = TextureManager.Instance.GetTexture2D("cursor");
    }

    public void ClearEvents()
    {
        OnPrimaryClick = null;
        OnPrimaryClickRelease = null;
        OnSecondaryClick = null;
    }

    public Vector2 GetPointerPosition()
    {
        return _pointerPosition;
    }

    public void Update(GameTime gameTime)
    {
        Cosmetris.Instance.IsMouseVisible = !Cosmetris.Instance.IsActive;

        if (!Cosmetris.Instance.IsActive) return;

        _gamePadState = GamePad.GetState(PlayerIndex.One);

        // If the player is trying to use the gamepad, switch control method
        if (_gamePadState.IsConnected)
        {
            if (_gamePadState.Buttons.A == ButtonState.Pressed && !isUsingController && !wasUsingController)
                isUsingController = true;
        }
        else
        {
            isUsingController = false;
        }

        if (isUsingController)
        {
            if (!wasUsingController)
            {
                // Set position to center of screen
                _pointerPosition = new Vector2(_uiScalingManager.DesiredWidth / 2.0f,
                    _uiScalingManager.DesiredHeight / 2.0f);
                wasUsingController = true;
            }

            if (_gamePadState.ThumbSticks.Right != Vector2.Zero)
            {
                var sensitivity = GetControllerSensitivity() * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                _untranslatedPosition = _gamePadState.ThumbSticks.Right * sensitivity;

                var gamePadPos = TranslatePosition(_untranslatedPosition);

                // Invert the Y axis
                gamePadPos.Y = -gamePadPos.Y;

                _pointerPosition += gamePadPos;

                // Clamp the pointer to the screen
                _pointerPosition.X = MathHelper.Clamp(_pointerPosition.X, 0, _uiScalingManager.DesiredWidth);
                _pointerPosition.Y = MathHelper.Clamp(_pointerPosition.Y, 0, _uiScalingManager.DesiredHeight);
            }

            if (_gamePadState.Triggers.Right > 0.5f && _previousGamePadState.Triggers.Right < 0.5f)
                OnPrimaryClick?.Invoke(this, EventArgs.Empty);

            // Check if trigger is released
            if (_gamePadState.Triggers.Right < 0.5f && _previousGamePadState.Triggers.Right > 0.5f)
                OnPrimaryClickRelease?.Invoke(this, EventArgs.Empty);

            if (_gamePadState.Triggers.Left > 0.5f && _previousGamePadState.Triggers.Left < 0.5f)
                OnSecondaryClick?.Invoke(this, EventArgs.Empty);

            _previousGamePadState = _gamePadState;
        }

        _mouseState = Mouse.GetState();

        // If the player is trying to use the mouse, switch control method
        if (isUsingController)
        {
            var shouldBreak = false;

            if (_mouseState.LeftButton == ButtonState.Pressed)
            {
                isUsingController = false;
                wasUsingController = false;
                shouldBreak = true;

                // Set mouse position to the pointer position
                Mouse.SetPosition((int)_pointerPosition.X, (int)_pointerPosition.Y);
            }

            if (!shouldBreak)
            {
                _previousMouseState = _mouseState;
                return;
            }
        }

        // Need to translate the Mouse position to the buffer size
        _untranslatedPosition = _mouseState.Position.ToVector2();
        var mousePos = TranslatePosition(_untranslatedPosition);

        _pointerPosition = new Vector2(mousePos.X, mousePos.Y);

        if (_mouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed)
            OnPrimaryClickRelease?.Invoke(this, EventArgs.Empty);
        else if (_mouseState.LeftButton == ButtonState.Pressed &&
                 _previousMouseState.LeftButton == ButtonState.Released)
            OnPrimaryClick?.Invoke(this, EventArgs.Empty);

        if (_mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
            OnSecondaryClick?.Invoke(this, EventArgs.Empty);

        _previousMouseState = _mouseState;
    }

    public int GetX()
    {
        return Window.Instance.ScalingManager.GetScaledX((int)_pointerPosition.X);
    }

    public int GetY()
    {
        return Window.Instance.ScalingManager.GetScaledY((int)_pointerPosition.Y);
    }

    public void UpdateCursorTexture()
    {
        _pointerTexture = TextureManager.Instance.GetTexture2D("cursor");
    }

    private float GetControllerSensitivity()
    {
        return GameSettings.Instance.GetValue<float>("Input", "Right Stick Sensitivity") * 2.5f;
    }

    /// <summary>
    ///     Translates the cursor position of the screen to the virtual resolution of the game
    /// </summary>
    private Vector2 TranslatePosition(Vector2 state)
    {
        var virtualX = Convert.ToSingle(state.X) * Convert.ToSingle(_uiScalingManager.DesiredWidth) /
                       Convert.ToSingle(_uiScalingManager.ActualWidth);
        var virtualY = Convert.ToSingle(state.Y) * Convert.ToSingle(_uiScalingManager.DesiredHeight) /
                       Convert.ToSingle(_uiScalingManager.ActualHeight);

        var mousePos = new Vector2(virtualX, virtualY);

        return mousePos;
    }

    private void SetPosition(Vector2 position)
    {
        _untranslatedPosition = position;
        _pointerPosition = position;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw the pointer
        spriteBatch.Draw(_pointerTexture, new Rectangle(GetX(), GetY(), _size, _size), Color.White);
    }

    public bool IsPrimaryClickPressed()
    {
        return _mouseState.LeftButton == ButtonState.Pressed || _gamePadState.Triggers.Right > 0.5f;
    }

    public bool IsPrimaryClickReleased()
    {
        return _mouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed;
    }
}