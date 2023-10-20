/*
 * Renderer.cs is part of Cosmetris.
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Renderers;

public class Renderer
{
    protected SpriteBatch _spriteBatch;

    protected GraphicsDevice GetGraphicsDevice()
    {
        return Window.Instance.GetGraphicsDevice();
    }

    public virtual void Initialize(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);

        //TODO: Logger
    }

    public virtual void Draw(GameTime gameTime)
    {
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void OnResize()
    {
    }

    public virtual void DrawImportant(GameTime gameTime)
    {
    }
}