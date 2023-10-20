/*
 * ShakeAnimator.cs is part of Cosmetris.
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

namespace Cosmetris.Game.Grid.Util.Animators;

/// <summary>
///     A Shake Animator for the Cosmono grid.
/// </summary>
public class ShakeAnimator
{
    private readonly double _originalDecreaseFactor; // The original decrease factor of the shake.
    private readonly double _originalShakeDuration; // The original duration of the shake.

    private readonly double _originalShakeRadius; // The original radius of the shake.
    private double _decreaseFactor; // The decrease factor of the shake.
    private double _elapsedTime; // The elapsed time of the shake.
    private Vector2 _offset = new(0, 0);
    private double _shakeDuration; // The duration of the shake.

    private double _shakeRadius; // The radius of the shake.
    private double _shakeStartAngle; // The starting angle of the shake.

    public ShakeAnimator(double shakeRadius = 15, double shakeDuration = 2.0, double decreaseFactor = 0.25)
    {
        _originalShakeRadius = shakeRadius;
        _originalShakeDuration = shakeDuration;
        _originalDecreaseFactor = decreaseFactor;

        _shakeRadius = shakeRadius;
        _shakeDuration = shakeDuration;
        _decreaseFactor = decreaseFactor;
    }

    /// <summary>
    ///     Whether or not the shake is currently active.
    /// </summary>
    public bool IsShaking { get; private set; }

    /// <summary>
    ///     Starts(Or restarts) the shake.
    /// </summary>
    public void StartShake()
    {
        IsShaking = true;
        _shakeStartAngle = 0;
        _elapsedTime = 0;
        _shakeRadius = _originalShakeRadius;
        _shakeDuration = _originalShakeDuration;
        _decreaseFactor = _originalDecreaseFactor;
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

        // There is nothing else to do until we begin shaking.
        if (!IsShaking) return;

        // Update the offset.
        _offset = new Vector2((float)(Math.Sin(_shakeStartAngle) * _shakeRadius),
            (float)(Math.Cos(_shakeStartAngle) * _shakeRadius));

        _shakeRadius -= _decreaseFactor * gameTime.ElapsedGameTime.TotalMilliseconds;
        _shakeStartAngle += 150 + new Random().Next(60);

        // If we either have reached the maximum time, or the shake radius is less than or equal to 0, stop shaking.
        if (_elapsedTime > _shakeDuration || _shakeRadius <= 0)
        {
            IsShaking = false;
            _shakeRadius = 0;
            _shakeStartAngle = 0;
            _elapsedTime = 0;
        }
    }

    /// <summary>
    ///     Gets the current matrix of the shake.
    /// </summary>
    /// <returns> The current matrix of the shake. </returns>
    public Matrix GetCurrentMatrix()
    {
        return IsShaking ? Matrix.CreateTranslation(_offset.X, _offset.Y, 0) : Matrix.Identity;
    }
}