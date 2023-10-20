/*
 * ColorExtensions.cs is part of Cosmetris.
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

namespace Cosmetris.Util.Colors;

public static class ColorExtensions
{
    public static Microsoft.Xna.Framework.Color HsvToRgb(float hue, float saturation, float value)
    {
        float r = 0, g = 0, b = 0;
        if (value != 0)
        {
            if (saturation == 0)
            {
                r = g = b = value;
            }
            else
            {
                var max = value;
                var dif = value * saturation;
                var min = value - dif;

                var h = hue * 360f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
        }

        return new Microsoft.Xna.Framework.Color(r, g, b);
    }

    public static Microsoft.Xna.Framework.Color Lerp(Microsoft.Xna.Framework.Color color1, Microsoft.Xna.Framework.Color color2, float amount)
    {
        var r = (byte)(color1.R + amount * (color2.R - color1.R));
        var g = (byte)(color1.G + amount * (color2.G - color1.G));
        var b = (byte)(color1.B + amount * (color2.B - color1.B));
        var a = (byte)(color1.A + amount * (color2.A - color1.A));
        return new Microsoft.Xna.Framework.Color(r, g, b, a);
    }

    public static Microsoft.Xna.Framework.Color Rainbow(double time, float fade, float speed = 1.0f)
    {
        // We expect time to be smallest number possible, milliseconds.
        return HsvToRgb((float)(time * speed / 1000.0f % 1.0f), 1.0f, fade);
    }

    public static Vector3 RgbToHsv(Color color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        var h = max;
        var s = max;
        var v = max;

        var d = max - min;
        s = max == 0 ? 0 : d / max;

        if (max == min)
        {
            h = 0; // achromatic
        }
        else
        {
            if (max == r)
                h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / d + 2;
            else if (max == b)
                h = (r - g) / d + 4;
            h /= 6;
        }

        return new Vector3(h, s, v);
    }
}