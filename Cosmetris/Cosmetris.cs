/*
 * Cosmetris.cs is part of Cosmetris.
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

﻿using Cosmetris.Game;
using Cosmetris.Render;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris;

public class Cosmetris : Microsoft.Xna.Framework.Game
{
    private const int BaseWidth = 1920;

    private const int BaseHeight = 1080;

    // Holds a private instance of the higher-level window class.
    private readonly Window _window;

    public Cosmetris()
    {
        // Create the window.
        _window = new Window(new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = BaseWidth,
            PreferredBackBufferHeight = BaseHeight,
            HardwareModeSwitch = false,
            SynchronizeWithVerticalRetrace = false,
            PreferredDepthStencilFormat = DepthFormat.None
        });
        
        GraphicsDevice.RasterizerState = new RasterizerState()
        {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = true
        };

        // Disable locked framerate and enable resizing.
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = false; // Hide cursor, we use a custom one

        Instance ??= this;
    }

    public static GameState GameState { get; private set; } = GameState.MainMenu;

    public static Cosmetris Instance { get; private set; }

    protected override void Initialize()
    {
        GraphicsDevice.PresentationParameters.BackBufferWidth = BaseWidth - 1;
        GraphicsDevice.PresentationParameters.BackBufferHeight = BaseHeight;
        GraphicsDevice.PresentationParameters.BackBufferFormat = SurfaceFormat.Color;
        GraphicsDevice.PresentationParameters.DepthStencilFormat = DepthFormat.None;
        GraphicsDevice.PresentationParameters.MultiSampleCount = 32;

        // apply changes
        GraphicsDevice.Reset();

        base.Initialize();
    }

    public static void UpdateGameState(GameState state)
    {
        GameState = state;
        if(Render.Window.Instance.ScreenRenderer().GetScreen() != null)
            Render.Window.Instance.ScreenRenderer().GetScreen().AddConsoleMessage("Game state changed to: " + state);
    }

    protected override void LoadContent()
    {
        _window.ContentLoad();
    }

    protected override void Update(GameTime gameTime)
    {
        _window.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _window.Draw(gameTime);

        base.Draw(gameTime);
    }
}