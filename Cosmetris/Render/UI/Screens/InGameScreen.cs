/*
 * InGameScreen.cs is part of Cosmetris.
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
using Cosmetris.Game;
using Cosmetris.Game.GameModes;
using Cosmetris.Game.Grid;
using Cosmetris.Game.Objects;
using Cosmetris.Input;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Util.Background;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cosmetris.Util;

namespace Cosmetris.Render.UI.Screens;

public class InGameScreen : Screen
{
    private static readonly Font _font = FontRenderer.Instance.GetFont("orbitron", 24);
    private TetrisGameManager _gameManager;
    private readonly bool _ghostPieceEnabled;

    private readonly float _gravityMultiplier;
    private TetrisGrid _grid;
    private readonly bool _holdEnabled;
    private readonly float _lockDelay;
    
    private bool _countDownFinished;

    private Panel _pausePanel;

    public InGameScreen(GameMode gameMode, float gravityMultiplier, float lockDelay, bool holdEnabled,
        bool ghostPieceEnabled)
    {
        _gravityMultiplier = gravityMultiplier;
        _lockDelay = lockDelay;
        _holdEnabled = holdEnabled;
        _ghostPieceEnabled = ghostPieceEnabled;

        CreateControls(gameMode);
        LayoutControls += Relayout;

        AddControl(_grid);
        Controller.Instance.OnButtonPress += OnButtonPress;

        CosmonoRain.Instance.HideRain();

        LayoutControls.Invoke();

        base.OnInit();
    }

    private void CreateControls(GameMode gameMode)
    {
        var rows = 20;
        var columns = 10;
        var cellSize = 48;
        var lineWidth = 1;
        var borderWidth = 4;

        _grid = new TetrisGrid(rows, columns, cellSize, lineWidth, borderWidth);
        _gameManager = new TetrisGameManager(gameMode, _grid, _gravityMultiplier, _lockDelay, _holdEnabled,
            _ghostPieceEnabled);
        _grid.SetGameManager(_gameManager);

        ResetPausePanel();
    }

   
    
    private void ResetPausePanel()
    {
        _pausePanel = new Panel(new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f),
            new Vector2(350, 650));
        _pausePanel.IsImportant = true;
        var pauseLabel = new Label("Paused", new Vector2(175, 15), _font, Microsoft.Xna.Framework.Color.White,
            Label.Align.Center);
        _pausePanel.AddControl(pauseLabel);
        var resumeButton = new Button("Resume", 175, 100, (sender, vector2) => { HandlePause(Cosmetris.GameState); },
            _font);
        _pausePanel.AddControl(resumeButton);
        var quitButton = new Button("Quit", 175, 200, (sender, vector2) =>
        {
            var messageBox = new MessageBox("Exit Game?",
                "Are you sure you want to exit the current game?\n\nAll progress will be lost!",
                MessageBoxButton.YESNO);
            messageBox.OnButtonClick += (s, e) =>
            {
                if (e == MessageBoxButtonType.YES)
                {
                    Cosmetris.UpdateGameState(GameState.MainMenu);
                    Window.Instance.ScreenRenderer().SetScreen(new MainMenuScreen());
                }
            };
            AddControl(messageBox);
        }, _font);
        _pausePanel.AddControl(quitButton);
    }

    private void Relayout()
    {
        // Center the grid
        var center = new Vector2(ScalingManager.DesiredWidth / 2.0f, ScalingManager.DesiredHeight / 2.0f);
        _grid.Position = new Vector2(center.X - _grid.Size.X / 2,
            center.Y - _grid.Size.Y / 2);

        if (_gameManager.GhostPiece != null)
            _gameManager.UpdateGhostPiece();

        if (_pausePanel != null)
            _pausePanel.SetPosition(new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f));
    }

    private void OnButtonPress(object sender, Controller.ControllerButton e)
    {
        if (e.Name.Equals("Pause")) HandlePause(Cosmetris.GameState);
    }

    private void HandlePause(GameState gameState)
    {
        if (gameState == GameState.GameOver || _gameManager.InCountdown)
            return;

        if (gameState == GameState.Paused)
        {
            if (_pausePanel.IsClosing) return;
            
            AddConsoleMessage("Resuming game...");
            _pausePanel.StartClosing();
            _pausePanel.OnClose += (s, e) =>
            {
                _gameManager.CountdownComplete = null;
                _gameManager.CountdownComplete += (o, args) =>
                {
                    Cosmetris.UpdateGameState(GameState.InGame);
                    _gameManager.FreezeCosmono(false);
                    ResetPausePanel();
                };
                _gameManager.Countdown();
            };
        }
        else
        {
            AddConsoleMessage("Pausing game...");
            AddControl(_pausePanel);
            Cosmetris.UpdateGameState(GameState.Paused);
            Window.Instance.GetSoundManager().PlaySFX("popup");
            _gameManager.FreezeCosmono(true);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _gameManager.Draw(spriteBatch, gameTime);
        base.Draw(spriteBatch, gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        _gameManager.Update(gameTime);
        base.Update(gameTime);
    }

    public override void OnClose()
    {
        ObjectManager.Instance.Dispose();
        Controller.Instance.OnButtonPress -= OnButtonPress;
        base.OnClose();
    }
}