/*
 * CosmonoPiece.cs is part of Cosmetris.
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

using Cosmetris.Game.Grid;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects.Cosmonoes;

/// <summary>
///     A CosmonoPiece is a single piece of a full Cosmono shape.
/// </summary>
public class CosmonoPiece : Object
{
    private readonly TetrisGameManager _gameManager;

    public CosmonoPiece(TetrisGameManager gameManager, string name, Vector2 position, Vector2 size, Texture2D texture,
        Color color) : base(name, position, size, texture, color)
    {
        _gameManager = gameManager;
    }

    public Texture2D PlacedTexture { get; set; }
    private TetrisGrid Grid => _gameManager.GetGrid();
    private static UIScalingManager ScalingManager => Window.Instance.ScalingManager;

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Hidden)
        {
            // Clamp rotation to 0-360
            var rotation = Rotation % 360;

            // If this piece has a Parent (like the game grid), get its position in screen coordinates.
            // Otherwise, use its position directly.
            var position = Parent != null
                ? GridToScreenPosition(Position) +
                  new Vector2(3.25f, 4.75f) // Had to offset the position a bit to make it look right.
                : Position;

            // Then use the calculated position for drawing
            spriteBatch.Draw(Texture, ScalingManager.GetScaledPosition(position), null, Color * Opacity, rotation,
                Origin,
                ScalingManager.GetScaledPosition(Size), SpriteEffects.None, Layer);
        }

        if (_children != null)
            foreach (var child in _children)
                child.Draw(spriteBatch);
    }

    /// <summary>
    ///     Converts a grid position to a screen position.
    /// </summary>
    /// <param name="gridPosition"> The position on the grid. </param>
    /// <returns> The position on the screen. </returns>
    private Vector2 GridToScreenPosition(Vector2 gridPosition)
    {
        var cellSize = Grid.GetCellSize();
        var lineWidth = Grid.GetLineWidth();
        var borderSize = Grid.GetBorderWidth();

        var gridOffset = GetActualPosition() - new Vector2(borderSize, borderSize);

        var complete = new Vector2(
            gridPosition.X * (cellSize + lineWidth) + gridOffset.X,
            gridPosition.Y * (cellSize + lineWidth) + gridOffset.Y
        );

        return complete;
    }

    /// <summary>
    ///     Attempts to return the position based on the parent's position.
    /// </summary>
    /// <returns> The actual position of the object. </returns>
    private Vector2 GetActualPosition()
    {
        var position = Parent?.Position ?? Position;
        return position;
    }
}