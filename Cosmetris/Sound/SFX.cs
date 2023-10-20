/*
 * SFX.cs is part of Cosmetris.
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

using Microsoft.Xna.Framework.Audio;

namespace Cosmetris.Sound;

public class SFX
{
    public SFX(SoundEffect sfx, string name, string path, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        SoundEffect = sfx;
        Name = name;
        Path = path;
        Volume = volume;
        Pitch = pitch;
        Pan = pan;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public float Volume { get; set; }
    public float Pitch { get; set; }
    public float Pan { get; set; }

    public SoundEffect SoundEffect { get; set; }
}