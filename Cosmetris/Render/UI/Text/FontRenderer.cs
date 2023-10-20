/*
 * FontRenderer.cs is part of Cosmetris.
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

using System.Collections.Generic;
using System.IO;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text.Util;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Text;

public class FontRenderer
{
    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    private readonly EffectsManager.FX _shadowEffect;
    private readonly EffectsManager.FX _textAA;
    private readonly TextRenderOperationPool _textRenderOperationPool;
    private SpriteBatch _spriteBatch;

    // Using a struct for better key representation
    private struct FontKey
    {
        public string Name { get; }
        public int Size { get; }

        public FontKey(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    // Changing the Fonts dictionary to use FontKey as the key type
    private Dictionary<FontKey, Font> Fonts { get; }

    public FontRenderer(SpriteBatch textBatch)
    {
        Instance ??= this;

        Fonts = new Dictionary<FontKey, Font>();

        var width = _scalingManager.ActualWidth;
        var height = _scalingManager.ActualHeight;

        _spriteBatch = textBatch;

        _textAA = new EffectsManager.FX("text_aa", width, height);
        EffectsManager.Instance.AddEffect(_textAA);

        var rainbow = new EffectsManager.FX("rainbowgradient", width, height);
        EffectsManager.Instance.AddEffect(rainbow);

        _shadowEffect = new EffectsManager.FX("textshadow", width, height);
        EffectsManager.Instance.AddEffect(_shadowEffect);

        _textRenderOperationPool = new TextRenderOperationPool(128);
    }

    public static FontRenderer Instance { get; private set; }

    public Font GetFont(string name, int size)
    {
        var key = new FontKey(name, size);

        if (Fonts.TryGetValue(key, out var font))
        {
            return font;
        }

        font = new Font(name, size);
        font.LoadContent(_spriteBatch);
        Fonts.Add(key, font);

        // Keeping the logging for debugging purposes
        if (Window.Instance.ScreenRenderer().GetScreen() != null)
        {
            Window.Instance.ScreenRenderer().GetScreen().AddConsoleMessage($"Loaded font: {name} {size}px");
        }

        return font;
    }

    public static FontSystem LoadFont(string path, GraphicsDevice graphicsDevice)
    {
        var settings = new FontSystemSettings();
        var result = new FontSystem(settings);
        using var stream = File.OpenRead(path);
        result.AddFont(stream);

        return result;
    }

    public void ReloadFonts(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;

        foreach (var fontPair in Fonts)
        {
            fontPair.Value.LoadContent(spriteBatch);
        }
    }

    public TextRenderOperation GetTextRenderOperationFromPool()
    {
        return _textRenderOperationPool.Get();
    }

    public void ReturnTextRenderOperationToPool(TextRenderOperation operation)
    {
        _textRenderOperationPool.Return(operation);
    }

    public void AddToRender(TextRenderOperation operation)
    {
        operation.Render(_spriteBatch);
        ReturnTextRenderOperationToPool(operation);
    }
}