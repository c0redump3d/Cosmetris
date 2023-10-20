/*
 * RandomUtil.cs is part of Cosmetris.
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
using System.Drawing;

namespace Cosmetris.Util.Numbers;

public static class RandomUtil
{
    private static readonly Random _random = new();

    /// <summary>
    ///     Returns a random float between 0 (inclusive) and 1 (exclusive).
    /// </summary>
    public static float NextFloat()
    {
        return (float)_random.NextDouble();
    }

    /// <summary>
    ///     Returns a random float between a specified range.
    /// </summary>
    /// <param name="minValue">Inclusive lower bound.</param>
    /// <param name="maxValue">Exclusive upper bound.</param>
    public static float NextFloat(float minValue, float maxValue)
    {
        return minValue + (float)_random.NextDouble() * (maxValue - minValue);
    }

    /// <summary>
    ///     Returns a random boolean value.
    /// </summary>
    public static bool NextBool()
    {
        return _random.Next(0, 2) == 0;
    }

    /// <summary>
    ///     Returns a random color.
    /// </summary>
    public static Color NextColor()
    {
        return Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
    }

    public static int Next(int i, int i2)
    {
        return _random.Next(i, i2);
    }
}