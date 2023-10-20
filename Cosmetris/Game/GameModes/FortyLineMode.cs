/*
 * FortyLineMode.cs is part of Cosmetris.
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
using Cosmetris.Game.Grid.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.GameModes;

public class FortyLineMode : GameMode
{
    private int _linesCleared;

    public FortyLineMode(string name, string objective) : base(name, objective)
    {
    }

    public override void Init()
    {
        GameManager.GetGrid().OnRowsRemoved += OnLineCleared;
        GameManager.GetGrid().OnGridDissolveComplete += OnGridDissolveComplete;
    }

    private void OnGridDissolveComplete(object sender, EventArgs e)
    {
        if (_linesCleared >= 40) GameManager.GameOver();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Dispose()
    {
        GameManager.GetGrid().OnRowsRemoved -= OnLineCleared;
        GameManager.GetGrid().OnGridDissolveComplete -= OnGridDissolveComplete;
        base.Dispose();
    }

    private void OnLineCleared(object sender, RowsRemovedEventArgs e)
    {
        _linesCleared += e.FullRows;
    }
}