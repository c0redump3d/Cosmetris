/*
 * TextureManager.cs is part of Cosmetris.
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
using System.Linq;
using Cosmetris.Game.Packs.TexturePacks;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Managers;

public class TextureManager
{
    private readonly List<Texture> _textures;

    private TextureManager()
    {
        _textures = new List<Texture>();
    }

    public bool IsUsingCustomTexturePack { get; set; }
    public TexturePack CurrentTexturePack { get; private set; }

    public static TextureManager Instance { get; } = new();

    public void AddTexture(string name, string textureName)
    {
        // Make sure we don't already have this texture
        if (GetTexture(name) != null)
            //TODO: Logger
            return;

        var texture2D = Cosmetris.Instance.Content.Load<Texture2D>("Textures/" + textureName);

        _textures.Add(new Texture(name, texture2D));
    }


    public void UpdateCustomTexture(TexturePack pack, string name, Texture2D texture2D)
    {
        // Make sure we don't already have this texture
        CurrentTexturePack = pack;
        var uuid = pack.PackMD5();

        var curTex = GetTexture($"{name}-{uuid}");
        if (curTex != null)
        {
            curTex.Texture2D = new Texture2D(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height);
            var dataColors = new Color[texture2D.Width * texture2D.Height];
            texture2D.GetData(dataColors);
            curTex.Texture2D.SetData(dataColors);
            return;
        }
        
        Window.Instance.ScreenRenderer().GetScreen()?.AddConsoleMessage($"Loaded texture: {name} {uuid}");

        IsUsingCustomTexturePack = true;
        _textures.Add(new Texture($"{name}-{uuid}", texture2D));
    }

    public Texture2D GetTexture2D(string name)
    {
        if (IsUsingCustomTexturePack)
            foreach (var t in _textures.Where(t => t.Name.ToLower().Contains(name.ToLower()) && t.Name.ToLower()
                         .Contains(CurrentTexturePack.PackMD5().ToLower())))
                return t.Texture2D;

        return (from t in _textures where t.Name.ToLower().Equals(name.ToLower()) select t.Texture2D).FirstOrDefault();
    }

    public Texture GetTexture(string name)
    {
        for (var i = 0; i < _textures.Count; i++)
            if (IsUsingCustomTexturePack)
            {
                if (_textures[i].Name.ToLower().Contains(name.ToLower()) &&
                    _textures[i].Name.ToLower().Contains(CurrentTexturePack.PackMD5().ToLower()))
                    return _textures[i];
            }
            else if (_textures[i].Name.ToLower().Equals(name.ToLower()))
            {
                return _textures[i];
            }

        return null;
    }

    public void TogglePack(TexturePack pack)
    {
        if (pack == null)
        {
            IsUsingCustomTexturePack = false;
            return;
        }

        if (pack.Blocks.Count != 0)
            foreach (var texture in pack.Blocks)
                UpdateCustomTexture(pack, $"{texture.Key}", texture.Value);

        if (pack.FX.Count != 0)
            foreach (var texture in pack.FX)
                UpdateCustomTexture(pack, $"{texture.Key}", texture.Value);

        // Make sure there is a pointter texture
        if (pack.Input.Count != 0)
        {
            foreach (var texture in pack.Input) UpdateCustomTexture(pack, $"{texture.Key}", texture.Value);

            Window.Instance.GetPointer().UpdateCursorTexture();
        }

        CurrentTexturePack = pack;
        GameSettings.Instance.SetValue("Strings", "Current Texture Pack", pack.PackMD5());
    }

    public void RemoveTexture(string name)
    {
        for (var i = 0; i < _textures.Count; i++)
            if (_textures[i].Name.ToLower().Equals(name.ToLower()))
                _textures.RemoveAt(i);
    }

    public void RemoveTexture(Texture2D texture2D)
    {
        for (var i = 0; i < _textures.Count; i++)
            if (_textures[i].Texture2D == texture2D)
            {
                // Dispose of the texture
                texture2D.Dispose();
                _textures.RemoveAt(i);
            }
    }
}

public class Texture
{
    public Texture(string name, Texture2D texture2D)
    {
        Name = name;
        Texture2D = texture2D;
    }

    public string Name { get; }
    public Texture2D Texture2D { get; set; }
}