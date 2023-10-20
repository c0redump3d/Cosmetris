/*
 * TexturePackManager.cs is part of Cosmetris.
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
using System.IO.Compression;
using System.Linq;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Settings;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Packs.TexturePacks;

public class TexturePackManager
{
    private readonly GraphicsDevice _graphicsDevice;

    public TexturePackManager(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    private void LoadPackIcon(TexturePack pack, string folderPath)
    {
        var iconPath = Path.Combine(folderPath, "pack.png");
        if (!File.Exists(iconPath)) return;
        using var stream = File.OpenRead(iconPath);
        stream.Seek(0, SeekOrigin.Begin);
        pack.PackIcon = Texture2D.FromStream(_graphicsDevice, stream);
    }

    public List<TexturePack> LoadAllAvailableTexturePacks()
    {
        var texturePacksFolderPath = GameSettings.Instance.GetTexturePacksFolderPath();

        // Load packs from folders
        var packs = Directory.GetDirectories(texturePacksFolderPath).Select(folder => LoadFromFolder(folder)).ToList();
        packs.AddRange(Directory.GetFiles(texturePacksFolderPath, "*.zip").Select(zipFile => LoadFromZip(zipFile)));

        // Load packs from zip files

        return packs;
    }

    private TexturePack LoadFromFolder(string folderPath)
    {
        var pack = new TexturePack();
        var packInfoPath = Path.Combine(folderPath, "pack.cos");

        if (File.Exists(packInfoPath)) ParsePackInfo(pack, File.ReadAllLines(packInfoPath));

        LoadPackIcon(pack, folderPath);
        LoadTexturesFromFolder(pack.Blocks, Path.Combine(folderPath, "Block"));
        LoadTexturesFromFolder(pack.RubbleFX, Path.Combine(folderPath, "FX/Rubble"));
        LoadTexturesFromFolder(pack.FX, Path.Combine(folderPath, "FX"));
        LoadTexturesFromFolder(pack.Input, Path.Combine(folderPath, "Input"));

        return pack;
    }

    private TexturePack LoadFromZip(string zipFilePath)
    {
        var pack = new TexturePack();

        using var zip = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in zip.Entries)
            if (entry.FullName.EndsWith("pack.cos"))
            {
                using var stream = entry.Open();
                stream.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(stream);
                var lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                ParsePackInfo(pack, lines);
            }
            else if (entry.FullName.EndsWith("pack.png"))
            {
                using var stream = entry.Open();
                stream.Seek(0, SeekOrigin.Begin);
                pack.PackIcon = Texture2D.FromStream(Window.Instance.GetGraphicsDevice(), stream);
            }
            else if (entry.FullName.StartsWith("Block/"))
            {
                LoadTextureFromZipEntry(pack.Blocks, entry);
            }
            else if (entry.FullName.StartsWith("FX/Rubble/"))
            {
                LoadTextureFromZipEntry(pack.RubbleFX, entry);
            }
            else if (entry.FullName.StartsWith("FX/"))
            {
                LoadTextureFromZipEntry(pack.FX, entry);
            }
            else if (entry.FullName.StartsWith("Input/"))
            {
                LoadTextureFromZipEntry(pack.Input, entry);
            }

        return pack;
    }

    private void ParsePackInfo(TexturePack pack, string[] lines)
    {
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "packName":
                        pack.PackName = value;
                        break;
                    case "packDescription":
                        pack.PackDescription = value;
                        break;
                    case "packCreator":
                        pack.PackCreator = value;
                        break;
                    case "packVersion":
                        pack.PackVersion = value;
                        break;
                    case "singleBlockTexture":
                        pack.SingleBlockTexture = bool.TryParse(value, out var result) && result;
                        break;
                }
            }
        }
    }

    private void LoadTexturesFromFolder(Dictionary<string, Texture2D> textures, string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        foreach (var filePath in Directory.GetFiles(folderPath, "*.png"))
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            using var stream = File.OpenRead(filePath);
            stream.Seek(0, SeekOrigin.Begin);
            textures[name] = Texture2D.FromStream(_graphicsDevice, stream);
        }
    }

    private void LoadTextureFromZipEntry(Dictionary<string, Texture2D> textures, ZipArchiveEntry entry)
    {
        var name = Path.GetFileNameWithoutExtension(entry.FullName);
        using var stream = entry.Open();
        stream.Seek(0, SeekOrigin.Begin);
        textures[name] = Texture2D.FromStream(_graphicsDevice, stream);
    }

    public void LoadDefaultBlocks()
    {
        TextureManager.Instance.AddTexture("i_block", "Block/i_block");
        TextureManager.Instance.AddTexture("t_block", "Block/t_block");
        TextureManager.Instance.AddTexture("z_block", "Block/z_block");
        TextureManager.Instance.AddTexture("s_block", "Block/s_block");
        TextureManager.Instance.AddTexture("j_block", "Block/j_block");
        TextureManager.Instance.AddTexture("l_block", "Block/l_block");
        TextureManager.Instance.AddTexture("o_block", "Block/o_block");
    }

    public void LoadSaved()
    {
        // Check if we have a custom texture pack
        var customPack = GameSettings.Instance.GetValue<string>("Strings", "Current Texture Pack");
        if (customPack is null or "") return;
        var packs = LoadAllAvailableTexturePacks();
        var pack = packs.FirstOrDefault(texturePack => texturePack.PackMD5() == customPack);

        if (pack != null)
            TextureManager.Instance.TogglePack(pack);
    }
}