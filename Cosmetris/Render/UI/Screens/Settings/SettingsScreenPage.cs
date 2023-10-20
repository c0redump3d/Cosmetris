/*
 * SettingsScreenPage.cs is part of Cosmetris.
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

namespace Cosmetris.Render.UI.Screens.Settings;

public class SettingsCategoryPage
{
    public SettingsCategoryPage()
    {
        CategoryOptions = new List<Control>();
    }

    public List<Control> CategoryOptions { get; set; }

    public void AddControl(Control control)
    {
        CategoryOptions.Add(control);
    }

    public void RemoveControl(Control control)
    {
        CategoryOptions.Remove(control);
    }
}