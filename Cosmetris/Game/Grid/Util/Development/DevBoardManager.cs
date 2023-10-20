/*
 * DevBoardManager.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Microsoft.Xna.Framework;

namespace Cosmetris.Game.Grid.Util.Development;

public static class DevBoardManager
{
    private static int _currentIndex;
    private static bool _devControlsCreated;
    private static Label _boardNameLabel;
    private static Font _debugFont;

    private static readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;

    private static readonly List<DevBoard> Boards = new()
    {
        DevBoards.PerfectClear,
        DevBoards.JTwist,
        DevBoards.JTwistSpecial,
        DevBoards.LTwistSpecial,
        DevBoards.LKick,
        DevBoards.STwist,
        DevBoards.ZTwist,
        DevBoards.FloorKickTest,
        DevBoards.ITwistHorizontal,
        DevBoards.ITwistVertical1,
        DevBoards.ITwistVertical2,
        DevBoards.OPieceTest,
        DevBoards.OverhangRotationTest,
        DevBoards.WallKickTest,
        DevBoards.TSpinDoubleSetup,
        DevBoards.TSpinMiniTest,
        DevBoards.TSpinSingleSetup1,
        DevBoards.TSpinTwoCorner,
        DevBoards.AlternatingGaps,
        DevBoards.DiagonalGaps,
        DevBoards.PerfectClearSetup,
        DevBoards.SingleHole
        // ... (all the other dev boards)
    };

    public static bool CreateDevControls()
    {
        _devControlsCreated = true;

        // Load Debug font if null
        if (_debugFont == null)
            _debugFont = FontRenderer.Instance.GetFont("debug", 18);

        // Create a simple label displaying the current board name.
        var name = GetCurrentBoard().Name;
        var length = _debugFont.MeasureString(name);
        _boardNameLabel = new Label(name,
            new Vector2(_scalingManager.DesiredWidth - _scalingManager.GetScaledX(length.X), 5), _debugFont,
            Color.White);


        Window.Instance.ScreenRenderer().GetScreen().GetControls().Add(_boardNameLabel);

        return true;
    }

    private static DevBoard GetCurrentBoard()
    {
        var curBoard = Boards[_currentIndex];
        return curBoard;
    }

    public static Label GetBoardNameLabel()
    {
        return _boardNameLabel;
    }

    public static Font GetDebugFont()
    {
        return _debugFont;
    }

    public static DevBoard GetNextBoard()
    {
        _currentIndex = (_currentIndex + 1) % Boards.Count;

        var nextBoard = GetCurrentBoard();

        return nextBoard;
    }

    public static void SetBoardNameLabel(Label devLabel)
    {
        _boardNameLabel = devLabel;
    }
}