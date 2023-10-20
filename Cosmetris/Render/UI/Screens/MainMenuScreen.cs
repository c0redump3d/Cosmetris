/*
 * MainMenuScreen.cs is part of Cosmetris.
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

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmetris.Game;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Screens.Settings;
using Cosmetris.Render.UI.Text;
using Cosmetris.Util;
using Cosmetris.Util.Background;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cosmetris.Render.UI.Controls.Animation;

namespace Cosmetris.Render.UI.Screens;

public class MainMenuScreen : Screen
{
    private static bool _isInitialized;

    private static readonly Font _debugFont = FontRenderer.Instance.GetFont("debug", 18);
    private static readonly Font _titleFont = FontRenderer.Instance.GetFont("orbitron", 96);
    private static readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 64);
    private static readonly Font _mediumFont = FontRenderer.Instance.GetFont("orbitron", 48);
    private static Font _smallFont = FontRenderer.Instance.GetFont("orbitron", 32);

    private readonly Label _copyRightLabel;

    private readonly EffectsManager.FX _rainbow;

    private readonly string[] CopyrightText =
    {
        "Tetris (c) 1985~2021 Tetris Holding.", "Tetris logos, Tetris theme song and",
        "Tetriminos are trademarks of Tetris Holding.",
        "The Tetris trade dress is owned by Tetris Holding.", "Licensed to The Tetris Company.",
        "Tetris Game Design by Alexey Pajitnov.",
        "Tetris Logo Design by Roger Dean.", "All Rights Reserved."
    };

    private Button _exitButton;
    private Button _multiplayerButton;
    private Label _nameLabel;

    private Panel _panel;

    private Button _playGameButton;
    private Button _settingsButton;
    private Button _texturePackButton;
    private Label _versionLabel;
    private Label _creatorLabel;
    private Divider _creatorDivider;

    public MainMenuScreen()
    {
        _rainbow = EffectsManager.Instance.GetFX("rainbowgradient");

        // Create all controls ONCE outside LayoutControls
        CreateControls();

        LayoutControls += Relayout;

        if (!_isInitialized)
        {
            var pos = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
            _copyRightLabel = new Label("", pos, _mediumFont, Microsoft.Xna.Framework.Color.White, Label.Align.Center,
                scale: 0.95f);

            _copyRightLabel.SetLines(CopyrightText);

            _copyRightLabel.SetVerticalAlignment(Label.VerticalAlign.Center);

            AddControl(_copyRightLabel);
            LayoutControls.Invoke();

            Timer.Instance.CreateTimer(5000f, (sender, args) =>
            {
                _isInitialized = true;
                AddControl(_panel);
                _copyRightLabel.FadeOut(150f);
            });
        }
        else
        {
            LayoutControls.Invoke();
            AddControl(_panel);
        }

        CosmonoRain.Instance.ShowRain();
        Cosmetris.UpdateGameState(GameState.MainMenu);

        Window.Instance.GetSoundManager().PlayMusic("MainMenu");
    }

    private void CreateControls()
    {
        // Create controls ONCE and add to the screen or parent control
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(1200, 680);
        _panel = new Panel(panelPosition, panelSize);

        _playGameButton = new Button("Start Game", 0, 0,
            (sender, vector2) => Window.Instance.ScreenRenderer().SetScreen(new GameStartScreen()), _defaultFont);
        _settingsButton = new Button("Settings", 0, 0,
            (o, v) => Window.Instance.ScreenRenderer().SetScreen(new SettingsScreen()), _defaultFont);
        _texturePackButton = new Button("Texture Packs", 0, 0,
            (o, v) => Window.Instance.ScreenRenderer().SetScreen(new TexturePackScreen()), _defaultFont);
        _multiplayerButton = new Button("Multiplayer", 0, 0,
            (o, v) => { }, _defaultFont);
        _exitButton = new Button("Quit Game", 0, 0, (o, v) => Cosmetris.Instance.Exit(), _defaultFont);

        _creatorLabel = new Label("CKProductions Presents", Vector2.Zero, _smallFont, (Microsoft.Xna.Framework.Color.Gray * 0.35f),
            Label.Align.Center, Label.Overflow.Wrap, 1f, 0);
        _creatorLabel.OnClick += (sender, vector2) =>
        {
            // open url in system browser
            ContentUtil.Instance.OpenUrl("https://ckproductions.dev/");
        };
        _creatorDivider = new Divider(Vector2.Zero, new Vector2(385, 2), Microsoft.Xna.Framework.Color.Gray * 0.45f);
        _nameLabel = new Label(Window.GameName.ToUpper(), Vector2.Zero, _titleFont, Microsoft.Xna.Framework.Color.White,
            Label.Align.Center, Label.Overflow.Wrap, 1f, 0, _rainbow.Effect);
        _versionLabel = new Label("v" + Window.GameVersion, Vector2.Zero, _mediumFont,
            Microsoft.Xna.Framework.Color.White, Label.Align.Center, Label.Overflow.Wrap, 1f, 0, _rainbow.Effect);
        // _versionLabel.OnClick += (sender, args) =>
        // {
        //     var messageBox = new MessageBox("Update Changes:",
        //         new string[] {"- Color Picker added to Settings","- High scores are now saved","- Added a countdown when starting and un-pausing the game.","- Other minor bug fixes and improvements."},
        //         MessageBoxButton.OK);
        //     
        //     AddControl(messageBox);
        // };

        // Add them to the screen or parent control
        _panel.AddControl(_playGameButton);
        _panel.AddControl(_texturePackButton);
        _panel.AddControl(_multiplayerButton);
        _panel.AddControl(_settingsButton);
        _panel.AddControl(_exitButton);
        _panel.AddControl(_creatorLabel);
        _panel.AddControl(_creatorDivider);
        _panel.AddControl(_nameLabel);
        _panel.AddControl(_versionLabel);
    }

    public void Relayout()
    {
        // Only update positions/sizes of controls here
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(1200, 680);

        _panel.Size = panelSize;
        _panel.Position = panelPosition - panelSize / 2f;

        var y = 100;

        _multiplayerButton.Enabled = true;
        _playGameButton.SetPosition(853, y);
        y += (int)_playGameButton.Size.Y + 25;
        _multiplayerButton.SetPosition(853, y);
        y += (int)_multiplayerButton.Size.Y + 25;
        _settingsButton.SetPosition(853, y);
        y += (int)_settingsButton.Size.Y + 25;
        _texturePackButton.SetPosition(853, y);
        y += (int)_texturePackButton.Size.Y + 25;
        _exitButton.SetPosition(853, y);
        _multiplayerButton.Enabled = false;

        _creatorLabel.SetPosition(new Vector2(325, 215));
        _creatorDivider.SetPosition(new Vector2(320, 250));
        _nameLabel.SetPosition(new Vector2(325, 245));
        _versionLabel.SetPosition(new Vector2(325, 340));

        if (!_isInitialized)
        {
            _copyRightLabel.SetPosition(new Vector2(ScalingManager.DesiredWidth / 2f,
                ScalingManager.DesiredHeight / 2f));
            _copyRightLabel.SetVerticalAlignment(Label.VerticalAlign.Center);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Show Window w/h vs Screen w/h
        if (Window.Instance.ScreenRenderer().IsDebugEnabled())
        {
            _debugFont.DrawLabel($"Renderer Resolution: {ScalingManager.DesiredWidth}x{ScalingManager.DesiredHeight}",
                0f,
                0f, Microsoft.Xna.Framework.Color.White);
            _debugFont.DrawLabel($"Window Resolution: {ScalingManager.ActualWidth}x{ScalingManager.ActualHeight}", 0f,
                20f,
                Microsoft.Xna.Framework.Color.White);
            _debugFont.DrawLabel($"UI Scale: {ScalingManager.WidthScaleFactor}x{ScalingManager.HeightScaleFactor}", 0f,
                40f,
                Microsoft.Xna.Framework.Color.White);
        }

        base.Draw(spriteBatch, gameTime);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void OnResize()
    {
        base.OnResize();
    }
}