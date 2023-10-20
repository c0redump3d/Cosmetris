/*
 * Font.cs is part of Cosmetris.
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
using System.Text;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util.Colors;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Text;

public class Font
{
    private readonly FontSystem _font;

    private readonly Dictionary<string, Vector2> _measureCacheDynamic = new();
    private readonly Dictionary<string, Vector2> _measureCacheOriginal = new();
    
    private readonly Dictionary<string, IEnumerable<(string text, Microsoft.Xna.Framework.Color currentColor)>> _formattedCache = new();
    
    private readonly string _name;
    private readonly int _origSize;

    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    private int _curSize;

    private float _maxHeight;
    private DynamicSpriteFont _origSpriteFont;

    private MonoGameFontStashRenderer _renderer;
    private DynamicSpriteFont _spriteFont;
    private bool _windowSizeChanged = true;

    public Font(string name, int size)
    {
        _name = name;
        _origSize = size;
        _curSize = size;

        var graphicsDevice = Window.Instance.GetGraphicsDevice();
        _font = FontRenderer.LoadFont("Content/Fonts/" + _name + ".ttf", graphicsDevice);
    }

    public void LoadContent(SpriteBatch spriteBatch)
    {
        if (_origSpriteFont == null)
            _origSpriteFont = _font.GetFont(_origSize);

        UpdateFontSize();

        _renderer = new MonoGameFontStashRenderer(spriteBatch);
    }

    public void UpdateFontSize()
    {
        _measureCacheDynamic.Clear();
        _measureCacheOriginal.Clear();
        _curSize = _scalingManager.GetScaledX(_origSize);
        _spriteFont = _font.GetFont(_curSize);

        // Recalculate the maxHeight value
        _maxHeight = 0;
        for (var i = 33; i <= 126; i++)
        {
            var character = (char)i;
            var charHeight = _spriteFont.MeasureString(character.ToString()).Y;

            // Update _maxHeight with the tallest character height found
            if (charHeight > _maxHeight) _maxHeight = charHeight;
        }
    }

    public void DrawLabel(string text, float x, float y, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null, bool scaled = true, float layerDepth = 0f)
    {
        // Translate x, y to screen coordinates
        if (scaled)
        {
            x = _scalingManager.GetScaledX(x);
            y = _scalingManager.GetScaledY(y);
        }

        var textSize = GetFontBase().MeasureString(text);
        var origin = Vector2.Zero;

        // Handle horizontal alignment
        switch (horizontalAlignment)
        {
            case TextHorizontalAlignment.Center:
                origin.X = textSize.X / 2;
                break;
            case TextHorizontalAlignment.Right:
                origin.X = textSize.X;
                break;
        }

        // Handle vertical alignment
        switch (verticalAlignment)
        {
            case TextVerticalAlignment.Center:
                origin.Y = textSize.Y / 2;
                break;
            case TextVerticalAlignment.Bottom:
                origin.Y = textSize.Y;
                break;
        }

        if (textColor == null)
            textColor = Microsoft.Xna.Framework.Color.White;

        var position = new Vector2(x, y);

        // Get a TextRenderOperation object from the pool
        var operation = FontRenderer.Instance.GetTextRenderOperationFromPool();
        if (operation != null)
        {
            // Initialize the object with the current text rendering parameters
            operation.Reset(_renderer, _spriteFont, text, position, textColor.Value,
                horizontalAlignment, verticalAlignment, scale, origin, effect, layerDepth);

            // Add the object to the render queue
            FontRenderer.Instance.AddToRender(operation);
        }
    }


    public void DrawFormattedString(string formattedText, float x, float y, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null, float lerpFactor = 0.5f)
    {
        
        IEnumerable<(string text, Microsoft.Xna.Framework.Color currentColor)> segments;
        
        if(_formattedCache.ContainsKey(formattedText))
            segments = _formattedCache[formattedText];
        else
        {
            segments = ColorFormatter.ParseFormattedString(formattedText, Microsoft.Xna.Framework.Color.White);
            _formattedCache.Add(formattedText, segments);
        }

        // Calculate the total width of the formatted string for centering
        var totalWidth = MeasureString(formattedText).X * scale;

        if (horizontalAlignment == TextHorizontalAlignment.Center)
            x -= totalWidth / 2;
        else if (horizontalAlignment == TextHorizontalAlignment.Right) x -= totalWidth;

        var currentX = x;
        foreach (var (text, color) in segments)
        {
            var textSize = MeasureString(text);

            Microsoft.Xna.Framework.Color effectiveColor;
            if (textColor.HasValue && color != Microsoft.Xna.Framework.Color.White)
                effectiveColor = ColorExtensions.Lerp(color, textColor.Value, lerpFactor);
            else
                effectiveColor = color;

            DrawLabel(text, currentX, y, effectiveColor, TextHorizontalAlignment.Left, verticalAlignment, scale,
                effect);
            currentX += textSize.X * scale;
        }
    }
    
    public void DrawFormattedString(string formattedText, Vector2 position, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null, float lerpFactor = 0.5f)
    {
        DrawFormattedString(formattedText, position.X, position.Y, textColor, horizontalAlignment, verticalAlignment,
            scale, effect, lerpFactor);
    }

    public void DrawLabel(StringBuilder builder, Vector2 position, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null)
    {
        DrawLabel(builder.ToString(), position.X, position.Y, textColor, horizontalAlignment, verticalAlignment,
            scale, effect);
    }
    
    public void DrawLabel(string text, Vector2 position, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null)
    {
        DrawLabel(text, position.X, position.Y, textColor, horizontalAlignment, verticalAlignment,
            scale, effect);
    }

    public void DrawLabel(StringBuilder textBuilder, float x, float y, Microsoft.Xna.Framework.Color? textColor,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top,
        float scale = 1f, Effect effect = null)
    {
        DrawLabel(textBuilder.ToString(), x, y, textColor, horizontalAlignment, verticalAlignment, scale, effect);
    }

    public Vector2 MeasureString(string text)
    {
        // Direct lookup is slightly faster if you're sure the key will exist
        if (!_measureCacheDynamic.ContainsKey(text))
        {
            var segments = ColorFormatter.ParseFormattedString(text, Microsoft.Xna.Framework.Color.White);

            var totalSize = Vector2.Zero;

            foreach (var (segment, _) in segments)
            {
                var segmentSize = _origSpriteFont.MeasureString(segment);
                totalSize.X += segmentSize.X;
                totalSize.Y = Math.Max(totalSize.Y, segmentSize.Y);
            }

            _measureCacheDynamic[text] = totalSize;
        }

        return _measureCacheDynamic[text];
    }

    public DynamicSpriteFont GetFontBase()
    {
        return _spriteFont;
    }

    public string GetName()
    {
        return _name;
    }

    public int GetSize()
    {
        return _curSize;
    }

    public float GetWidth(string line)
    {
        if (!_measureCacheOriginal.ContainsKey(line))
        {
            // check to see if the string is formatted ({})
            var unFormatted = line;
            
            if (unFormatted.Contains("{"))
            {
                // Now, we need to remove the formatting
                foreach (var c in unFormatted)
                {
                    if (c == '{')
                    {
                        var index = unFormatted.IndexOf(c);
                        unFormatted = unFormatted.Remove(index, 2);
                    }
                }
            }
            
            // Translate x, y to screen coordinates
            
            _measureCacheOriginal.Add(line, _origSpriteFont.MeasureString(unFormatted));
            
            return _measureCacheOriginal[line].X;
        }
        
        return _measureCacheOriginal[line].X;
    }

    public float GetHeight()
    {
        return _maxHeight;
    }
}

public class MonoGameFontStashRenderer : IFontStashRenderer
{
    private readonly SpriteBatch _textBatch;

    public MonoGameFontStashRenderer(SpriteBatch spriteBatch)
    {
        _textBatch = spriteBatch;
        GraphicsDevice = spriteBatch.GraphicsDevice;
    }

    public Microsoft.Xna.Framework.Color TextColor { get; set; }

    public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Microsoft.Xna.Framework.Color color,
        float rotation, Vector2 scale, float depth)
    {
        _textBatch.Draw(texture, pos, src.Value, TextColor, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
    }

    public GraphicsDevice GraphicsDevice { get; }


    public void DrawShadow(DynamicSpriteFont font, string text, Vector2 pos, Microsoft.Xna.Framework.Color color,
        Vector2 scale, float depth)
    {
        font.DrawText(this, text, pos + new Vector2(1, 1), color, scale, depth);
    }
}