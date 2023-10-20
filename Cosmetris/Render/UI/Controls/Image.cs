/*
 * Image.cs is part of Cosmetris.
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

namespace Cosmetris.Render.UI.Controls;

public class Image : Control
{
    private bool _isHidden;
    private Texture2D _texture;

    public Image(Texture2D texture, Vector2 position, Vector2 size)
    {
        _texture = texture;
        Position = position;
        Size = size;

        Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        if (_isHidden) return;

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_isHidden || _texture == null) return;

        var pos = GetActualPosition();
        spriteBatch.Draw(_texture,
            new Rectangle(ScalingManager.GetScaledX(pos.X), ScalingManager.GetScaledY(pos.Y),
                ScalingManager.GetScaledX(Size.X), ScalingManager.GetScaledY(Size.Y)),
            Microsoft.Xna.Framework.Color.White * GetOpacity());

        base.Draw(spriteBatch, gameTime);
    }

    public void Hide()
    {
        _isHidden = true;
    }

    public void Show()
    {
        _isHidden = false;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public void SetSize(Vector2 size)
    {
        Size = size;
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);
    }

    public override void SetPosition(int x, int y)
    {
        base.SetPosition(x, y);
    }
}