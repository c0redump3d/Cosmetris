/*
 * BoardLoader.cs is part of Cosmetris.
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
using Cosmetris.Game.Grid.Util.Development;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Grid.Util;

public class BoardLoader
{
    private readonly int _columns;
    private readonly TetrisGrid _grid;
    private readonly int _rows;

    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    private readonly Texture2D _blockTexture;

    private int _lastDevScreenHash;

    public BoardLoader(TetrisGrid grid)
    {
        _grid = grid;
        _columns = grid.TotalColumns();
        _rows = grid.TotalRows();

        TextureManager.Instance.AddTexture("x_block", "Block/x_block");

        _blockTexture = TextureManager.Instance.GetTexture("x_block").Texture2D;
    }

    private Cell[,] LoadTestBoard(DevBoard testBoard)
    {
        if (testBoard.Board.GetLength(1) != _columns)
            throw new ArgumentException("Test board width does not match the TetrisGrid width.");

        var cells = _grid.GetCurrentGrid();

        // Calculate the starting row from where to fill the test board data
        var startRow = _rows - testBoard.Board.GetLength(0);

        // Clear/fill the top rows with empty cells
        for (var row = 0; row < startRow; row++)
        for (var col = 0; col < _columns; col++)
        {
            cells[row, col].IsOccupied = false;
            cells[row, col].Texture = null;
        }

        // Fill the bottom rows with test board data
        for (var row = startRow; row < _rows; row++)
        for (var col = 0; col < _columns; col++)
        {
            var occupied = testBoard.Board[row - startRow, col] == 1;
            cells[row, col].IsOccupied = occupied;
            cells[row, col].Texture = occupied ? _blockTexture : null;
            cells[row, col].Color =
                occupied ? Color.White : Color.Transparent;
        }

        return cells;
    }

    public Cell[,] CycleToNextDevBoard()
    {
        var nextBoard = DevBoardManager.GetNextBoard();
        var board = LoadTestBoard(nextBoard);

        var devLabel = DevBoardManager.GetBoardNameLabel();

        if (devLabel != null)
        {
            var name = nextBoard.Name;
            devLabel.SetText(name);
            var font = DevBoardManager.GetDebugFont();
            var nameLength = font.MeasureString(name);
            var pos = _scalingManager.DesiredWidth - _scalingManager.GetScaledX(nameLength.X);
            devLabel.Position = new Vector2(pos, 5);

            DevBoardManager.SetBoardNameLabel(devLabel);
        }

        if (_lastDevScreenHash == Window.Instance.ScreenRenderer().GetScreen().GetHashCode())
            return null;

        _lastDevScreenHash = Window.Instance.ScreenRenderer().GetScreen().GetHashCode();
        DevBoardManager.CreateDevControls();

        return board;
    }
}