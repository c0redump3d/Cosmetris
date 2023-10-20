/*
 * ColorCache.cs is part of Cosmetris.
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
using Cosmetris.Util.Colors;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Color;

public class ColorCache
{
    private const int MaxColorCacheSize = 1000;

    public const float HoverTransitionSpeed = 2.5f;
    public const float ClickTransitionSpeed = 8.0f;

    private readonly
        Dictionary<(Microsoft.Xna.Framework.Color, Microsoft.Xna.Framework.Color, Microsoft.Xna.Framework.Color, float,
            float), Microsoft.Xna.Framework.Color> _colorCache = new();

    public void Update(Control control, float clickTarget, GameTime gameTime)
    {
        var clickSpeed = ClickTransitionSpeed;
        var clickDelta = (clickTarget - control.ClickLerpAmount) * clickSpeed *
                         (float)gameTime.ElapsedGameTime.TotalSeconds;
        control.ClickLerpAmount += clickDelta;
    }

    public Microsoft.Xna.Framework.Color GetLerpedRectangleColor(Control control,
        Microsoft.Xna.Framework.Color baseColor, Microsoft.Xna.Framework.Color hoverColor,
        Microsoft.Xna.Framework.Color clickColor)
    {
        // Make sure the cache doesn't get too big
        if (_colorCache.Count > MaxColorCacheSize) _colorCache.Clear();

        var opacity = baseColor.A / 255f;
        var cacheKey = (baseColor, hoverColor, clickColor, control.HoverLerpAmount, control.ClickLerpAmount);

        if (!_colorCache.TryGetValue(cacheKey, out var cachedColor))
        {
            Microsoft.Xna.Framework.Color lerpedColor;

            if (control.ClickLerpAmount < 1.0f)
            {
                opacity = clickColor.A / 255f;
                lerpedColor = ColorExtensions.Lerp(baseColor, clickColor, control.ClickLerpAmount);
            }
            else if (control.HoverLerpAmount < 1.0f)
            {
                opacity = hoverColor.A / 255f;
                lerpedColor = ColorExtensions.Lerp(baseColor, hoverColor, control.HoverLerpAmount);
            }
            else
            {
                lerpedColor = baseColor;
            }

            cachedColor = new Microsoft.Xna.Framework.Color(lerpedColor.R, lerpedColor.G, lerpedColor.B,
                (int)(lerpedColor.A * opacity));
            _colorCache[cacheKey] = cachedColor;
        }

        return cachedColor;
    }
}