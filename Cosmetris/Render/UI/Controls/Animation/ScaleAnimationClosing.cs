/*
 * ScaleAnimationClosing.cs is part of Cosmetris.
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object = Cosmetris.Game.Objects.Object;

namespace Cosmetris.Render.UI.Controls.Animation;

public class ScaleClosingAnimation : IControlIClosingAnimation
{
    private readonly Control _control;
    private readonly float _duration;
    private readonly Vector2 _endScale;

    private readonly float _initialTextScale = 1f;
    private readonly Vector2 _startScale;
    private readonly Dictionary<Control, Vector2[]> ChildOriginal = new();
    private readonly Dictionary<Game.Objects.Object, Microsoft.Xna.Framework.Color> ObjectOriginal = new();
    private float _elapsedTime;
    private float _opacity = 1f;

    public ScaleClosingAnimation(Control control, Vector2 startScale, Vector2 endScale, float duration)
    {
        _control = control;
        _startScale = startScale;

        // Can't divide by zero (to determine scale factor)
        if (endScale == Vector2.Zero)
            endScale = Vector2.One;

        _endScale = endScale;
        _duration = duration;
        _elapsedTime = 0f;
        IsClosing = false;
    }

    private float TextScale => _initialTextScale * ScaleFactor.X;

    private Vector2 ScaleFactor => _control.Size / _startScale;

    public bool IsClosing { get; private set; }

    public void Update(Control control, GameTime gameTime)
    {
        if (!IsClosing) return;

        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _opacity -= 0.005f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        var progress = Math.Min(_elapsedTime / _duration, 1.0f); // Ensure progress doesn't exceed 1

        var deltaSize = Vector2.Lerp(_startScale, _endScale, progress) - _control.Size;

        // End the animation if needed
        if (_elapsedTime >= _duration || _control.Size.X <= 0 || _control.Size.Y <= 0)
        {
            OnComplete?.Invoke(this, EventArgs.Empty);
            control.IsMarkedForDeletion = true;
            control.OnClose?.Invoke(this, EventArgs.Empty);
            IsClosing = false;
            _control.Dispose();
        }
        
        _control.Size += deltaSize;
        _control.Position -= deltaSize / 2f;

        ApplyToChildControls(control);
    }

    public void Draw(Control control, SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Currently empty, but reserved for potential drawing logic
    }

    public void StartClosing()
    {
        IsClosing = true;
    }

    public void ApplyToChildControls(Control control)
    {
        if (!IsClosing) return;

        foreach (var childControl in _control.GetChildren())
        {
            if (!ChildOriginal.ContainsKey(childControl))
            {
                Vector2[] original = { childControl.Size, childControl.Position };
                ChildOriginal.Add(childControl, original);
            }

            childControl.Size = ChildOriginal[childControl][0] * ScaleFactor;
            childControl.Position = ChildOriginal[childControl][1] * ScaleFactor;

            childControl.SetTextScale(TextScale);
        }
    }

    public Vector2 GetScaleFactor()
    {
        return ScaleFactor;
    }

    public float GetOpacity()
    {
        return _opacity;
    }

    public EventHandler OnComplete { get; set; }
}