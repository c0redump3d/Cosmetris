/*
 * TextRenderOperation.cs is part of Cosmetris.
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

using Cosmetris.Render.UI.Text.Util;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cosmetris.Render.Managers;

namespace Cosmetris.Render.UI.Text;

public class TextRenderOperation
{
    private Microsoft.Xna.Framework.Color Color;
    private Effect Effect;
    private DynamicSpriteFont Font;
    private TextHorizontalAlignment HorizontalAlignment;
    private Vector2 Origin;
    private Vector2 Position;
    private MonoGameFontStashRenderer Renderer;
    private string Text;
    private Vector2 VectorScale;
    private TextVerticalAlignment VerticalAlignment;

    public float LayerDepth { get; set; }

    public void Reset(MonoGameFontStashRenderer renderer, DynamicSpriteFont font, string text,
        Vector2 position, Microsoft.Xna.Framework.Color color,
        TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment,
        float scale, Vector2 origin, Effect effect, float layerDepth)
    {
        Renderer = renderer;
        Font = font;
        Text = text;
        Position = position;
        Color = color;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        VectorScale = new Vector2(scale);
        Origin = origin;
        Effect = effect;
        LayerDepth = layerDepth;
    }

    public void Render(SpriteBatch textBatch)
    {
        Renderer.TextColor = Color;
        textBatch.Begin(effect: Effect, sortMode: SpriteSortMode.BackToFront);
        Font.DrawText(Renderer, Text, Position, Color, VectorScale, 0, Origin, LayerDepth);
        textBatch.End();
    }
}