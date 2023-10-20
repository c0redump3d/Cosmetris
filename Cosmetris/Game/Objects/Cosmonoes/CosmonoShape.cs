/*
 * CosmonoShape.cs is part of Cosmetris.
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
using Cosmetris.Render.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects.Cosmonoes;

/// <summary>
///     The CosmonoShape class represents a shape that a cosmono can take.
/// </summary>
public class CosmonoShape
{
    public CosmonoShape()
    {
        Shapes = new List<CosmonoShape>
        {
            new CosmonoShape("I", new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(2, 0) },
                "Block/i_block", "Block/i_placed"),
            new CosmonoShape("T", new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(0, -1) },
                "Block/t_block", "Block/t_placed"),
            new CosmonoShape("Z", new List<Vector2> { Vector2.Zero, new(1, 0), new(0, -1), new(-1, -1) },
                "Block/z_block", "Block/z_placed"),
            new CosmonoShape("S", new List<Vector2> { Vector2.Zero, new(-1, 0), new(0, -1), new(1, -1) },
                "Block/s_block", "Block/s_placed"),
            new CosmonoShape("J", new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(-1, -1) },
                "Block/j_block", "Block/j_placed"),
            new CosmonoShape("L", new List<Vector2> { Vector2.Zero, new(1, 0), new(-1, 0), new(1, -1) },
                "Block/l_block", "Block/l_placed"),
            new CosmonoShape("O", new List<Vector2> { Vector2.Zero, new(1, 0), new(0, 1), new(1, 1) },
                "Block/o_block", "Block/o_placed")
        };
    }

    private CosmonoShape(string name, List<Vector2> offsets, string normalTexture, string placedTexture)
    {
        Name = name;
        Offsets = offsets;
        var normalName = normalTexture.Split('/')[1];
        var placedName = placedTexture.Split('/')[1];

        if (TextureManager.Instance.GetTexture(normalName) == null)
            TextureManager.Instance.AddTexture(normalName, normalTexture);

        if (TextureManager.Instance.GetTexture(placedName) == null)
            TextureManager.Instance.AddTexture(placedName, placedTexture);

        Texture = TextureManager.Instance.GetTexture2D(normalName);
        PlacedTexture = TextureManager.Instance.GetTexture2D(placedName);
    }

    public List<CosmonoShape> Shapes { get; }

    /// <summary>
    ///     The offsets of the blocks in the shape.
    /// </summary>
    public List<Vector2> Offsets { get; set; }

    /// <summary>
    ///     The texture of the shape.
    /// </summary>
    public Texture2D Texture { get; set; }

    public string Name { get; set; }

    /// <summary>
    ///     The texture of the shape when it is placed.
    /// </summary>
    public Texture2D PlacedTexture { get; set; }

    public List<CosmonoShape> AllShapes()
    {
        return Shapes;
    }

    public bool Equals(string name)
    {
        return Name == name;
    }

    public override bool Equals(object obj)
    {
        // expect a string name
        if (obj is string name)
            return Name == name;

        // expect a CosmonoShape
        if (obj is CosmonoShape shape)
            return Name == shape.Name;

        return false;
    }
}