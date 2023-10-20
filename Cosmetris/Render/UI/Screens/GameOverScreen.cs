/*
 * GameOverScreen.cs is part of Cosmetris.
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

using Cosmetris.Game;
using Cosmetris.Game.Objects;
using Cosmetris.Game.Objects.Cosmonoes.Util;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Screens;

public class GameOverScreen : Screen
{
    private readonly TetrisGameManager _gameManager;
    private readonly Font _largeFont = FontRenderer.Instance.GetFont("orbitron", 48);

    private readonly Font _mediumFont = FontRenderer.Instance.GetFont("orbitron", 28);
    private Panel _panel;

    public GameOverScreen(TetrisGameManager gameManager)
    {
        _gameManager = gameManager;

        CreateControls();
        LayoutControls += Relayout;
        LayoutControls.Invoke();

        Window.Instance.GetSoundManager().PlayMusic("MainMenu");

        var score = _gameManager.CurrentCosmono().GetScore();
        var oldScore = GameSettings.Instance.GetValue<Score>("High Scores",$"{_gameManager.ActiveGameMode.Name} High Score");

        if (oldScore != null)
        {
            if (score.ScoreValue > oldScore.ScoreValue)
            {
                GameSettings.Instance.SetValue("High Scores", $"{_gameManager.ActiveGameMode.Name} High Score", score);
            }
        }
        else
        {
            GameSettings.Instance.SetValue("High Scores", $"{_gameManager.ActiveGameMode.Name} High Score", score);
        }

        GameSettings.Instance.Save();
        
        base.OnInit();
    }

    private void CreateControls()
    {
        // Create controls ONCE and add to the screen or parent control
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(650, 650);
        _panel = new Panel(panelPosition, panelSize);

        // Gather scores
        var scores = _gameManager.GetScoreList();

        // Create labels

        var title = new Label("Game Over", new Vector2(panelSize.X / 2f, 15), _largeFont,
            Microsoft.Xna.Framework.Color.Crimson, Label.Align.Center);

        _panel.AddControl(title);

        string niceTime;
        if (_gameManager.ActiveGameMode.GameTimer.ElapsedTime < 60)
        {
            niceTime = $"{_gameManager.ActiveGameMode.GameTimer.ElapsedTime:n2} seconds";
        }
        else
        {
            var minutes = (int)(_gameManager.ActiveGameMode.GameTimer.ElapsedTime / 60);
            var seconds = (int)(_gameManager.ActiveGameMode.GameTimer.ElapsedTime % 60);
            niceTime = $"{minutes}:{seconds:00}";
        }

        var timeElapsed = new Label($"Time Elapsed: {niceTime}",
            new Vector2(panelSize.X / 2f, 75), _mediumFont, Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

        _panel.AddControl(timeElapsed);

        var x = panelSize.X / 2f;
        var y = panelSize.Y / 2f + 50 - scores.Count * 25;

        foreach (var score in scores)
        {
            var pos = new Vector2(x, y);
            var label = new Label($"{score.Key}: {score.Value:n0}", pos, _mediumFont,
                Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

            y += 50;
            _panel.AddControl(label);
        }

        var menuButton = new Button("Menu", panelSize.X / 2f, panelSize.Y + 50,
            (s, o) => Window.Instance.ScreenRenderer().SetScreen(new MainMenuScreen()), _mediumFont);

        _panel.AddControl(menuButton);

        AddControl(_panel);
    }

    private void Relayout()
    {
        // Recenter the panel
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(650, 650);

        _panel.Size = panelSize;
        _panel.Position = panelPosition - panelSize / 2f;
    }

    public override void OnClose()
    {
        ObjectManager.Instance.Dispose();
        base.OnClose();
    }
}