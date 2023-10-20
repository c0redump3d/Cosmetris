/*
 * GameMode.cs is part of Cosmetris.
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
using Cosmetris.Game.Objects.Cosmonoes;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.GameModes;

public class GameMode : IGameMode
{
    public GameMode(string name, string objective)
    {
        Name = name;
        Objective = objective;
    }

    public ElapsedTimer GameTimer { get; } = new();

    protected UIScalingManager ScalingManager { get; } = Window.Instance.ScalingManager;
    protected GameState GameState => Cosmetris.GameState;

    public string Name { get; }
    public string Objective { get; }

    public bool ShowPinch { get; set; } = true;
    public TetrisGameManager GameManager { get; private set; }
    public Cosmono Cosmono { get; private set; }

    public void GameModeInit(TetrisGameManager gameManager, Cosmono cosmono)
    {
        GameManager = gameManager;
        Cosmono = cosmono;

        Init();
    }

    public virtual void Update(GameTime gameTime)
    {
        if (GameState is not GameState.InGame)
            return;

        GameTimer.Update(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void Init()
    {
    }
}

public interface IGameMode : IDisposable
{
    /// <summary>
    ///     Name of the game mode.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     A basic description of what the player should do in this mode.
    /// </summary>
    string Objective { get; }

    /// <summary>
    ///     If true, the pinch overlay will be drawn.
    /// </summary>
    bool ShowPinch { get; set; }

    TetrisGameManager GameManager { get; }
    Cosmono Cosmono { get; }

    void GameModeInit(TetrisGameManager gameManager, Cosmono cosmono);

    void Update(GameTime gameTime);

    void Draw(SpriteBatch spriteBatch);

    void Dispose();
}