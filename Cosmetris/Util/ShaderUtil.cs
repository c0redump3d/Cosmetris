/*
 * ShaderUtil.cs is part of Cosmetris.
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

using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Util;

public static class ShaderUtil
{
    public static Effect LoadEffect(GraphicsDevice graphicsDevice, string name)
    {
        var directories = Directory.GetDirectories(ContentUtil.Instance.GetPath("Shaders"));
        string shaderPath = null;

        // Iterate through all directories, including the root directory
        foreach (var dir in directories.Append(ContentUtil.Instance.GetPath("Shaders")))
        {
            var candidatePath = Path.Combine(dir, name + ".mgfxo");
            if (File.Exists(candidatePath))
            {
                shaderPath = candidatePath;
                break;
            }
        }

        if (shaderPath == null)
            throw new FileNotFoundException($"Shader '{name}' not found in any subdirectory of the 'Shaders' folder.");

        var bytecode = File.ReadAllBytes(shaderPath);
        return new Effect(graphicsDevice, bytecode);
    }
}