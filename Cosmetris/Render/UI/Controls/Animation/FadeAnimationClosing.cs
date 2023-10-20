/*
 * FadeAnimationClosing.cs is part of Cosmetris.
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls.Animation;

public class FadeAnimationClosing : IControlIClosingAnimation
{
    private readonly float _duration;

    private readonly Control _control;
    private float _elapsedTime;
    private float _decayRate;

    public FadeAnimationClosing(Control control, float duration)
    {
        _control = control;
        Opacity = 1f;
        _duration = duration;
        _elapsedTime = 0f;
        _decayRate = 1f / duration;
        IsClosing = true;
    }

    public float Opacity { get; set; }
    public bool IsClosing { get; private set; }
    public EventHandler OnComplete { get; set; }

    public void Update(Control control, GameTime gameTime)
    {
        if (!IsClosing) return;

        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        Opacity -= _decayRate * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // End the animation if needed
        if (_elapsedTime >= _duration || Opacity <= 0)
        {
            OnComplete?.Invoke(this, EventArgs.Empty);
            Opacity = 0f;
            control.Hidden = true;
            control.IsMarkedForDeletion = true;
            control.OnClose?.Invoke(this, EventArgs.Empty);
            IsClosing = false;
            _control.Dispose();
        }
    }

    public void Draw(Control control, SpriteBatch spriteBatch, GameTime gameTime)
    {
    }

    public void StartClosing()
    {
        IsClosing = true;
    }

    public void ApplyToChildControls(Control control)
    {
    }

    public Vector2 GetScaleFactor()
    {
        return Vector2.One;
    }

    public float GetOpacity()
    {
        return MathHelper.Clamp(Opacity, 0f, 1f);
    }
}