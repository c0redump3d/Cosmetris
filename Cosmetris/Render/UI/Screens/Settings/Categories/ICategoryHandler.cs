/*
 * ICategoryHandler.cs is part of Cosmetris.
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
using Cosmetris.Settings;
using Cosmetris.Render.UI.Text;

namespace Cosmetris.Render.UI.Screens.Settings.Categories;

public interface ICategoryHandler : IDisposable
{
    int Size { get; }
    bool OptionWasChanged { get; set; }

    public void Handle(SettingsScreen screen, GameOptionBase option, float xOffset, float yOffset, int currentPage,  SettingsCategory currentCategory)
    {
    }
    
    public void Initialize()
    {
    }
}