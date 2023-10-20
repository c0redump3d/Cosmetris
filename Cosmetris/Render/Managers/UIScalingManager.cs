/*
 * UIScalingManager.cs is part of Cosmetris.
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

namespace Cosmetris.Render.Managers;

public class UIScalingManager
{
    public delegate void ScreenResizedEventHandler(Vector2 newResolution, Vector2 oldScaleFactors);

    private readonly int _defaultDesiredHeight;
    private readonly int _defaultDesiredWidth;

    private readonly Dictionary<float, (int width, int height)> _knownAspectRatios = new()
    {
        { 16f / 9f, (1920, 1080) }, // 16:9 -> 1920x1080
        { 21f / 9f, (2560, 1080) }, // 21:9 -> 2560x1080
        { 4f / 3f, (1440, 1080) }, // 4:3  -> 1440x1080
        { 3f / 2f, (1620, 1080) }, // 3:2  -> 1620x1080
        { 1f / 1f, (1080, 1080) } // 1:1  -> 1080x1080
    };

    public UIScalingManager(int desiredWidth, int desiredHeight, int actualWidth, int actualHeight)
    {
        _defaultDesiredWidth = desiredWidth;
        _defaultDesiredHeight = desiredHeight;

        DesiredWidth = desiredWidth;
        DesiredHeight = desiredHeight;

        ActualWidth = actualWidth;
        ActualHeight = actualHeight;

        AdjustDesiredDimensionsToAspectRatio();
    }


    public int DesiredWidth { get; private set; }
    public int DesiredHeight { get; private set; }

    public int ActualWidth { get; private set; }
    public int ActualHeight { get; private set; }

    public float WidthScaleFactor => (float)ActualWidth / DesiredWidth;
    public float HeightScaleFactor => (float)ActualHeight / DesiredHeight;
    public event ScreenResizedEventHandler ScreenResized;

    private void AdjustDesiredDimensionsToAspectRatio()
    {
        var currentAspectRatio =
            (float)Math.Round(ActualWidth / (double)ActualHeight, 2); // Rounded to 2 decimal places

        if (_knownAspectRatios.TryGetValue(currentAspectRatio, out var ratio))
        {
            DesiredWidth = ratio.width;
            DesiredHeight = ratio.height;
            return; // We've matched a known aspect ratio, so exit early
        }

        // If not a known aspect ratio, proceed with the default logic
        var defaultAspectRatio = (float)_defaultDesiredWidth / _defaultDesiredHeight;

        if (currentAspectRatio > defaultAspectRatio) // Wider than default
        {
            DesiredWidth = (int)(_defaultDesiredHeight * currentAspectRatio);
            DesiredHeight = _defaultDesiredHeight;
        }
        else if (currentAspectRatio < defaultAspectRatio) // Taller than default
        {
            DesiredWidth = _defaultDesiredWidth;
            DesiredHeight = (int)(_defaultDesiredWidth / currentAspectRatio);
        }
        else // The same
        {
            DesiredWidth = _defaultDesiredWidth;
            DesiredHeight = _defaultDesiredHeight;
        }
    }

    public Vector2 GetScaledPosition(Vector2 position)
    {
        return new Vector2(position.X * WidthScaleFactor, position.Y * HeightScaleFactor);
    }

    public int GetScaledX(int x)
    {
        return (int)(x * WidthScaleFactor);
    }

    public int GetScaledY(int y)
    {
        return (int)(y * HeightScaleFactor);
    }

    public int GetScaledX(float x)
    {
        return GetScaledX((int)x);
    }

    public int GetScaledY(float y)
    {
        return GetScaledY((int)y);
    }

    public Vector2 GetScaledSize(Vector2 size)
    {
        return new Vector2(size.X * WidthScaleFactor, size.Y * HeightScaleFactor);
    }

    public void ResizeScale(int newWidth, int newHeight)
    {
        var oldDesired = new Vector2(DesiredWidth, DesiredHeight);

        ActualWidth = newWidth;
        ActualHeight = newHeight;

        AdjustDesiredDimensionsToAspectRatio();

        ScreenResized?.Invoke(new Vector2(DesiredWidth, DesiredHeight), oldDesired);
    }

    public Rectangle GetScaledRectangle(Rectangle rect)
    {
        return new Rectangle(GetScaledX(rect.X), GetScaledY(rect.Y), GetScaledX(rect.Width), GetScaledY(rect.Height));
    }
}