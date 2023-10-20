/*
 * TSpinHandler.cs is part of Cosmetris.
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

using Cosmetris.Game.Objects.Cosmonoes;
using Microsoft.Xna.Framework;

namespace Cosmetris.Game.Grid.Util;

public class TSpinHandler
{
    private readonly int _cellSize;
    private readonly int _columns;
    private readonly TetrisGrid _grid;
    private readonly int _rows;

    public TSpinHandler(TetrisGrid grid)
    {
        _grid = grid;
        _cellSize = grid.GetCellSize();
        _columns = grid.TotalColumns();
        _rows = grid.TotalRows();
    }

    /// <summary>
    ///     Checks if the given Cosmono is a T-Block & is 'locked' in a T-Spin.
    /// </summary>
    /// <param name="cosmono"> The Cosmono to check. </param>
    /// <param name="rowsRemoved"> The number of rows removed. </param>
    /// <returns> Whether or not the Cosmono is a T-Block & is 'locked' in a T-Spin. </returns>
    public bool IsTSpin(Cosmono cosmono, int rowsRemoved)
    {
        if (!cosmono.GetCosmonoShape().Equals("T")) return false;

        var tSpinType = IsTLocked(cosmono, rowsRemoved, cosmono.LastRotationWasKick(), cosmono.LastMoveWasRotation());

        cosmono.GetScore().HandleTSpin(tSpinType, rowsRemoved);

        return tSpinType != TSpinType.NONE;
    }

    private TSpinType IsTLocked(Cosmono cosmono, int clearedLines, bool lastRotationWasKick, bool lastMoveWasRotation)
    {
        // Define the corners to check based on the current rotation of the T piece
        Vector2 topLeft, topRight, bottomLeft, bottomRight;

        switch (cosmono.ShapeRotation())
        {
            case RotationData.Rotation.Zero:
                topLeft = GetGridPosition(cosmono.Position, new Vector2(-1, 1));
                topRight = GetGridPosition(cosmono.Position, new Vector2(1, 1));
                bottomLeft = GetGridPosition(cosmono.Position, new Vector2(-1, -1));
                bottomRight = GetGridPosition(cosmono.Position, new Vector2(1, -1));
                break;
            case RotationData.Rotation.Right:
                topLeft = GetGridPosition(cosmono.Position, new Vector2(1, 1));
                topRight = GetGridPosition(cosmono.Position, new Vector2(1, -1));
                bottomLeft = GetGridPosition(cosmono.Position, new Vector2(-1, 1));
                bottomRight = GetGridPosition(cosmono.Position, new Vector2(-1, -1));
                break;
            case RotationData.Rotation.Two:
                topLeft = GetGridPosition(cosmono.Position, new Vector2(-1, -1));
                topRight = GetGridPosition(cosmono.Position, new Vector2(1, -1));
                bottomLeft = GetGridPosition(cosmono.Position, new Vector2(-1, 1));
                bottomRight = GetGridPosition(cosmono.Position, new Vector2(1, 1));
                break;
            case RotationData.Rotation.Left:
                topLeft = GetGridPosition(cosmono.Position, new Vector2(-1, -1));
                topRight = GetGridPosition(cosmono.Position, new Vector2(-1, 1));
                bottomLeft = GetGridPosition(cosmono.Position, new Vector2(1, -1));
                bottomRight = GetGridPosition(cosmono.Position, new Vector2(1, 1));
                break;
            default:
                return TSpinType.NONE; // Return early if not a valid rotation
        }

        var occupiedCount = 0;

        if (IsBlockAtPosition(topLeft)) occupiedCount++;
        if (IsBlockAtPosition(topRight)) occupiedCount++;
        if (IsBlockAtPosition(bottomLeft)) occupiedCount++;
        if (IsBlockAtPosition(bottomRight)) occupiedCount++;

        // Determine the T-Spin type based on occupiedCount and clearedLines
        switch (occupiedCount)
        {
            case 3:
                if (!lastMoveWasRotation)
                    break;

                return clearedLines switch
                {
                    0 => TSpinType.MINIZERO,
                    1 => TSpinType.ONE,
                    2 => TSpinType.TWO,
                    3 => TSpinType.THREE,
                    _ => TSpinType.NONE
                };
            case 2:
                // Check for T-Spin Mini conditions

                var isTSpinMini = cosmono.ShapeRotation() switch
                {
                    RotationData.Rotation.Zero => (!IsBlockAtPosition(bottomLeft) && IsBlockAtPosition(topRight)) ||
                                                  (!IsBlockAtPosition(bottomRight) && IsBlockAtPosition(topLeft)),
                    RotationData.Rotation.Right => (!IsBlockAtPosition(bottomLeft) && IsBlockAtPosition(topRight)) ||
                                                   (!IsBlockAtPosition(topLeft) && IsBlockAtPosition(bottomRight)),
                    RotationData.Rotation.Two => (!IsBlockAtPosition(topLeft) && IsBlockAtPosition(bottomRight)) ||
                                                 (!IsBlockAtPosition(topRight) && IsBlockAtPosition(bottomLeft)),
                    RotationData.Rotation.Left => (!IsBlockAtPosition(topRight) && IsBlockAtPosition(bottomLeft)) ||
                                                  (!IsBlockAtPosition(bottomRight) && IsBlockAtPosition(topLeft)),
                    _ => false
                };

                // If conditions for T-Spin Mini are met.
                if (isTSpinMini && lastRotationWasKick && lastMoveWasRotation)
                    return clearedLines switch
                    {
                        0 => TSpinType.MINIZERO,
                        1 => TSpinType.MINISINGLE,
                        _ => TSpinType.NONE
                    };

                break;
        }

        return TSpinType.NONE;
    }

    private Vector2 GetGridPosition(Vector2 basePosition, Vector2 partPosition)
    {
        var worldPosition =
            basePosition + partPosition * _cellSize + new Vector2(_cellSize / 2.0f, _cellSize / 2.0f);
        var gridPosition = _grid.ScreenToGridPosition(worldPosition);

        return gridPosition;
    }

    private bool IsBlockAtPosition(Vector2 position)
    {
        // Only check if there's a block at the given position.
        // Do not check for out-of-bounds.

        // Ensure the position isn't out-of-bounds.
        if (position.X < 0 || position.X >= _columns || position.Y + 2 < 0 ||
            position.Y + 2 >= _rows) return true; // Out-of-bounds positions are considered "occupied".

        return _grid.GetCurrentGrid()[(int)position.Y + 2, (int)position.X].IsOccupied;
    }
}