/*
 * ColorPicker.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Cosmetris.Util.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls;

public class ColorPicker : Control
{
    private Vector2 _cursorPosition; // Position of the cursor within the circle.
    private readonly float _radius; // Radius of the color picker circle.
    private bool _isDragging; // Indicates if the cursor is currently being dragged.
    private Microsoft.Xna.Framework.Color _selectedColor; // Currently selected color.
    private Microsoft.Xna.Framework.Color[] _colorData;

    private Rectangle _alphaSliderRectangle; // The bounds of the alpha slider.
    private float _alphaSliderHandlePosition; // Vertical position of the slider handle.
    private const float AlphaSliderWidth = 20f; // Width of the alpha slider.
    private const float AlphaSliderHandleHeight = 10f; // Height of the slider handle.

    private Font _font = FontRenderer.Instance.GetFont("orbitron", 20);
    
    public EventHandler<Microsoft.Xna.Framework.Color> OnColorChanged; // Event raised when color is changed.

    public ColorPicker(string text, float x, float y, float radius, Microsoft.Xna.Framework.Color defaultColor = default)
    {
        Text = text;
        Position = new Vector2(x, y);
        Size = new Vector2(radius * 2, radius * 2);
        
        _radius = radius;
        _colorData = new Microsoft.Xna.Framework.Color[RenderUtil.ColorWheelTexture.Width * RenderUtil.ColorWheelTexture.Height];
        RenderUtil.ColorWheelTexture.GetData(_colorData);
        _selectedColor = defaultColor == default ? Microsoft.Xna.Framework.Color.White : defaultColor;
        Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update the color picker value when dragging the mouse
        if (Window.Instance.GetPointer().IsPrimaryClickPressed() && Hover)
        {
            var pos = Window.Instance.GetPointer().GetPointerPosition();
            ControlClick(this, pos);
            OnColorChanged?.Invoke(this, _selectedColor);
        }
        
        _alphaSliderRectangle.X = (int)(GetActualPosition().X + Size.X + 10);
        _alphaSliderRectangle.Y = (int)GetActualPosition().Y;

        if (JustFinishedCreation())
        {
            // get position based on the selected color
            SetBrightnessHandleFromColor(_selectedColor);
            // find position on the color wheel based on the selected color
            _cursorPosition = PositionFromColor(_selectedColor);
        }
        
        // Check for user input on the alpha slider.
        if (Window.Instance.GetPointer().IsPrimaryClickPressed() && _alphaSliderRectangle.Contains(Window.Instance.GetPointer().GetPointerPosition()))
        {
            var mouseY = Window.Instance.GetPointer().GetPointerPosition().Y;
            _alphaSliderHandlePosition = MathHelper.Clamp(mouseY, _alphaSliderRectangle.Top, _alphaSliderRectangle.Bottom - AlphaSliderHandleHeight);
    
            // Calculate the brightness percentage based on the handle's position.
            float brightnessPercentage = 1 - (_alphaSliderHandlePosition - _alphaSliderRectangle.Top) / (_alphaSliderRectangle.Height - AlphaSliderHandleHeight);
    
            // Get the base color from the color wheel
            var baseColor = ColorFromPosition(_cursorPosition);

            // Lerp the color with black based on the brightness percentage
            _selectedColor = Microsoft.Xna.Framework.Color.Lerp(baseColor, Microsoft.Xna.Framework.Color.Black, 1 - brightnessPercentage);
    
            OnColorChanged?.Invoke(this, _selectedColor);
        }
    }
    
    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw the color picker circle using the ColorWheelTexture
        var radius = _radius * GetScaleFactor().X;
        var pos = GetActualPosition() + new Vector2(radius, radius);
        Rectangle rect = new Rectangle((int)ScalingManager.GetScaledX(pos.X), (int)ScalingManager.GetScaledY(pos.Y), (int)(ScalingManager.GetScaledX(Size.X) * GetScaleFactor().X), (int)(ScalingManager.GetScaledY(Size.Y) * GetScaleFactor().Y));
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        _font.DrawLabel(Text, pos.X, pos.Y - radius - (24 * GetScaleFactor().X), Microsoft.Xna.Framework.Color.White, TextHorizontalAlignment.Center, TextVerticalAlignment.Center, scale: GetScaleFactor().X);
        
        spriteBatch.Draw(RenderUtil.ColorWheelTexture, rect, null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(radius*1.28f, radius*1.28f),
            SpriteEffects.None, 0f);

        // Draw the cursor at its current position.
        var cursorSize = 5f; // Or whatever size you prefer.
        
        spriteBatch.Draw(RenderUtil.Pixel, ScalingManager.GetScaledPosition(_cursorPosition), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(0.5f, 0.5f), cursorSize * GetScaleFactor(), SpriteEffects.None, 0f);

        _font.DrawLabel($"{_selectedColor.R}, {_selectedColor.G}, {_selectedColor.B}", pos.X, pos.Y + radius + (10* GetScaleFactor().X), Microsoft.Xna.Framework.Color.White, TextHorizontalAlignment.Center, TextVerticalAlignment.Center, scale: GetScaleFactor().X);
        // Draw the alpha slider's track.
        var r = ScalingManager.GetScaledRectangle(_alphaSliderRectangle);
        r.Width = (int)(r.Width * GetScaleFactor().X);
        r.Height = (int)(r.Height * GetScaleFactor().Y);
        spriteBatch.Draw(RenderUtil.WhiteToTransparentGradient, r, null, _selectedColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        // Draw the alpha slider's handle.
        Rectangle handleRect = new Rectangle(r.X, (int)(int)ScalingManager.GetScaledY(_alphaSliderHandlePosition), (int)(ScalingManager.GetScaledX(_alphaSliderRectangle.Width) * GetScaleFactor().X), (int)(ScalingManager.GetScaledY(AlphaSliderHandleHeight) * GetScaleFactor().Y));
        spriteBatch.Draw(RenderUtil.RedPixel, handleRect, null, Microsoft.Xna.Framework.Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
    
        base.Draw(spriteBatch, gameTime);
    }

    protected override void ControlClick(object sender, Vector2 mousePos)
    {
        Vector2 direction = mousePos - GetActualPosition() - new Vector2(_radius, _radius);
        float distance = direction.Length();
        float cursorOffset = 5f; // Half of the cursor size

        if (distance > _radius - cursorOffset) 
        {
            // If near the edge or outside the circle, normalize the direction and place cursor close to the circumference
            direction.Normalize();
            _cursorPosition = GetActualPosition() + new Vector2(_radius, _radius) + direction * (_radius - cursorOffset);
            // clamp position value to snap to 0 or 255
            _cursorPosition.X = MathHelper.Clamp(_cursorPosition.X, GetActualPosition().X, GetActualPosition().X + Size.X);
            _cursorPosition.Y = MathHelper.Clamp(_cursorPosition.Y, GetActualPosition().Y, GetActualPosition().Y + Size.Y);
            
        }
        else
        {
            // Else, set the cursor position to mouse position
            _cursorPosition = mousePos;
        }
        
        UpdateSelectedColor();
    }

    protected override void OnResize()
    {
        int sliderX = (int)(GetActualPosition().X + Size.X + 10);
        _alphaSliderRectangle = new Rectangle(sliderX, (int)GetActualPosition().Y, (int)AlphaSliderWidth, (int)Size.Y);
        // get position based on the selected color
        SetBrightnessHandleFromColor(_selectedColor);
        // find position on the color wheel based on the selected color
        _cursorPosition = PositionFromColor(_selectedColor);
    }

    private void UpdateSelectedColor()
    {
        // Get the base color from the color wheel
        var baseColor = ColorFromPosition(_cursorPosition);

        // Calculate the brightness percentage based on the handle's position.
        float brightnessPercentage = 1 - (_alphaSliderHandlePosition - _alphaSliderRectangle.Top) / (_alphaSliderRectangle.Height - AlphaSliderHandleHeight);

        // Lerp the color with black based on the brightness percentage
        _selectedColor = Microsoft.Xna.Framework.Color.Lerp(baseColor, Microsoft.Xna.Framework.Color.Black, 1 - brightnessPercentage);
    }

    private Microsoft.Xna.Framework.Color ColorFromPosition(Vector2 position)
    {
        Vector2 offset = position - GetActualPosition();

        // Scale the offset based on the ratio of the control's size to the texture's actual size.
        int x = (int)(offset.X * RenderUtil.ColorWheelTexture.Width / Size.X);
        int y = (int)(offset.Y * RenderUtil.ColorWheelTexture.Height / Size.Y);

        // Use cached color data
        if (x >= 0 && x < RenderUtil.ColorWheelTexture.Width && y >= 0 && y < RenderUtil.ColorWheelTexture.Height)
        {
            return _colorData[y * RenderUtil.ColorWheelTexture.Width + x];
        }

        return Microsoft.Xna.Framework.Color.Transparent;
    }
    
    private Vector2 PositionFromColor(Microsoft.Xna.Framework.Color color)
    {
        Vector3 hsv = ColorExtensions.RgbToHsv(color);

        // Convert hue to an angle (0° to 360°).
        float hueAngle = hsv.X * 360f;

        // Saturation determines the distance from the center.
        float distanceFromCenter = hsv.Y * _radius;

        // Convert polar coordinates to Cartesian coordinates.
        float x = distanceFromCenter * (float)Math.Cos(MathHelper.ToRadians(hueAngle));
        float y = distanceFromCenter * (float)Math.Sin(MathHelper.ToRadians(hueAngle));

        return GetActualPosition() + new Vector2(_radius + x, _radius + y);
    }

    public void SetBrightnessHandleFromColor(Microsoft.Xna.Framework.Color color)
    {
        Vector3 hsv = ColorExtensions.RgbToHsv(color);
        float brightness = hsv.Z;

        float handleRange = _alphaSliderRectangle.Height - AlphaSliderHandleHeight;

        // Calculate the handle's vertical position based on brightness
        _alphaSliderHandlePosition = _alphaSliderRectangle.Bottom - (brightness * handleRange) - AlphaSliderHandleHeight;
    }

    public override void SetPosition(Vector2 position)
    {
        Position = position;
        Position = new Vector2(position.X - Size.X/2f, position.Y);
        // Initialize the alpha slider's bounds.
        int sliderX = (int)(GetActualPosition().X + Size.X + 10);
        _alphaSliderRectangle = new Rectangle(sliderX, (int)GetActualPosition().Y, (int)AlphaSliderWidth, (int)Size.Y);
    }

    public override void SetPosition(int i, int i1)
    {
        Position = new Vector2(i - Size.X/2f, i1);
        // Initialize the alpha slider's bounds.
        int sliderX = (int)(GetActualPosition().X + Size.X + 10);
        _alphaSliderRectangle = new Rectangle(sliderX, (int)GetActualPosition().Y, (int)AlphaSliderWidth, (int)Size.Y);
    }
}