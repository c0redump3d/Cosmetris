/*
 * ElapsedTimer.cs is part of Cosmetris.
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

using System.Text;
using Microsoft.Xna.Framework;

namespace Cosmetris.Util;

public class ElapsedTimer
{
    public ElapsedTimer()
    {
        Reset();
    }

    public float ElapsedTime { get; set; }

    public void Reset()
    {
        ElapsedTime = 0;
    }

    public void Update(GameTime gameTime)
    {
        ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public StringBuilder GetElapsedTimeString()
    {
        StringBuilder sb = new();
        var minutes = (int)ElapsedTime / 60;
        var seconds = (int)ElapsedTime % 60;
        var milliseconds = (int)(ElapsedTime * 1000) % 1000;

        sb.Append(minutes.ToString("00"));
        sb.Append(':');
        sb.Append(seconds.ToString("00"));
        sb.Append(':');
        sb.Append(milliseconds.ToString("000"));

        return sb;
    }

    public bool HasElapsed(float seconds)
    {
        return ElapsedTime >= seconds;
    }
}