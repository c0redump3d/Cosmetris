/*
 * Rubble.cs is part of Cosmetris.
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
using System.IO;
using Cosmetris.Game.Packs.TexturePacks;
using Cosmetris.Render.Managers;
using Cosmetris.Util;
using Cosmetris.Util.Numbers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Particle.Particles;

public class Rubble : Particle
{
    private const float GRAVITY = 112.8f; // Adjust as needed
    private static List<Texture2D> _rubbleTextures;
    private readonly float _rotationSpeed; // Speed of rotation (radians per second)
    private readonly Texture2D _texture;

    private float _rotation; // Current rotation of the rubble
    private Vector2 _velocity;

    static Rubble()
    {
        // Only add textures
        // read how many files are in the rubble directory
        var count = Directory.GetFiles($"{ContentUtil.Instance.RootDirPath}/Textures/FX/Rubble").Length;
        if (count == 0)
            throw new Exception("No rubble textures found!");

        for (var i = 0; i < count; i++)
            TextureManager.Instance.AddTexture($"rubble{i}", $"FX/Rubble/rubble{i}");
    }

    public Rubble(float x, float y, float size, float speed, float angle, Color color)
        : base(x, y, size, speed, angle, color)
    {
        _texture = _rubbleTextures[RandomUtil.Next(0, _rubbleTextures.Count)];

        // Initial velocity based on speed and angle
        _velocity = new Vector2(speed * (float)Math.Cos(angle), speed * (float)Math.Sin(angle));

        _rotation = 0f;
        _rotationSpeed = RandomUtil.NextFloat(-1f, 1f);
    }

    public static void LoadTextures(TexturePack pack)
    {
        _rubbleTextures = new List<Texture2D>();
        if (pack == null)
        {
            var count = Directory.GetFiles($"{ContentUtil.Instance.RootDirPath}/Textures/FX/Rubble").Length;
            if (count == 0)
                throw new Exception("No rubble textures found!");

            for (var i = 0; i < count; i++) _rubbleTextures.Add(TextureManager.Instance.GetTexture2D($"rubble{i}"));
        }
        else
        {
            var count = pack.RubbleFX.Count;
            if (count == 0)
                throw new Exception("No rubble textures found!");

            for (var i = 0; i < count; i++)
                _rubbleTextures.Add(pack.RubbleFX[$"rubble{i}"]);
        }
    }

    public override void Update(GameTime gameTime)
    {
        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update position based on velocity
        X += _velocity.X * deltaSeconds;
        Y += _velocity.Y * deltaSeconds;

        // Update rotation based on rotation speed
        _rotation += _rotationSpeed * deltaSeconds;

        // Apply gravity to the vertical velocity
        _velocity.Y += GRAVITY * deltaSeconds;

        // Check for out-of-bounds and remove if necessary
        if (X < 0 || X > ScalingManager.DesiredWidth || Y > ScalingManager.DesiredHeight)
            ParticleManager.Instance.RemoveParticle(this);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_texture, ScalingManager.GetScaledPosition(new Vector2(X, Y)), null, Color, _rotation,
            ScalingManager.GetScaledPosition(new Vector2(_texture.Width / 2f, _texture.Height / 2f)), Size,
            SpriteEffects.None, 0f);
    }
}