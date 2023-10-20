/*
 * Slider.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls;

public class Slider : Control
{
    public enum SliderType
    {
        Line,
        Rectangle
    }

    public enum ValueFormat
    {
        Percentage,
        Float,
        Int
    }

    private readonly Font _font;
    private readonly int _numSnapPoints;
    private float _currentTextScale;
    
    private string _cachedValueText = null;
    private float? _cachedProgress = null;
    private List<float> _cachedSnapPoints;

    public Slider(Vector2 position, Vector2 size, SliderType type, float minValue, float maxValue, float initialValue,
        ValueFormat format, string fontName, float fontSize, int snapPoints = 10)
    {
        position.X -= size.X / 2f;
        Position = position;
        Size = size;
        Type = type;
        MinValue = minValue;
        MaxValue = maxValue;
        InitialValue = initialValue;
        Value = initialValue;
        Format = format;
        _numSnapPoints = snapPoints;
        _font = FontRenderer.Instance.GetFont(fontName, (int)fontSize);
        _currentTextScale = fontSize / _font.GetHeight();
        CalculateSnapPoints();
        Initialize();
    }

    public SliderType Type { get; set; }

    public ValueFormat Format { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public float Value { get; set; }
    public float InitialValue { get; set; }
    public EventHandler<Slider> OnValueChanged { get; set; }

    private void DrawLineSlider(SpriteBatch spriteBatch)
    {
        var actualPosition = GetActualPosition();

        // Render the value text first, so it's drawn behind the slider dragger
        var valueText = GetValueText();
        var textSize = _font.MeasureString(valueText);
        _font.DrawLabel(valueText, actualPosition.X + (Size.X - textSize.X * _currentTextScale) / 2,
            actualPosition.Y + Size.Y,
            ControlTextColor, scale: _currentTextScale);

        RenderUtil.DrawLine(actualPosition.X, actualPosition.Y + Size.Y / 2, actualPosition.X + Size.X,
            actualPosition.Y + Size.Y / 2,
            ControlBorderColor);

        // Draw the small lines and snap points
        if (Type == SliderType.Line)
            for (var i = 0; i <= _numSnapPoints; i++)
            {
                var x = actualPosition.X + Size.X / _numSnapPoints * i;
                RenderUtil.DrawLine(x, actualPosition.Y + Size.Y / 2 - Size.Y / 4, x,
                    actualPosition.Y + Size.Y / 2 + Size.Y / 4, ControlBorderColor);
            }

        var progress = GetProgress();
        var valueBarPosition = new Vector2(actualPosition.X + progress * Size.X, actualPosition.Y);
        RenderUtil.DrawRoundRect(valueBarPosition.X - 4f, valueBarPosition.Y, 8, Size.Y, 5f,ColorCache.GetLerpedRectangleColor(this, ChildControlBorderColor, ChildControlHoverBorderColor,
            ChildControlPressedBorderColor));
    }

    private void DrawRectangleSlider(SpriteBatch spriteBatch)
    {
        var actualPosition = GetActualPosition();

        RenderUtil.DrawRoundRect(actualPosition.X, actualPosition.Y, Size.X, Size.Y, 5,
            ColorCache.GetLerpedRectangleColor(this, ChildControlBGColor, ChildControlHoverBGColor,
                ChildControlPressedBGColor));

        // Render the value text first, so it's drawn behind the slider dragger
        var valueText = GetValueText();
        _font.DrawLabel(valueText, actualPosition.X + (Size.X / 2f),
            actualPosition.Y + (Size.Y / 2f) - 2.5f, ControlTextColor, TextHorizontalAlignment.Center, TextVerticalAlignment.Center, scale: _currentTextScale);
        
        var progress = GetProgress();
        var sliderPosition = new Vector2(actualPosition.X + progress * (Size.X - Size.Y), actualPosition.Y);
        RenderUtil.DrawRoundRect(sliderPosition.X, sliderPosition.Y, Size.Y, Size.Y, 5,
            ColorCache.GetLerpedRectangleColor(this, ChildControlBorderColor, ChildControlHoverBorderColor,
                ChildControlPressedBorderColor) * 0.65f);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);

        // Draw the slider based on the type
        switch (Type)
        {
            case SliderType.Line:
                // Draw the line and value bar
                DrawLineSlider(spriteBatch);
                break;
            case SliderType.Rectangle:
                // Draw the rectangle and slider inside it
                DrawRectangleSlider(spriteBatch);
                break;
        }
    }

    private string GetValueText()
    {
        // If we've cached the value, return the cached version
        if (_cachedValueText != null)
            return _cachedValueText;

        // Otherwise, calculate the value text
        switch (Format)
        {
            case ValueFormat.Percentage:
                _cachedValueText = $"{(int)((Value - MinValue) / (MaxValue - MinValue) * 100)}%";
                break;
            case ValueFormat.Float:
                _cachedValueText = $"{Value:F1}";
                break;
            case ValueFormat.Int:
                _cachedValueText = $"{(int)Value}";
                break;
            default:
                throw new InvalidOperationException("Invalid value format.");
        }

        return _cachedValueText;
    }
    
    private float GetProgress()
    {
        if (!_cachedProgress.HasValue)
        {
            _cachedProgress = (Value - MinValue) / (MaxValue - MinValue);
        }

        return _cachedProgress.Value;
    }

    private void CalculateSnapPoints()
    {
        _cachedSnapPoints = new List<float>();

        for (var i = 0; i <= _numSnapPoints; i++)
        {
            var snapPoint = MinValue + (MaxValue - MinValue) / _numSnapPoints * i;
            _cachedSnapPoints.Add(snapPoint);
        }
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);

        var vector2 = Position;
        vector2.X -= Size.X / 2f;

        Position = vector2;
    }

    public override void SetPosition(int i, int i1)
    {
        base.SetPosition(i, i1);

        var vector2 = Position;
        vector2.X -= Size.X / 2f;

        Position = vector2;
    }

    public override void SetTextScale(float scale)
    {
        _currentTextScale = scale;
    }

    // Update the Slider value based on the pointer position
    protected override void ControlClick(object sender, Vector2 mousePos)
    {
        base.ControlClick(sender, mousePos);

        var pointerProgress = (mousePos.X - GetActualPosition().X) / Size.X;
        Value = MathHelper.Clamp(MinValue + pointerProgress * (MaxValue - MinValue), MinValue, MaxValue);
        
        
        // Snap to the nearest snap point
        if (Type == SliderType.Line)
        {
            foreach (var snapPoint in _cachedSnapPoints)
            {
                var snapThreshold = (MaxValue - MinValue) / _numSnapPoints / 2f;
                if (Math.Abs(Value - snapPoint) < snapThreshold)
                {
                    Value = snapPoint;
                    break;
                }
            }
        }
        
        // If close to either end, snap to it
        if (Math.Abs(Value - MinValue) < 0.005f) Value = MinValue;
        if (Math.Abs(Value - MaxValue) < 0.005f) Value = MaxValue;
        
        // Snap to default values
        var middle = (MaxValue - MinValue) / 2f;
        if (Math.Abs(Value - middle) < 0.005f) Value = middle;

        if (Format == ValueFormat.Int) Value = (float)Math.Round(Value);
        _cachedValueText = null;
        _cachedProgress = null;
        OnValueChanged?.Invoke(this, this);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Update the slider value when dragging the mouse
        if (Window.Instance.GetPointer().IsPrimaryClickPressed() && Hover)
        {
            var pos = Window.Instance.GetPointer().GetPointerPosition();
            ControlClick(this, pos);
            OnValueChanged?.Invoke(this, this);
        }
    }
}