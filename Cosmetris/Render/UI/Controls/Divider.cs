/*
 * Divider.cs is part of Cosmetris.
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

using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Cosmetris.Render.UI.Controls;

public class Divider : Control
{
    public Divider(Vector2 position, Vector2 size, Microsoft.Xna.Framework.Color color)
    {
        Position = new Vector2(position.X - (size.X/2f), position.Y);
        Size = size;
        Color = color;
        Initialize();
    }

    public Microsoft.Xna.Framework.Color Color { get; set; }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var actualPosition = GetActualPosition();
        
        RenderUtil.DrawLine(actualPosition, actualPosition + new Vector2(Size.X, 0), Color);
        base.Draw(spriteBatch, gameTime);
    }

    public override void SetPosition(Vector2 position)
    {
        position.X -= Size.X / 2f;
        base.SetPosition(position);
    }
}