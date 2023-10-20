/*
 * TexturePackScreen.cs is part of Cosmetris.
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
using System.Collections.Generic;
using Cosmetris.Game;
using Cosmetris.Game.Packs.TexturePacks;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Screens;

public class TexturePackScreen : Screen
{
    private const int PacksPerPage = 3; // Change this to fit your layout
    private static readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 48);
    private static readonly Font _smallFont = FontRenderer.Instance.GetFont("orbitron", 24);
    private Button _backButton;

    private TexturePack _currentPack;
    private int _currentPage;

    private Button _lastClickedButton;

    private Button _nextPageButton;

    private Panel _panel;
    private Button _previousPageButton;
    private List<TexturePack> _texturePacks;

    public TexturePackScreen()
    {
        CreateControls();
        LayoutControls += Relayout;

        Relayout();

        LoadTexturePacks();
        UpdatePage();

        Cosmetris.UpdateGameState(GameState.TexturePacks);
    }

    private void LoadTexturePacks()
    {
        _texturePacks = Window.Instance.PackManager.LoadAllAvailableTexturePacks();

        var currentPackGuid = GameSettings.Instance.GetValue<string>("Strings", "Current Texture Pack");
        if (currentPackGuid != null) _currentPack = _texturePacks.Find(p => p.PackMD5() == currentPackGuid);
    }

    private void CreateControls()
    {
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(1280, 720);
        _panel = new Panel(panelPosition, panelSize);

        _nextPageButton = new Button("Next", 0, 0, (o, v) => ChangePage(1), _defaultFont);
        _previousPageButton = new Button("Previous", 0, 0, (o, v) => ChangePage(-1), _defaultFont);
        _backButton = new Button("Back", 0, 0,
            (o, v) => Window.Instance.ScreenRenderer().SetScreen(new MainMenuScreen()), _defaultFont);

        var title = new Label("Texture Packs", new Vector2(640, 50), _defaultFont, Microsoft.Xna.Framework.Color.White,
            Label.Align.Center);

        _panel.AddControl(title);
        _panel.AddControl(_nextPageButton);
        _panel.AddControl(_previousPageButton);
        _panel.AddControl(_backButton);

        AddControl(_panel);
    }

    private void ChangePage(int direction)
    {
        _currentPage += direction;
        _currentPage = Math.Max(0, Math.Min(_currentPage, (_texturePacks.Count - 1) / PacksPerPage));
        UpdatePage();
    }

    private void UpdatePage()
    {
        // Remove old texture pack labels
        for (var i = 0; i < PacksPerPage; i++)
        {
            _panel.RemoveControl($"PackControl{i}0");
            _panel.RemoveControl($"PackControl{i}1");
            _panel.RemoveControl($"PackControl{i}2");
            _panel.RemoveControl($"PackControl{i}3");
            _panel.RemoveControl($"PackControl{i}4");
            _panel.RemoveControl($"PackControl{i}5");
        }

        // Add new texture pack labels for the current page
        for (var i = 0; i < PacksPerPage && i + _currentPage * PacksPerPage < _texturePacks.Count; i++)
        {
            var pack = _texturePacks[i + _currentPage * PacksPerPage];
            var yOffset = 100 + i * 150;

            var icon = new Image(pack.PackIcon, new Vector2(100, yOffset + 30), new Vector2(64, 64));
            var nameLabel = new Label(pack.PackName, new Vector2(200, yOffset), _defaultFont,
                Microsoft.Xna.Framework.Color.White);
            var creatorLabel = new Label("By " + pack.PackCreator, new Vector2(200, yOffset + 50), _smallFont,
                Microsoft.Xna.Framework.Color.White);
            var descriptionLabel = new Label(pack.PackDescription, new Vector2(200, yOffset + 80), _smallFont,
                Microsoft.Xna.Framework.Color.Gray);
            var versionLabel = new Label("Version: " + pack.PackVersion, new Vector2(200, yOffset + 110), _smallFont,
                Microsoft.Xna.Framework.Color.Gray);
            var enableButton = new Button("Enable", 1150, yOffset + 50, null, _smallFont);

            var currentPackGuid = GameSettings.Instance.GetValue<string>("Strings", "Current Texture Pack");
            if (currentPackGuid != null) _currentPack = _texturePacks.Find(p => p.PackMD5() == currentPackGuid);

            enableButton.OnClick += (o, v) =>
            {
                TextureManager.Instance.TogglePack(pack);
                _currentPack = pack;
                HandleButtonChange(pack, enableButton);
            };

            HandleButtonChange(pack, enableButton);

            icon.Tag = $"PackControl{i}0";
            nameLabel.Tag = $"PackControl{i}1";
            creatorLabel.Tag = $"PackControl{i}2";
            descriptionLabel.Tag = $"PackControl{i}3";
            versionLabel.Tag = $"PackControl{i}4";
            enableButton.Tag = $"PackControl{i}5";

            _panel.AddControl(icon);
            _panel.AddControl(nameLabel);
            _panel.AddControl(creatorLabel);
            _panel.AddControl(descriptionLabel);
            _panel.AddControl(versionLabel);
            _panel.AddControl(enableButton);
        }

        // Enable or disable page buttons as needed
        _previousPageButton.Enabled = _currentPage > 0;
        _nextPageButton.Enabled = _currentPage < (_texturePacks.Count - 1) / PacksPerPage;
    }

    private void HandleButtonChange(TexturePack pack, Button button)
    {
        if (_currentPack != null)
            if (_currentPack.PackMD5().Equals(pack.PackMD5()))
            {
                if (_lastClickedButton != null)
                {
                    _lastClickedButton.Text = "Enable";
                    _lastClickedButton.Enabled = true;
                }

                button.Text = "Enabled";
                button.Enabled = false;
                _lastClickedButton = button;
            }
    }

    public void Relayout()
    {
        var panelPosition = new Vector2(ScalingManager.DesiredWidth / 2f, ScalingManager.DesiredHeight / 2f);
        var panelSize = new Vector2(1280, 720);

        _panel.Size = panelSize;
        _panel.Position = panelPosition - panelSize / 2f;

        _previousPageButton.SetPosition(125, 650);
        _nextPageButton.SetPosition(1155, 650);
        _backButton.SetPosition(640, 650);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
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