/*
 * Cell.cs is part of Cosmetris.
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Grid.Util;

public class Cell
{
    public Cell()
    {
        IsOccupied = false;
        Texture = null;
        Color = Color.Transparent;
    }

    /// <summary>
    ///     This is used to determine if the cell is occupied or not.
    /// </summary>
    public bool IsOccupied { get; set; }

    /// <summary>
    ///     This is used to determine what texture the cell should use.
    /// </summary>
    public Texture2D Texture { get; set; }

    public Color Color { get; set; }
}