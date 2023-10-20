/*
 * Label.cs is part of Cosmetris.
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
using System.Text;
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Cosmetris.Render.UI.Controls;

public class Label : Control
{
    public enum Align
    {
        Left,
        Center,
        Right
    }

    public enum Overflow
    {
        Wrap,
        Clip
    }

    public enum VerticalAlign
    {
        Top,
        Center,
        Bottom
    }

    private readonly Align _align;

    private readonly Effect _effect;
    private readonly Font _font;
    private readonly int _maxLineWidth;
    private readonly Overflow _overflow;

    private readonly Microsoft.Xna.Framework.Color _textColor;
    private float _createdScale;
    private bool _isPositionChanged = true; // Flag to indicate if the position has changed and needs recalculating
    private RectangleF _lastHoverRect;
    private bool _isTextChanged = true; // Flag to indicate if the text has changed and needs recalculating
    private bool _isTextFormatted;
    private float _lineHeight;

    private string[] _lines;

    private float _lineSpacing;
    private string _processedText;
    private float _realScale;
    private string _text;
    private float _totalHeight;

    private VerticalAlign _verticalAlign = VerticalAlign.Top;

    private int _lastWindowWidth;
    private int _lastWindowHeight;

    public Label(string text, Vector2 position, Font font, Microsoft.Xna.Framework.Color textColor = default,
        Align align = Align.Left, Overflow overflow = Overflow.Wrap,
        float scale = 1f, int maxLineWidth = 0, Effect effect = null)
    {

        _lastWindowWidth = Window.Instance.ScalingManager.ActualWidth;
        _lastWindowHeight = Window.Instance.ScalingManager.ActualHeight;
        
        _text = text;
        Position = position;
        _font = font;
        _isTextChanged = true;
        Size = new Vector2(_font.MeasureString(text).X, _font.MeasureString(text).Y);
        _textColor = textColor == default ? PanelInfoTextColor : textColor;
        _align = align;
        _overflow = overflow;
        _createdScale = scale < 1.0f ? 0f : 1f;
        _realScale = scale;
        _maxLineWidth = maxLineWidth;
        _effect = effect;
        Initialize();
    }

    private float Scale
    {
        get => _realScale * _createdScale;
        set => _realScale = value;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_isTextChanged)
        {
            UpdateHoverRect();
            ProcessText();
            _lines = _processedText.Split('\n');

            _lineHeight = _font.MeasureString("A").Y * Scale;
            _totalHeight = _lines.Length * (_lineHeight + _lineSpacing) - _lineSpacing;
            _isTextChanged = false; // Reset the flag
        }

        var pos = GetActualPosition();
        var horizontalAlignment = GetHorizontalAlignment();

        // 3. Adjust the starting vertical position based on the vertical alignment
        float currentY;
        switch (_verticalAlign)
        {
            case VerticalAlign.Center:
                currentY = pos.Y - _totalHeight / 2;
                break;
            case VerticalAlign.Bottom:
                currentY = pos.Y - _totalHeight;
                break;
            default: // Top
                currentY = pos.Y;
                break;
        }

        foreach (var line in _lines)
        {
            var lineWidth = _font.MeasureString(line).X * Scale;
            var drawX = pos.X;

            if (horizontalAlignment == TextHorizontalAlignment.Center)
                drawX -= lineWidth / 2;
            else if (horizontalAlignment == TextHorizontalAlignment.Right) drawX -= lineWidth;

            if (_isTextFormatted)
                _font.DrawFormattedString(line, drawX, currentY, _textColor * GetOpacity(),
                    TextHorizontalAlignment.Left,
                    TextVerticalAlignment.Top, Scale, _effect);
            else
                _font.DrawLabel(line, drawX, currentY, _textColor * GetOpacity(),
                    TextHorizontalAlignment.Left,
                    TextVerticalAlignment.Top, Scale, _effect, layerDepth: Layer);

            currentY += _lineHeight + _lineSpacing;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_createdScale < 1f)
        {
            _createdScale += 0.0025f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _isTextChanged = true; // Set the flag
        }
        else
        {
            _createdScale = 1f;
        }
        
        if (_lastWindowWidth != Window.Instance.ScalingManager.ActualWidth ||
            _lastWindowHeight != Window.Instance.ScalingManager.ActualHeight)
        {
            _lastWindowWidth = Window.Instance.ScalingManager.ActualWidth;
            _lastWindowHeight = Window.Instance.ScalingManager.ActualHeight;
            UpdateHoverRect();
        }
        

        base.Update(gameTime);
    }

    private void ProcessText()
    {
        if (_maxLineWidth > 0 && _overflow == Overflow.Wrap)
            _processedText = WrapText(_text, _maxLineWidth);
        else if (_maxLineWidth > 0 && _overflow == Overflow.Clip)
            _processedText = ClipText(_text, _maxLineWidth);
        else
            _processedText = _text;

        _isTextFormatted = _processedText.Contains('{');
    }

    private TextHorizontalAlignment GetHorizontalAlignment()
    {
        switch (_align)
        {
            case Align.Center:
                return TextHorizontalAlignment.Center;
            case Align.Right:
                return TextHorizontalAlignment.Right;
            default:
                return TextHorizontalAlignment.Left;
        }
    }

    public void SetLineSpacing(float spacing)
    {
        _lineSpacing = spacing;
    }

    public void SetVerticalAlignment(VerticalAlign verticalAlign)
    {
        _verticalAlign = verticalAlign;
    }

    public void SetText(string text)
    {
        if (_text != text)
        {
            _text = text;
            _isTextChanged = true; // Set the flag
        }
    }

    private string WrapText(string text, int maxLineWidth)
    {
        var words = text.Split(' ');
        var wrappedText = new StringBuilder();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            var testLineWidth = _font.MeasureString(testLine).X;

            if (testLineWidth <= maxLineWidth)
            {
                currentLine = testLine;
            }
            else
            {
                if (wrappedText.Length > 0)
                    wrappedText.Append("\n");

                wrappedText.Append(currentLine);
                currentLine = word;
            }
        }

        if (currentLine.Length > 0)
        {
            if (wrappedText.Length > 0)
                wrappedText.Append("\n");

            wrappedText.Append(currentLine);
        }

        return wrappedText.ToString();
    }

    private string ClipText(string text, int maxLineWidth)
    {
        var clippedText = "";
        var currentLineWidth = 0f;

        foreach (var t in text)
        {
            var charWidth = _font.MeasureString(t.ToString()).X;
            currentLineWidth += charWidth;

            if (currentLineWidth <= maxLineWidth)
                clippedText += t;
            else
                break;
        }

        return clippedText;
    }

    public void SetLines(string[] lines)
    {
        _text = string.Join("\n", lines);
        Size = new Vector2(_font.MeasureString(_text).X * Scale, _font.MeasureString(_text).Y * Scale);
        _isTextChanged = true; // Set the flag
    }

    protected override RectangleF GetHoverRect()
    {
        return _lastHoverRect;
    }

    public override void SetTextScale(float scale)
    {
        Scale = scale;
        _isTextChanged = true; // Set the flag
    }

    private void UpdateHoverRect()
    {
        if(_lines == null || _lines.Length == 0) return;
        
        var pos = GetActualPosition();
        var horizontalAlignment = GetHorizontalAlignment();
        
        // 3. Adjust the starting vertical position based on the vertical alignment
        float currentY;
        switch (_verticalAlign)
        {
            case VerticalAlign.Center:
                currentY = pos.Y - _totalHeight / 2;
                break;
            case VerticalAlign.Bottom:
                currentY = pos.Y - _totalHeight;
                break;
            default: // Top
                currentY = pos.Y;
                break;
        }
        
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        
        foreach (var line in _lines)
        {
            var lineWidth = _font.MeasureString(line).X * Scale;
            var drawX = pos.X;

            if (horizontalAlignment == TextHorizontalAlignment.Center)
                drawX -= lineWidth / 2;
            else if (horizontalAlignment == TextHorizontalAlignment.Right) drawX -= lineWidth;

            minX = Math.Min(minX, drawX);
            maxX = Math.Max(maxX, drawX + lineWidth);
            minY = Math.Min(minY, currentY);
            maxY = Math.Max(maxY, currentY + _lineHeight);
            
            currentY += _lineHeight + _lineSpacing;
        }
        
        _lastHoverRect = new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    protected override void OnResize()
    {
        base.OnResize();
    }
}