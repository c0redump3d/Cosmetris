/*
 * Score.cs is part of Cosmetris.
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
using System.Xml.Serialization;
using Cosmetris.Game.Grid.Util;
using Cosmetris.Render;
using Microsoft.Xna.Framework;

namespace Cosmetris.Game.Objects.Cosmonoes.Util;

/// <summary>
///     The score class of a parent Cosmono.
/// </summary>
public class Score
{
    private int _currentCombo;
    private readonly TetrisGameManager _gameManager;
    private int _linesAtLastLevelUp;
    
    public Score() { }

    public Score(TetrisGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    /// <summary>
    ///     The total number of lines cleared.
    /// </summary>
    public int LinesCleared { get; set; }

    /// <summary>
    ///     The number of T-Spin Mini's performed.
    /// </summary>
    public int TSpinMini { get; set; }

    /// <summary>
    ///     The number of T-Spin's performed.
    /// </summary>
    public int TSpin { get; set; }

    /// <summary>
    ///     The number of Back-to-Back's performed.
    /// </summary>
    public int BackToBack { get; set; }

    /// <summary>
    ///     The number of Combo's performed.
    /// </summary>
    public int Combo { get; set; }

    /// <summary>
    ///     The number of Soft Drops performed.
    /// </summary>
    public int SoftDrop { get; set; }

    /// <summary>
    ///     The number of Hard Drops performed.
    /// </summary>
    public int HardDrop { get; set; }

    /// <summary>
    ///     The number of Perfect Clears performed.
    /// </summary>
    public int PerfectClear { get; set; }

    /// <summary>
    ///     The current score.
    /// </summary>
    public int ScoreValue { get; set; }

    /// <summary>
    ///     The current level.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    ///     Whether or not the last move was a Back-to-Back.
    /// </summary>
    [XmlIgnore]
    public bool LastWasBackToBack { get; set; }

    /// <summary>
    ///     Handle the lines cleared of the parent Cosmono.
    /// </summary>
    /// <param name="cleared">The number of lines cleared.</param>
    /// <param name="wasTSpin">Whether or not the last move was a T-Spin.</param>
    public void HandleLinesCleared(int cleared, bool wasTSpin)
    {
        // Update the total LinesCleared
        LinesCleared += cleared;

        if (cleared == 0)
        {
            _currentCombo = 0; // Reset the combo if no lines were cleared
        }
        else
        {
            _currentCombo++; // Increase the combo counter for cleared lines
            if (_currentCombo > 1)
                HandleCombo(_currentCombo - 1); // We subtract one because the first line clear isn't part of a combo
        }

        switch (cleared)
        {
            case 1:
                ScoreValue += 100 * Level;
                if (!wasTSpin)
                    Window.Instance.GetSoundManager().PlaySFX("clear");
                break;
            case 2:
                ScoreValue += 300 * Level;
                if (!wasTSpin)
                {
                    _gameManager.GetGrid().CreateScoreLabel("Double!", Color.BlueViolet);
                    Window.Instance.GetSoundManager().PlaySFX("linedouble");
                }

                break;
            case 3:
                ScoreValue += 500 * Level;
                if (!wasTSpin)
                {
                    _gameManager.GetGrid().CreateScoreLabel("Triple!", Color.Purple);
                    Window.Instance.GetSoundManager().PlaySFX("linetriple");
                }

                break;
            case 4:
                ScoreValue += 800 * Level;
                if (!wasTSpin)
                {
                    _gameManager.GetGrid().CreateScoreLabel("Tetris!", Color.Crimson);
                    Window.Instance.GetSoundManager().PlaySFX("linetetris");
                }

                break;
        }
    }

    /// <summary>
    ///     Handle T-Spin scoring and back-to-back bonuses of the parent Cosmono.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cleared"></param>
    public void HandleTSpin(TSpinType type, int cleared)
    {
        if (cleared <= 0 && type == TSpinType.NONE)
        {
            LastWasBackToBack = false;
            return;
        }

        if (LastWasBackToBack && type != TSpinType.NONE && type != TSpinType.MINIZERO)
        {
            // Reward player for back-to-back t-spin
            HandleBackToBack();
            BackToBack++;
        }

        switch (type)
        {
            case TSpinType.MINIZERO:
                ScoreValue += 100 * Level;
                TSpinMini++;
                _gameManager.GetGrid().CreateScoreLabel("T-Spin Mini!", Color.Cyan);
                Window.Instance.GetSoundManager().PlaySFX("tspin0");
                break;
            case TSpinType.MINISINGLE:
                ScoreValue += 200 * Level;
                TSpinMini++;
                _gameManager.GetGrid().CreateScoreLabel("T-Spin Mini Single!", Color.BlueViolet);
                Window.Instance.GetSoundManager().PlaySFX("tspin1");
                break;
            case TSpinType.ONE:
                ScoreValue += 800 * Level;
                TSpin++;
                _gameManager.GetGrid().CreateScoreLabel("T-Spin Single!", Color.Navy);
                Window.Instance.GetSoundManager().PlaySFX("tspin1");
                break;
            case TSpinType.TWO:
                ScoreValue += 1200 * Level;
                TSpin++;
                _gameManager.GetGrid().CreateScoreLabel("T-Spin Double!", Color.CornflowerBlue);
                Window.Instance.GetSoundManager().PlaySFX("tspin2");
                break;
            case TSpinType.THREE:
                ScoreValue += 1600 * Level;
                TSpin++;
                _gameManager.GetGrid().CreateScoreLabel("T-Spin Triple!", Color.RoyalBlue);
                Window.Instance.GetSoundManager().PlaySFX("tspin3");
                break;
            case TSpinType.NONE:
                LastWasBackToBack = false;
                break;
        }

        // Check if the last move was a T-Spin or a Tetris to handle back-to-back bonus
        if (type != TSpinType.NONE && cleared > 0)
            LastWasBackToBack = true;
        else
            LastWasBackToBack = false;
    }

    /// <summary>
    ///     Handle the back-to-back bonus of the parent Cosmono.
    /// </summary>
    public void HandleBackToBack()
    {
        _gameManager.GetGrid().CreateScoreLabel("Back-to-Back!", Color.Gold);
        ScoreValue += 100 * Level;
    }

    /// <summary>
    ///     Handle the soft drop score of the parent Cosmono.
    /// </summary>
    public void HandleSoftDrop()
    {
        ScoreValue += 1 * Level;
        SoftDrop++;
    }

    /// <summary>
    ///     Handle the hard drop score of the parent Cosmono.
    /// </summary>
    public void HandleHardDrop()
    {
        ScoreValue += 2 * Level;
        HardDrop++;
    }

    /// <summary>
    ///     Handle the combo score of the parent Cosmono.
    /// </summary>
    /// <param name="combo">The current combo.</param>
    public void HandleCombo(int combo)
    {
        _gameManager.GetGrid().CreateScoreLabel($"{combo}x Combo!", Color.Yellow);
        ScoreValue += 50 * combo * Level;
        Combo++;
    }

    /// <summary>
    ///     Check if the parent Cosmono has leveled up.
    /// </summary>
    /// <returns>Whether or not the parent Cosmono has leveled up.</returns>
    public bool CheckForLevelUp()
    {
        // This checks if 10 lines have been cleared since the last level up.
        if (LinesCleared - _linesAtLastLevelUp >= 10)
        {
            LevelUp();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Increase the level of the parent Cosmono.
    /// </summary>
    private void LevelUp()
    {
        Level++;
        _linesAtLastLevelUp = LinesCleared;

        _gameManager.GetGrid().CreateMessageLabel("Level Up!", Color.GreenYellow);

        // Certain levels change the in-game music.
        if (Level == 5)
            Window.Instance.GetSoundManager().PlayMusic("MusicL5-Loop", "warning5");
        else if (Level == 8)
            Window.Instance.GetSoundManager().PlayMusicSpecial("MusicL8-Start", "MusicL8-Loop", "warning8");
    }

    /// <summary>
    ///     Handle the perfect clear score of the parent Cosmono.
    /// </summary>
    public void HandlePerfectClear()
    {
        _gameManager.GetGrid().CreateScoreLabel("Perfect Clear!", Color.Gold);
        ScoreValue += 1000 * Level;
        PerfectClear++;
        Window.Instance.GetSoundManager().PlaySFX("lineperfect");
    }

    /// <summary>
    ///     Get the score list of the parent Cosmono.
    /// </summary>
    /// <returns>The score name and value of the parent Cosmono.</returns>
    public Dictionary<string, int> GetScoreList()
    {
        return new Dictionary<string, int>
        {
            { "Lines Cleared", LinesCleared },
            { "T-Spin Mini", TSpinMini },
            { "T-Spin", TSpin },
            { "Back-to-Back", BackToBack },
            { "Combo", Combo },
            { "Soft Drop", SoftDrop },
            { "Hard Drop", HardDrop },
            { "Perfect Clear", PerfectClear },
            { "Score", ScoreValue },
            { "Level", Level }
        };
    }
}