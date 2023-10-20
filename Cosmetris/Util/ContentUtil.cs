/*
 * ContentUtil.cs is part of Cosmetris.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OperatingSystem = Cosmetris.Util.OperatingSystem;

namespace Cosmetris.Util;

public class ContentUtil
{
    private string Location = "";

    private ContentUtil()
    {
        var provider = new PlatformProvider();
        provider.Initialize();
        Location = string.Empty;
        ProviderInit(provider);
    }

    public string RootDirPath => Path.Combine(Location, "Content");

    public static ContentUtil Instance { get; } = new();

    private void ProviderInit(PlatformProvider provider)
    {
        if (provider.OperatingSystem == OperatingSystem.MacOSX)
            Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
        else if (provider.OperatingSystem == OperatingSystem.Linux)
            Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

        if (Directory.Exists(Location))
            return;

        Location = AppDomain.CurrentDomain.BaseDirectory;
    }

    public string GetPath(string path)
    {
        return Path.Combine(RootDirPath, path);
    }
    
    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}