/*
 * PlatformProvider.cs is part of Cosmetris.
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
using System.Runtime.InteropServices;

namespace Cosmetris.Util;

internal enum OperatingSystem
{
    Windows,
    Linux,
    MacOSX,
    Unknown
}

internal class PlatformProvider
{
    private bool _init;

    public OperatingSystem OperatingSystem { get; private set; }

    public string Rid
    {
        get
        {
            if (OperatingSystem == OperatingSystem.Windows && Environment.Is64BitProcess)
                return "win-x64";
            if (OperatingSystem == OperatingSystem.Windows && !Environment.Is64BitProcess)
                return "win-x86";
            if (OperatingSystem == OperatingSystem.Linux)
                return "linux-x64";
            return OperatingSystem == OperatingSystem.MacOSX ? "osx" : "unknown";
        }
    }


    public void Initialize()
    {
        if (_init)
            return;
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                OperatingSystem = OperatingSystem.Windows;
                break;
            case PlatformID.Unix:
                OperatingSystem = OperatingSystem.Linux;
                var num = IntPtr.Zero;
                try
                {
                    num = Marshal.AllocHGlobal(8192);
                    break;
                }
                catch
                {
                    break;
                }
                finally
                {
                    if (num != IntPtr.Zero)
                        Marshal.FreeHGlobal(num);
                }
            case PlatformID.MacOSX:
                OperatingSystem = OperatingSystem.MacOSX;
                break;
            default:
                OperatingSystem = OperatingSystem.Unknown;
                break;
        }

        _init = true;
    }
}