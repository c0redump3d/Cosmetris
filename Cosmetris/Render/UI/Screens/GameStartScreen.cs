/*
 * GameStartScreen.cs is part of Cosmetris.
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
using Cosmetris.Game.GameModes;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Controls.Animation;
using Cosmetris.Render.UI.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Screens;

public class GameStartScreen : Screen
{
    private readonly List<GameMode> _gameModes = new()
    {
        new GameMode("Survival", "Clear as many lines as you can before the grid fills up!"),
        new FortyLineMode("40 Line Mode", "Clear 40 lines as fast as you can to win!"),
        new TimeAttackMode("Time Attack", "Clear as many lines as you can in 3 minutes!"),
        new PunisherMode("Punisher Mode",
            "Aim for high line clears, t-spins, and perfect clears to avoid punishment. The lower the lines cleared, the higher chance of punishment!")
    };

    private readonly Font _largeFont = FontRenderer.Instance.GetFont("orbitron", 48);
    private readonly Font _mediumFont = FontRenderer.Instance.GetFont("orbitron", 28);
    private Label _gameModeLabel;
    private bool _ghostPieceEnabled = true;
    private Button _ghostPieceEnabledButton;

    private Slider _gravityMultiplierSlider;
    private bool _holdEnabled = true;

    private Button _holdEnabledButton;
    private Slider _lockDelaySlider;
    private Label _objectiveLabel;

    private Panel _panel;

    private GameMode _selectedGameMode;

    public GameStartScreen()
    {
        _selectedGameMode = _gameModes[0];

        CreateControls();
        LayoutControls += Relayout;
        LayoutControls.Invoke();

        base.OnInit();
    }

    private void CreateControls()
    {
        // Create controls ONCE and add to the screen or parent control
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f - 50);
        var panelSize = new Vector2(650, 650);
        _panel = new Panel(panelPosition, panelSize);

        // Create labels

        var title = new Label("Game Configuration", new Vector2(panelSize.X / 2f, 15), _largeFont,
            Microsoft.Xna.Framework.Color.White, Label.Align.Center);
        
        var divider = new Divider(new Vector2((panelSize.X / 2f), 85), new Vector2(panelSize.X - 125, 2),
            Microsoft.Xna.Framework.Color.DarkGray * 0.5f);

        _panel.AddControl(title);
        _panel.AddControl(divider);

        _gameModeLabel = new Label(_selectedGameMode.Name, new Vector2(panelSize.X / 2f, 100), _largeFont,
            Microsoft.Xna.Framework.Color.Crimson, Label.Align.Center);

        _objectiveLabel = new Label(_selectedGameMode.Objective, new Vector2(panelSize.X / 2f, 150), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center, maxLineWidth: 600);

        var menuButton = new Button("Menu", panelSize.X / 2f, panelSize.Y + 100,
            (s, o) => Window.Instance.ScreenRenderer().SetScreen(new MainMenuScreen()), _largeFont);

        _panel.AddControl(menuButton);

        var startButton = new Button("Start", panelSize.X / 2f, panelSize.Y + 25,
            (s, o) =>
            {
                _panel.SetClosingAnimation(new ScaleClosingAnimation(_panel, _panel.GetFinalSize(), Vector2.One, 0.85f));
                
                Window.Instance.ScreenRenderer().SetScreen(new InGameScreen(_selectedGameMode,
                    _gravityMultiplierSlider.Value, _lockDelaySlider.Value, _holdEnabled, _ghostPieceEnabled));
                
                Window.Instance.GetSoundManager().PlaySFX("start");
            },
            _largeFont);

        _panel.AddControl(startButton);

        var leftButton = new Button("<", panelSize.X / 2f - 100, panelSize.Y + 50,
            (s, o) =>
            {
                var index = _gameModes.IndexOf(_selectedGameMode);
                index--;
                if (index < 0) index = _gameModes.Count - 1;
                _selectedGameMode = _gameModes[index];
                _gameModeLabel.SetText(_selectedGameMode.Name);
                _objectiveLabel.SetText(_selectedGameMode.Objective);
            }, _mediumFont);

        _panel.AddControl(leftButton);

        var rightButton = new Button(">", panelSize.X / 2f + 100, panelSize.Y + 50,
            (s, o) =>
            {
                var index = _gameModes.IndexOf(_selectedGameMode);
                index++;
                if (index >= _gameModes.Count) index = 0;
                _selectedGameMode = _gameModes[index];
                _gameModeLabel.SetText(_selectedGameMode.Name);
                _objectiveLabel.SetText(_selectedGameMode.Objective);
            }, _mediumFont);

        _panel.AddControl(rightButton);

        _panel.AddControl(_gameModeLabel);
        _panel.AddControl(_objectiveLabel);
        _objectiveLabel.SetText(_selectedGameMode.Objective);

        // extra game mode modifiers
        var modifiersLabel = new Label("Modifiers", new Vector2(panelSize.X / 2f, 250), _mediumFont,
            Microsoft.Xna.Framework.Color.White, Label.Align.Center);

        _panel.AddControl(modifiersLabel);

        // gravity multiplier

        var gravityMultiplierLabel = new Label("Gravity Multiplier", new Vector2(panelSize.X / 2f, 290), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

        _panel.AddControl(gravityMultiplierLabel);

        _gravityMultiplierSlider = new Slider(
            new Vector2(panelSize.X / 2f,
                330),
            new Vector2(300, 40),
            Slider.SliderType.Line, 0.5f, 2f, 1f, Slider.ValueFormat.Float,
            "orbitron", 24, 15);

        _panel.AddControl(_gravityMultiplierSlider);

        // lock delay

        var lockDelayLabel = new Label("Lock Delay (ms)", new Vector2(panelSize.X / 2f, 420), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

        _panel.AddControl(lockDelayLabel);

        _lockDelaySlider = new Slider(
            new Vector2(panelSize.X / 2f,
                460),
            new Vector2(300, 40),
            Slider.SliderType.Line, 0f, 1000f, 500f, Slider.ValueFormat.Int,
            "orbitron", 24, 20);

        _panel.AddControl(_lockDelaySlider);

        CreateHoldEnabledButton();

        CreateGhostPieceEnabledButton();

        AddControl(_panel);
    }

    private void CreateHoldEnabledButton()
    {
        _holdEnabledButton = new Button("Enabled", _panel.Size.X / 2f + 450, 590,
            (s, o) =>
            {
                _holdEnabledButton.Text = _holdEnabledButton.Text.Equals("Enabled") ? "Disabled" : "Enabled";
                _holdEnabled = !_holdEnabled;
            }, _mediumFont);

        var holdEnabledLabel = new Label("Hold Piece", new Vector2(_panel.Size.X / 2f + 450, 540), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

        _panel.AddControl(_holdEnabledButton);
        _panel.AddControl(holdEnabledLabel);
    }

    private void CreateGhostPieceEnabledButton()
    {
        _ghostPieceEnabledButton = new Button("Enabled", _panel.Size.X / 2f + 200, 590,
            (s, o) =>
            {
                _ghostPieceEnabledButton.Text =
                    _ghostPieceEnabledButton.Text.Equals("Enabled") ? "Disabled" : "Enabled";
                _ghostPieceEnabled = !_ghostPieceEnabled;
            }, _mediumFont);

        var ghostPieceEnabledLabel = new Label("Ghost Piece", new Vector2(_panel.Size.X / 2f + 200, 540), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center);

        _panel.AddControl(_ghostPieceEnabledButton);
        _panel.AddControl(ghostPieceEnabledLabel);
    }

    private void Relayout()
    {
        // Recenter the panel
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f - 50);
        var panelSize = new Vector2(650, 650);

        _panel.Size = panelSize;
        _panel.Position = panelPosition - panelSize / 2f;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}