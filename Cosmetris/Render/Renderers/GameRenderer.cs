/*
 * GameRenderer.cs is part of Cosmetris.
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

using Cosmetris.Game.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Renderers;

public class GameRenderer : Renderer
{
    public override void Update(GameTime gameTime)
    {
        ObjectManager.Instance.Update(gameTime);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        //BEGIN LOGGER

        // Start drawing to the target
        _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);

        // Draw all objects to the target
        ObjectManager.Instance.Draw(_spriteBatch);

        // Stop drawing to the target
        _spriteBatch.End();

        //END LOGGER

        base.Draw(gameTime);
    }
}