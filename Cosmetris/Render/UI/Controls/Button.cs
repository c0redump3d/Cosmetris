/*
 * Button.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Color;
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Cosmetris.Util.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls;

public class Button : Control
{
    public enum Align
    {
        Left,
        Center,
        Right
    }

    private readonly Align _align;

    private readonly EventHandler<Vector2> _clickEventHandler;
    private readonly Font _font;
    private readonly EventHandler<Vector2> _hoverBeginEventHandler;
    private readonly EventHandler<Vector2> _hoverEndEventHandler;
    private readonly float _padding = 10f;
    private readonly Microsoft.Xna.Framework.Color _textColorHover = new(100, 149, 237); // CornflowerBlue

    private readonly Microsoft.Xna.Framework.Color _textColorNormal = new(255, 255, 255); // White
    private float _currentTextScale;
    private bool _isHidden;

    public Button(string text, float x, float y, EventHandler<Vector2> clickEventHandler,
        Font font, float fontScale = 1f, Align align = Align.Center)
    {
        Text = text;
        Position = new Vector2(x, y);
        _clickEventHandler = clickEventHandler;
        _font = font;
        _align = align;
        _currentTextScale = fontScale;

        UpdateButtonSize();

        // calculate position based on alignment
        AlignButton();

        BeginHover += (sender, vector2) =>
        {
            if (!Enabled)
                return;

            _hoverBeginEventHandler?.Invoke(sender, vector2);
            Window.Instance.GetSoundManager().PlaySFX("hover");
        };
        HoverRelease += (sender, vector2) => { _hoverEndEventHandler?.Invoke(sender, vector2); };
        OnClick += (sender, obj) =>
        {
            _clickEventHandler?.Invoke(sender, obj);
            Window.Instance.GetSoundManager().PlaySFX("click");
        };
        
        Initialize();
    }

    public ColorCache ColorCache { get; set; } = new();

    public void AlignButton()
    {
        var pos = Position;
        var leftPadding = _align == Align.Left ? 0f : (_padding - Size.X) / 2f;

        switch (_align)
        {
            case Align.Left:
                pos.X += Size.X / 2f;
                break;
            case Align.Center:
                pos.X += leftPadding;
                break;
            case Align.Right:
                pos.X -= Size.X / 2f;
                break;
        }

        Position = pos;
    }

    private void UpdateButtonSize()
    {
        var textSize = _font.MeasureString(Text);
        Size = new Vector2(textSize.X + _padding * 2, textSize.Y + _padding * 2);
    }

    public override void Update(GameTime gameTime)
    {
        if (_isHidden) return;

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_isHidden) return;

        var pos = GetActualPosition();

        var opacity = !Enabled ? 0.5f : 1f;

        RenderUtil.DrawBorderRoundRect(pos.X, pos.Y, Size.X, Size.Y, 10f,
            ColorCache.GetLerpedRectangleColor(this, ControlBGColor, ControlHoverBGColor, ControlPressedBGColor) *
            opacity,
            ColorCache.GetLerpedRectangleColor(this, ControlBorderColor, ControlHoverBorderColor,
                ControlPressedBorderColor) * opacity, layer: Layer);

        var textPosition = new Vector2(pos.X + Size.X/2f - 2f, pos.Y + Size.Y/2f - 2f);
        var textColor = Enabled ? Hover ? _textColorHover : _textColorNormal : _textColorNormal * 0.5f;

        if (Enabled)
            textColor = ColorExtensions.Lerp(_textColorNormal, _textColorHover,
                HoverLerpAmount) * GetOpacity();
        _font.DrawLabel(Text, textPosition.X, textPosition.Y, textColor * opacity, TextHorizontalAlignment.Center,
            TextVerticalAlignment.Center, _currentTextScale, layerDepth: Layer);

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

    public override void SetTextScale(float scale)
    {
        _currentTextScale = scale;
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);

        UpdateButtonSize();
        AlignButton();
    }

    public override void SetPosition(int i, int i1)
    {
        base.SetPosition(i, i1);

        UpdateButtonSize();
        AlignButton();
    }

    public void SetText(string text)
    {
        Text = text;
        UpdateButtonSize();
    }
}