/*
 * TexturePack.cs is part of Cosmetris.
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
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Packs.TexturePacks;

public class TexturePack
{
    public string PackName { get; set; }
    public string PackDescription { get; set; }
    public string PackCreator { get; set; }
    public string PackVersion { get; set; }
    public Texture2D PackIcon { get; set; }

    public Dictionary<string, Texture2D> Blocks { get; } = new();
    public Dictionary<string, Texture2D> FX { get; } = new();
    public Dictionary<string, Texture2D> RubbleFX { get; } = new();
    public Dictionary<string, Texture2D> Input { get; } = new();

    public bool SingleBlockTexture { get; set; }

    public string PackMD5()
    {
        if (!PackName.Equals(""))
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(PackName));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

        return "";
    }
}