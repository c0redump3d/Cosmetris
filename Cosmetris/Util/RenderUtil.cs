/*
 * RenderUtil.cs is part of Cosmetris.
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
using System.IO;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using ColorExtensions = Cosmetris.Util.Colors.ColorExtensions;

namespace Cosmetris.Util;

public static class RenderUtil
{
    private static readonly List<Texture2D> _starTextures = new();

    private static EffectsManager.FX _roundRectEffect;

    public static Texture2D Pixel;
    public static Texture2D RedPixel;
    public static Texture2D GreenPixel;
    
    public static Texture2D WhiteToTransparentGradient { get; } = GenerateWhiteToBlackGradient(Window.Instance.GetGraphicsDevice(), 1, 256);
    public static Texture2D ColorWheelTexture { get; } = GenerateColorWheelTexture(Window.Instance.GetGraphicsDevice(), 256);

    private static SpriteBatch UtilBatch { get; set; }

    private static GraphicsDevice _graphics { get; } = Window.Instance.GetGraphicsDevice();
    private static UIScalingManager _scalingManager { get; } = Window.Instance.ScalingManager;
    private static Random Random { get; } = new();

    public static void Initialize(SpriteBatch spriteBatch)
    {
        
        UtilBatch = spriteBatch;
        
        // Get how many files are in the star folder
        var starCount = 0;
        foreach (var tex in Directory.GetFiles($"{ContentUtil.Instance.RootDirPath}/Textures/FX/Sparkle"))
        {
            var file = new FileInfo(tex);
            var path = $@"{file.ToString().Replace($"{ContentUtil.Instance.RootDirPath}/", @"").Replace(@".xnb", @"")}";
            var load = Cosmetris.Instance.Content.Load<Texture2D>(path);
            _starTextures.Add(load);
            starCount++;
        }

        Pixel = new Texture2D(_graphics, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData(new[] { Color.Black });
        
        RedPixel = new Texture2D(_graphics, 1, 1, false, SurfaceFormat.Color);
        RedPixel.SetData(new[] { Color.Red });
        
        GreenPixel = new Texture2D(_graphics, 1, 1, false, SurfaceFormat.Color);
        GreenPixel.SetData(new[] { Color.Green });

        TextureManager.Instance.AddTexture("noise-gui", "FX/noise-gui");

        _roundRectEffect = new EffectsManager.FX("roundedrect", 0, 0);
        //_roundRectEffect.Effect.Parameters["NoiseSampler"].SetValue(_guiNoise);
        _roundRectEffect.ApplyEffect();
        EffectsManager.Instance.AddEffect(_roundRectEffect);
    }

    public static void OnResize(SpriteBatch spriteBatch)
    {
        UtilBatch = spriteBatch;
    }

    public static void DrawRoundRect(float x, float y, float width, float height, float radius, Color color)
    {
        if (width <= 0 || height <= 0)
            return;

        x = _scalingManager.GetScaledX(x);
        y = _scalingManager.GetScaledY(y);
        width = _scalingManager.GetScaledX(width);
        height = _scalingManager.GetScaledY(height);
        radius = _scalingManager.GetScaledX(radius);

        // Prepare shader effect
        _roundRectEffect.Effect.Parameters["Radius"].SetValue(radius);
        _roundRectEffect.Effect.Parameters["Color"].SetValue(color.ToVector4());
        _roundRectEffect.Effect.Parameters["OutlineColor"].SetValue(Color.Black.ToVector4());
        _roundRectEffect.Effect.Parameters["Size"].SetValue(new Vector2(width, height));
        _roundRectEffect.Effect.Parameters["OutlineThickness"].SetValue(1.75f);

        // Draw rounded rectangle
        UtilBatch.Begin(effect: _roundRectEffect.Effect, sortMode: SpriteSortMode.BackToFront, depthStencilState: _graphics.DepthStencilState, rasterizerState: RasterizerState.CullNone);
        UtilBatch.Draw(Pixel, new Rectangle((int)x, (int)y, (int)width, (int)height), Color.Transparent);
        UtilBatch.End();
    }

    public static void DrawLine(float x1, float y1, float x2, float y2, Color color, float layer = 0f)
    {
        x1 = _scalingManager.GetScaledX(x1);
        y1 = _scalingManager.GetScaledY(y1);
        x2 = _scalingManager.GetScaledX(x2);
        y2 = _scalingManager.GetScaledY(y2);

        UtilBatch.Begin(sortMode: SpriteSortMode.BackToFront, depthStencilState: _graphics.DepthStencilState, rasterizerState: RasterizerState.CullNone);
        UtilBatch.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, layerDepth: layer);
        UtilBatch.End();
    }

    public static void DrawLine(Vector2 startPoint, Vector2 endPoint, Color color, float layer = 0f)
    {
        DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, color, layer);
    }
    
    public static void DrawBorderRoundRect(float x, float y, float width, float height, float radius, Color innerColor,
        Color outerColor, float borderWidth = 1.75f, float layer = 0f)
    {
        if (width <= 0 || height <= 0)
            return;

        x = _scalingManager.GetScaledX(x) - borderWidth;
        y = _scalingManager.GetScaledY(y) + borderWidth;
        width = _scalingManager.GetScaledX(width);
        height = _scalingManager.GetScaledY(height);

        // Prepare shader effect
        _roundRectEffect.Effect.Parameters["Radius"].SetValue(radius);
        _roundRectEffect.Effect.Parameters["Color"].SetValue(innerColor.ToVector4());
        _roundRectEffect.Effect.Parameters["OutlineColor"].SetValue(outerColor.ToVector4());
        _roundRectEffect.Effect.Parameters["Size"].SetValue(new Vector2(width, height));
        _roundRectEffect.Effect.Parameters["OutlineThickness"].SetValue(borderWidth);

        // Draw rounded rectangle
        UtilBatch.Begin(SpriteSortMode.BackToFront, effect: _roundRectEffect.Effect, depthStencilState: _graphics.DepthStencilState, rasterizerState: RasterizerState.CullNone);
        UtilBatch.Draw(Pixel, new Rectangle((int)x, (int)y, (int)width, (int)height), null, Color.Transparent, 0f,
            Vector2.Zero, SpriteEffects.None, layer);
        UtilBatch.End();
    }

    public static void DrawBorderRect(float x, float y, float width, float height, Color innerColor, Color outerColor)
    {
        x = x;
        y = y;
        width = width;
        height = height;

        var pos = new Vector2(x, y);
        var size = new Vector2(width, height);

        // Prepare shader effect
        _roundRectEffect.Effect.Parameters["Radius"].SetValue(5f);
        _roundRectEffect.Effect.Parameters["Color"].SetValue(innerColor.ToVector4());
        _roundRectEffect.Effect.Parameters["OutlineColor"].SetValue(outerColor.ToVector4());
        _roundRectEffect.Effect.Parameters["Size"].SetValue(new Vector2(width, height));
        _roundRectEffect.Effect.Parameters["OutlineThickness"].SetValue(1.75f);

        // Draw rounded rectangle
        UtilBatch.Begin(effect: _roundRectEffect.Effect, depthStencilState: _graphics.DepthStencilState, rasterizerState: RasterizerState.CullNone);
        UtilBatch.Draw(Pixel, new Rectangle((int)x, (int)y, (int)width, (int)height), Color.Transparent);
        UtilBatch.End();
    }

    public static Texture2D GetStarTexture()
    {
        var index = Random.Next(0, _starTextures.Count - 1);
        return _starTextures[index];
    }

    public static Texture2D GenerateGridTexture(int rows, int columns, int cellSize, int borderWidth, int lineWidth,
        Vector2 vec, GraphicsDevice graphicsDevice)
    {
        var gridTexture = new Texture2D(graphicsDevice, (int)vec.X, (int)vec.Y);
        var colorData = new Color[(int)vec.X * (int)vec.Y];

        var gridColor = new Color(75, 75, 75, 200);
        var borderColor = new Color(51, 153, 255, 235);
        var clearColor = new Color(0, 0, 0, 165);
        var stopBeforeTop = borderWidth; // Define how much gap you want before top

        // Fill the entire grid with clearColor
        for (var y = stopBeforeTop; y < vec.Y; y++)
        for (var x = 0; x < vec.X; x++)
            colorData[x + y * (int)vec.X] = clearColor;

        // Draw grid lines
        for (var i = 0; i <= columns; i++)
        {
            var xPos = i * (cellSize + lineWidth) + borderWidth;
            for (var y = stopBeforeTop; y < vec.Y; y++) // start drawing vertical lines from where the border starts
            for (var x = 0; x < lineWidth; x++)
                colorData[xPos + x + y * (int)vec.X] = gridColor;
        }


        for (var j = 0; j <= rows; j++)
        {
            var yPos = j * (cellSize + lineWidth) + borderWidth;
            for (var x = 0; x < vec.X; x++)
            for (var y = 0; y < lineWidth; y++)
                colorData[x + (yPos + y) * (int)vec.X] = gridColor;
        }

        // Draw border
        // Left and Right Borders (stop a little before the top)

        for (var y = stopBeforeTop; y < vec.Y - stopBeforeTop; y++)
        for (var x = 0; x < borderWidth; x++)
        {
            colorData[x + y * (int)vec.X] = borderColor;
            colorData[(int)vec.X - 1 - x + y * (int)vec.X] = borderColor;
        }

        // Bottom Border
        for (var x = 0; x < vec.X; x++)
        for (var y = 0; y < borderWidth; y++)
            colorData[x + ((int)vec.Y - 1 - y) * (int)vec.X] = borderColor;

        gridTexture.SetData(colorData);
        return gridTexture;
    }
    
    private static Texture2D GenerateColorWheelTexture(GraphicsDevice graphicsDevice, int diameter)
    {
        Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);
        Color[] data = new Color[diameter * diameter];

        Vector2 center = new Vector2(diameter / 2f, diameter / 2f);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Vector2 position = new Vector2(x, y);
                Vector2 direction = position - center;
                float distance = direction.Length();
                float angle = (float)Math.Atan2(direction.Y, direction.X);

                // Map the angle to [0, 2*PI]
                angle = angle < 0 ? angle + 2 * (float)Math.PI : angle;

                // Convert angle and distance to HSV
                float hue = angle * (float)(180 / Math.PI) / 360; // Convert angle from [0, 2*PI] to [0, 1]
                float saturation = MathHelper.Clamp(distance / (diameter / 2f), 0, 1);
                float value = distance <= diameter / 2f ? 1 : 0; // Use this to make sure we don't color outside the circle.

                Color col = Colors.ColorExtensions.HsvToRgb(hue, saturation, value);
                
                // Set the alpha to transparent if the pixel is outside the circle
                if (distance > diameter / 2f)
                {
                    col.A = 0;
                }
                
                // Convert HSV to RGB
                data[y * diameter + x] = col;
            }
        }

        texture.SetData(data);
        return texture;
    }
    
    private static Texture2D GenerateWhiteToBlackGradient(GraphicsDevice graphicsDevice, int width, int height)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float brightness = 1 - (float)y / height;
            for (int x = 0; x < width; x++)
            {
                data[y * width + x] = new Color(brightness, brightness, brightness);
            }
        }

        texture.SetData(data);
        return texture;
    }
    
}