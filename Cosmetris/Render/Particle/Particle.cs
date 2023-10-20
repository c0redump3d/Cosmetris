/*
 * Particle.cs is part of Cosmetris.
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

using Cosmetris.Render.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Particle;

public class Particle
{
    protected static readonly UIScalingManager ScalingManager = Window.Instance.ScalingManager;

    private float _lifeTime;
    protected bool _normalRender = true;

    public Particle(float x, float y, float size, float speed, float angle, Color color)
    {
        X = x;
        Y = y;
        Size = size;
        Speed = speed;
        Angle = angle;
        Color = color;

        _lifeTime = 0;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Angle { get; set; }
    public Color Color { get; set; }

    public virtual void Update(GameTime gameTime)
    {
        if (X < 0 || X > ScalingManager.DesiredWidth || Y < 0 || Y > ScalingManager.DesiredHeight)
            ParticleManager.Instance.RemoveParticle(this);
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }
}