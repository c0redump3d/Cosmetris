/*
 * CosmonoRain.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Settings;
using Cosmetris.Util.Numbers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Util.Background;

public class CosmonoRain
{
    
    private const float _standardGravityInterval = 0.5f; // 500 ms in seconds
    private const int _standardSpawnMax = 30;
    
    private float _gravityInterval = 0.5f; // 500 ms in seconds
    private int _spawnMax = 30;
    private const int _minSpawnInterval = 2000; // 1/2 second
    private const int _maxSpawnInterval = 6000; // 6 seconds

    private readonly List<Vector2> _positions;

    private readonly List<RainShapes> _rainShapes;
    private readonly Random _rand;

    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    private readonly int _unitSize = 48; // multiplier for offsets
    private float _fadeTime;
    private float _gravityTimer;
    private bool _isHidden;

    private bool _isHiding;

    private bool _shouldUpdate = true;

    private float _spawnTime; // Time to spawn the next block
    private float _waitTime = 100;

    public CosmonoRain()
    {
        _rainShapes = new List<RainShapes>();
        _positions = new List<Vector2>();
        _rand = new Random();
        InitializeRainShapes();

        // Set the initial spawn time for the next block
        _spawnTime = _rand.Next(_minSpawnInterval, _maxSpawnInterval);
    }

    public static CosmonoRain Instance { get; } = new();

    public void HideRain()
    {
        _isHiding = true;
        Window.Instance.ScreenRenderer().GetScreen().AddConsoleMessage("Hiding rain");
    }

    public void ShowRain()
    {
        var rainEnabled = GameSettings.Instance.GetValue<bool>("Background Options", "Cosmono Rain");

        if (!rainEnabled)
        {
            HideRain();
            return;
        }
        
        _isHidden = false;
        _isHiding = false;
        _shouldUpdate = true;
        
        var intensity = GameSettings.Instance.GetValue<float>("Background Options", "Cosmono Rain Intensity");
        var speed = GameSettings.Instance.GetValue<float>("Background Options", "Cosmono Rain Speed");
        
        _gravityInterval = _standardGravityInterval / speed;
        _spawnMax = (int) (_standardSpawnMax * intensity);
        
        if(Window.Instance.ScreenRenderer().GetScreen() != null)
            Window.Instance.ScreenRenderer().GetScreen().AddConsoleMessage("Showing rain");
    }

    private void InitializeRainShapes()
    {
        for (var i = 0; i < 1; i++) // Only spawn 1 or 2 blocks initially
            SpawnNewShape();
    }

    private void RotateShapeRandomly(RainShapes shape)
    {
        var rotations = _rand.Next(0, 4);
        for (var i = 0; i < rotations; i++) RotateShapeClockwise(shape);
    }

    private void RotateShapeClockwise(RainShapes shape)
    {
        Vector2 rotationCenter;
        if (shape.Equals(RainShapes.I) || shape.Equals(RainShapes.O))
            rotationCenter = new Vector2(0.5f, 0.5f);
        else
            rotationCenter = shape.Offsets[0];

        for (var i = 0; i < shape.Offsets.Count; i++)
        {
            var relativePosition = shape.Offsets[i] - rotationCenter;
            shape.Offsets[i] = rotationCenter + new Vector2(relativePosition.Y, -relativePosition.X);
        }
    }

    public void UpdateRain(GameTime gameTime)
    {
        if (!_shouldUpdate) return;

        var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_isHiding)
        {
            _fadeTime -= 0.4f * elapsedSeconds; // Decreasing the fade time to hide

            if (_fadeTime <= 0f)
            {
                _shouldUpdate = false;
                _fadeTime = 0f;
                _isHidden = true;
            }
        }
        else
        {
            if (_fadeTime < 1f) _fadeTime += 0.4f * elapsedSeconds; // Increasing the fade time to show
        }

        _gravityTimer += elapsedSeconds;
        if (_gravityTimer >= _gravityInterval)
        {
            MoveShapesDown();
            _gravityTimer -= _gravityInterval;
        }

        _waitTime -= (float)(1.0f * gameTime.ElapsedGameTime.TotalMilliseconds);
        if (_waitTime <= 0)
        {
            RecycleOffScreenShapes();
            _waitTime = _rand.Next(350, 6000);
        }

        if (_rainShapes.Count < _spawnMax)
        {
            _spawnTime -= (float)(1.0f * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (_spawnTime <= 0)
            {
                SpawnNewShape();
                _spawnTime = _rand.Next(_minSpawnInterval, _maxSpawnInterval);
            }
        }
    }

    private bool CheckCollision(RainShapes shape, Vector2 position)
    {
        foreach (var offset in shape.Offsets)
        {
            var actualPosition = position + offset;

            for (var i = 0; i < _rainShapes.Count; i++)
                foreach (var existingOffset in _rainShapes[i].Offsets)
                    if (_positions[i] + existingOffset == actualPosition)
                        return true;
        }

        return false;
    }

    private bool CheckInBounds(Vector2 position, RainShapes shape)
    {
        foreach (var offset in shape.Offsets)
        {
            var actualPosition = position + offset;
            var width = _scalingManager.ActualWidth / _scalingManager.GetScaledX(_unitSize);
            if (actualPosition.X < 0 || actualPosition.X >= width)
                return false;
        }

        return true;
    }

    private void SpawnNewShape()
    {
        var shape = RainShapes.GetRandomShape(_rand);
        RotateShapeRandomly(shape);
        var width = _scalingManager.DesiredWidth / _scalingManager.GetScaledX(_unitSize);

        var position = new Vector2(_rand.Next(0, width), -8); // -3 to ensure there's room for all cosmono blocks

        var isSafeToSpawn = true;
        foreach (var offset in shape.Offsets)
        {
            var actualPosition = position + offset;

            if (CheckCollision(shape, actualPosition))
            {
                isSafeToSpawn = false;
                break;
            }
        }

        if (isSafeToSpawn)
        {
            _rainShapes.Add(shape);
            _positions.Add(position);
        }
    }

    private void MoveShapesDown()
    {
        for (var i = 0; i < _rainShapes.Count; i++) _positions[i] = new Vector2(_positions[i].X, _positions[i].Y + 1);
    }

    private void RecycleOffScreenShapes()
    {
        int randAmount = RandomUtil.Next(1, 8);
        int spawnedCount = 0;
        
        for (var i = 0; i < _rainShapes.Count; i++)
            if (_rainShapes[i].Offsets.All(offset => offset.Y + _positions[i].Y > ((float)_scalingManager.ActualHeight / _scalingManager.GetScaledY(_unitSize))))
            {
                var newShape = RainShapes.GetRandomShape(_rand);
                RotateShapeRandomly(newShape);

                var width = _scalingManager.ActualWidth / _scalingManager.GetScaledX(_unitSize);

                var newPosition = new Vector2(_rand.Next(0, width), -8);

                if (!CheckCollision(newShape, newPosition) && CheckInBounds(newPosition, newShape))
                {
                    _rainShapes[i] = newShape;
                    _positions[i] = newPosition;
                }

                if(spawnedCount >= randAmount) break;
                
                spawnedCount++;
            }
    }

    public void DrawRain(SpriteBatch spriteBatch)
    {
        if (_isHidden) return;

        for (var i = 0; i < _rainShapes.Count; i++)
            foreach (var offset in _rainShapes[i].Offsets)
            {
                var destinationRect = new Rectangle(
                    (int)((_positions[i].X + offset.X) * _scalingManager.GetScaledX(_unitSize)),
                    (int)((_positions[i].Y + offset.Y) * _scalingManager.GetScaledY(_unitSize)),
                    _scalingManager.GetScaledX(_unitSize), _scalingManager.GetScaledY(_unitSize));

                spriteBatch.Draw(_rainShapes[i].Texture, destinationRect, Color.White * (0.85f * _fadeTime));
            }
    }


    private class RainShapes
    {
        private RainShapes(RainShapes type)
        {
            Offsets = type.Offsets;
            Texture = type.Texture;
        }

        private RainShapes(List<Vector2> offsets, string textureName)
        {
            Offsets = offsets;

            if (TextureManager.Instance.GetTexture(textureName) == null)
                TextureManager.Instance.AddTexture(textureName, $"Block/{textureName}");

            Texture = TextureManager.Instance.GetTexture2D(textureName);
        }

        internal List<Vector2> Offsets { get; }
        internal Texture2D Texture { get; }

        internal static RainShapes I =>
            new(new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(2, 0) },
                "i_block");

        private static RainShapes T =>
            new(
                new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(0, -1) },
                "t_block");

        private static RainShapes Z =>
            new(
                new List<Vector2> { Vector2.Zero, new(1, 0), new(0, -1), new(-1, -1) },
                "z_block");

        private static RainShapes S =>
            new(
                new List<Vector2> { Vector2.Zero, new(-1, 0), new(0, -1), new(1, -1) },
                "s_block");

        private static RainShapes J =>
            new(
                new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(-1, -1) },
                "j_block");

        private static RainShapes L =>
            new(
                new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(1, -1) },
                "l_block");

        internal static RainShapes O =>
            new(new List<Vector2> { Vector2.Zero, new(1, 0), new(0, 1), new(1, 1) },
                "o_block");

        public override bool Equals(object obj)
        {
            if (!(obj is RainShapes))
                return false;

            return ((RainShapes)obj).Texture == Texture;
        }

        public static RainShapes GetRandomShape(Random random)
        {
            var shapes = new List<RainShapes> { I, T, Z, S, J, L, O };
            return shapes[random.Next(shapes.Count)];
        }
    }
}