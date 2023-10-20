/*
 * RotationData.cs is part of Cosmetris.
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

namespace Cosmetris.Game.Objects.Cosmonoes;

public static class RotationData
{
    public enum Rotation
    {
        Zero,
        Right,
        Two,
        Left
    }

    private static readonly Dictionary<(Rotation, Rotation), List<Vector2>> JLSZTData = new()
    {
        {
            (Rotation.Zero, Rotation.Right),
            new List<Vector2> { new(0, 0), new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2) }
        },
        {
            (Rotation.Right, Rotation.Zero),
            new List<Vector2> { new(0, 0), new(1, 0), new(1, -1), new(0, 2), new(1, 2) }
        },
        {
            (Rotation.Right, Rotation.Two), new List<Vector2> { new(0, 0), new(1, 0), new(1, -1), new(0, 2), new(1, 2) }
        },
        {
            (Rotation.Two, Rotation.Right),
            new List<Vector2> { new(0, 0), new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2) }
        },
        {
            (Rotation.Two, Rotation.Left), new List<Vector2> { new(0, 0), new(1, 0), new(1, 1), new(0, -2), new(1, -2) }
        },
        {
            (Rotation.Left, Rotation.Two),
            new List<Vector2> { new(0, 0), new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2) }
        },
        {
            (Rotation.Left, Rotation.Zero),
            new List<Vector2> { new(0, 0), new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2) }
        },
        {
            (Rotation.Zero, Rotation.Left),
            new List<Vector2> { new(0, 0), new(1, 0), new(1, 1), new(0, -2), new(1, -2) }
        }
    };

    private static readonly Dictionary<(Rotation, Rotation), List<Vector2>> IData = new()
    {
        {
            (Rotation.Zero, Rotation.Right),
            new List<Vector2> { new(0, 0), new(-2, 0), new(1, 0), new(-2, -1), new(1, 2) }
        },
        {
            (Rotation.Right, Rotation.Zero),
            new List<Vector2> { new(0, 0), new(2, 0), new(-1, 0), new(2, 1), new(-1, -2) }
        },
        {
            (Rotation.Right, Rotation.Two),
            new List<Vector2> { new(0, 0), new(-1, 0), new(2, 0), new(-1, 2), new(2, -1) }
        },
        {
            (Rotation.Two, Rotation.Right),
            new List<Vector2> { new(0, 0), new(1, 0), new(-2, 0), new(1, -2), new(-2, 1) }
        },
        {
            (Rotation.Two, Rotation.Left),
            new List<Vector2> { new(0, 0), new(2, 0), new(-1, 0), new(2, 1), new(-1, -2) }
        },
        {
            (Rotation.Left, Rotation.Two),
            new List<Vector2> { new(0, 0), new(-2, 0), new(1, 0), new(-2, -1), new(1, 2) }
        },
        {
            (Rotation.Left, Rotation.Zero),
            new List<Vector2> { new(0, 0), new(1, 0), new(-2, 0), new(1, -2), new(-2, 1) }
        },
        {
            (Rotation.Zero, Rotation.Left),
            new List<Vector2> { new(0, 0), new(-1, 0), new(2, 0), new(-1, 2), new(2, -1) }
        }
    };

    private static readonly Dictionary<(Rotation, Rotation), List<Vector2>> OData = new()
    {
        {
            (Rotation.Zero, Rotation.Right),
            new List<Vector2> { new(0, 0), new(0, -1), new(-1, -1), new(1, 0), new(-1, 0) }
        },
        {
            (Rotation.Right, Rotation.Zero),
            new List<Vector2> { new(0, 0), new(0, 1), new(1, 1), new(-1, 0), new(1, 0) }
        },
        {
            (Rotation.Right, Rotation.Two), new List<Vector2> { new(0, 0), new(0, 1), new(1, 1), new(-1, 0), new(1, 0) }
        },
        {
            (Rotation.Two, Rotation.Right),
            new List<Vector2> { new(0, 0), new(0, -1), new(-1, -1), new(1, 0), new(-1, 0) }
        },
        {
            (Rotation.Two, Rotation.Left),
            new List<Vector2> { new(0, 0), new(0, -1), new(-1, -1), new(1, 0), new(-1, 0) }
        },
        { (Rotation.Left, Rotation.Two), new List<Vector2> { new(0, 0), new(0, 1), new(1, 1), new(-1, 0), new(1, 0) } },
        {
            (Rotation.Left, Rotation.Zero), new List<Vector2> { new(0, 0), new(0, 1), new(1, 1), new(-1, 0), new(1, 0) }
        },
        {
            (Rotation.Zero, Rotation.Left),
            new List<Vector2> { new(0, 0), new(0, -1), new(-1, -1), new(1, 0), new(-1, 0) }
        }
    };

    public static List<Vector2> GetKickData(Rotation from, Rotation to, bool isI, bool isO)
    {
        if (isI)
            return IData[(from, to)];
        if (isO)
            return OData[(from, to)];

        return JLSZTData[(from, to)];
    }

    public static Rotation RotationIndexToRotation(int currentRotationIndex)
    {
        var clampedIndex = Math.Max(0, Math.Min(3, currentRotationIndex));

        switch (clampedIndex)
        {
            case 0:
                return Rotation.Zero;
            case 1:
                return Rotation.Right;
            case 2:
                return Rotation.Two;
            case 3:
                return Rotation.Left;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentRotationIndex), currentRotationIndex, null);
        }
    }
}