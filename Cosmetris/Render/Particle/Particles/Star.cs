/*
 * Star.cs is part of Cosmetris.
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
using Cosmetris.Render.Managers;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Particle.Particles;

public class Star : Particle
{
    private static readonly Effect _glowEffect = EffectsManager.Instance.GetEffect("glow");
    private readonly Texture2D _starTexture;
    private readonly Vector2 _velocity;

    public Star(float x, float y, float size, float speed, float angle, Color color) : base(x, y, size, speed, angle,
        color)
    {
        _normalRender = false;
        _starTexture = RenderUtil.GetStarTexture();
        _velocity = new Vector2(Speed * (float)Math.Cos(Angle), Speed * (float)Math.Sin(Angle));
    }

    public override void Update(GameTime gameTime)
    {
        X += _velocity.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Y += _velocity.Y * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // We want to wrap the star around the screen
        if (X < -Size)
            X = ScalingManager.DesiredWidth + Size;
        else if (X - Size > ScalingManager.DesiredWidth) X = -Size;

        if (Y < -Size)
            Y = ScalingManager.DesiredHeight + Size;
        else if (Y - Size > ScalingManager.DesiredHeight) Y = -Size;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        float x = ScalingManager.GetScaledX(X);
        float y = ScalingManager.GetScaledY(Y);

        SpecialDraw(spriteBatch, _starTexture, new Vector2(x, y), _glowEffect, Color);
    }

    private void SpecialDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Effect effect, Color color)
    {
        spriteBatch.End();
        spriteBatch.Begin(effect: effect);
        effect.Parameters["GlowRadius"].SetValue(Size * 4f);
        effect.Parameters["Padding"].SetValue(new Vector2(8f, 8f));
        effect.Parameters["SpriteTexture"].SetValue(texture);
        effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, (int)Size, (int)Size), color * 0.5f);
        spriteBatch.End();
        spriteBatch.Begin();
    }
}