/*
 * PunisherMode.cs is part of Cosmetris.
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

using Cosmetris.Game.Grid.Util;
using Cosmetris.Util.Numbers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.GameModes;

public class PunisherMode : GameMode
{
    public PunisherMode(string name, string objective) : base(name, objective)
    {
    }

    public override void Init()
    {
        GameManager.GetGrid().OnRowsRemoved += OnLineCleared;
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
        base.Dispose();
    }

    private void OnLineCleared(object sender, RowsRemovedEventArgs e)
    {
        // Punish the player based on how many lines they cleared
        // takes into account t-spins and perfect clears
        // the less lines cleared, the more punishment

        var linesCleared = e.FullRows;
        var wasTSpin = e.WasTSpin;
        var wasPerfectClear = e.WasPerfectClear;

        var chance = RandomUtil.Next(0, 100);
        var punish = 0;

        if (wasTSpin || wasPerfectClear)
            return;

        switch (linesCleared)
        {
            case 1:
                if (chance < 25)
                    return;
                punish = RandomUtil.Next(1, 4);
                break;
            case 2:
                if (chance < 50)
                    return;
                punish = RandomUtil.Next(1, 3);
                break;
            case 3:
                if (chance < 75)
                    return;
                punish = RandomUtil.Next(0, 2);
                break;
        }

        GameManager.GetGrid().AddGarbage(punish);
    }
}