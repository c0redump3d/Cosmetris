/*
 * DevBoard.cs is part of Cosmetris.
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

namespace Cosmetris.Game.Grid.Util.Development;

/// <summary>
///     A pre-defined board for testing purposes.
/// </summary>
public struct DevBoard
{
    public readonly string Name;
    public readonly int[,] Board;

    public DevBoard(string name, int[,] board)
    {
        Name = name;
        Board = board;
    }
}

/// <summary>
///     A collection of pre-defined boards for testing purposes.
/// </summary>
public static class DevBoards
{
    // This board is set up for a T-spin double test.
    public static readonly DevBoard TSpinDoubleSetup = new(
        "TSpinDoubleSetup",
        new[,]
        {
            { 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 },
            { 1, 1, 1, 1, 0, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 0, 1, 1, 1, 1 }
        });

    // This board is set up for testing wall kicks.
    public static readonly DevBoard WallKickTest = new(
        "WallKickTest",
        new[,]
        {
            { 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // This board is set up for a T-spin single test.
    public static readonly DevBoard TSpinSingleSetup1 = new(
        "TSpinSingleSetup1",
        new[,]
        {
            { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 }
        });

    // Test for T-spin triple setup (though T-spin triples aren't officially part of the guideline)
    public static readonly DevBoard TSpinTwoCorner = new(
        "TSpinCorner",
        new[,]
        {
            { 1, 1, 1, 1, 1, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 }
        });

    // I-piece vertical wall kick test 1
    public static readonly DevBoard ITwistVertical1 = new(
        "ITwistVertical1",
        new[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }
        });


    // I-piece vertical wall kick test2
    public static readonly DevBoard ITwistVertical2 = new(
        "ITwistVertical2",
        new[,]
        {
            { 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 }
        });


    // I-piece horizontal wall kick test
    public static readonly DevBoard ITwistHorizontal = new(
        "ITwistHorizontal",
        new[,]
        {
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // J-piece  twist test
    public static readonly DevBoard JTwist = new(
        "JTwist",
        new[,]
        {
            { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 }
        });


    // L-piece twist test
    public static readonly DevBoard JTwistSpecial = new(
        "JTwistSpecial",
        new[,]
        {
            { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 0, 1, 1, 1, 1 }
        });

    // L-piece twist test
    public static readonly DevBoard LTwistSpecial = new(
        "LTwistSpecial",
        new[,]
        {
            { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 0, 1, 0, 0, 0, 0 }
        });

    // L-piece wall kick test
    public static readonly DevBoard LKick = new(
        "LKick",
        new[,]
        {
            { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // Z-piece twist test
    public static readonly DevBoard ZTwist = new(
        "ZTwist",
        new[,]
        {
            { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 },
            { 1, 1, 0, 0, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1 }
        });

    // S-piece twist test
    public static readonly DevBoard STwist = new(
        "STwist",
        new[,]
        {
            { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 0, 0, 1, 1 },
            { 1, 1, 1, 1, 1, 0, 0, 1, 1, 1 }
        });

    // O-piece movement test (mostly ensuring it can move or slide correctly)
    public static readonly DevBoard OPieceTest = new(
        "OPieceTest",
        new[,]
        {
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 1, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // T-spin mini test
    public static readonly DevBoard TSpinMiniTest = new(
        "TSpinMiniTest",
        new[,]
        {
            { 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 1, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // Floor Kick Test for L, J, T, S, Z pieces
    public static readonly DevBoard FloorKickTest = new(
        "FloorKickTest",
        new[,]
        {
            { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        });

    // Overhang Rotation Test
    public static readonly DevBoard OverhangRotationTest = new(
        "OverhangRotationTest",
        new[,]
        {
            { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        }
    );

    // Perfect Clear Setup
    public static readonly DevBoard PerfectClearSetup = new(
        "PerfectClearSetup",
        new[,]
        {
            { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1 }
        }
    );

    // Full Board
    public static readonly DevBoard FullBoard = new(
        "FullBoard",
        new[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        }
    );

    // Single Hole
    public static readonly DevBoard SingleHole = new(
        "SingleHole",
        new[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        }
    );

    // Alternating Gaps
    public static readonly DevBoard AlternatingGaps = new(
        "AlternatingGaps",
        new[,]
        {
            { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 }
        }
    );

    // Diagonal Gaps
    public static readonly DevBoard DiagonalGaps = new(
        "DiagonalGaps",
        new[,]
        {
            { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 }
        }
    );

    public static readonly DevBoard PerfectClear = new(
        "PerfectClear",
        new[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 }
        }
    );
}