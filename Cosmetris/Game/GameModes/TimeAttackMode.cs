/*
 * TimeAttackMode.cs is part of Cosmetris.
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

using Cosmetris.Render.UI.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.GameModes;

public class TimeAttackMode : GameMode
{
    private const float TimeLimit = 180f;
    private readonly Font _font = FontRenderer.Instance.GetFont("orbitron", 28);

    public TimeAttackMode(string name, string objective) : base(name, objective)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Render time elapsed top right of grid
        var timeElapsed = GameTimer.GetElapsedTimeString();
        var gridPosition = GameManager.GetGrid().GetActualPosition();
        var gridSize = GameManager.GetGrid().Size;
        var gridFinalSize = GameManager.GetGrid().GetFinalSize();

        var timeElapsedPosition = new Vector2(gridPosition.X, gridPosition.Y - 30);

        // correctly align the text based on the scaling size of the grid
        timeElapsedPosition.X *= gridSize.X / gridFinalSize.X;
        timeElapsedPosition.Y *= gridSize.Y / gridFinalSize.Y;

        _font.DrawLabel(timeElapsed, timeElapsedPosition, Color.White * GameManager.GetGrid().GetOpacity());

        base.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        if (GameTimer.HasElapsed(TimeLimit)) GameManager.GameOver();

        base.Update(gameTime);
    }
}