/*
 * ColorCategoryHandler.cs is part of Cosmetris.
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

using Cosmetris.Render.UI.Controls;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Screens.Settings.Categories;

public class ColorCategoryHandler : ICategoryHandler
{
    public int Size => 2;
    public bool OptionWasChanged { get; set; }

    public void Handle(SettingsScreen screen, GameOptionBase option, float xOffset, float yOffset, int currentPage,
        SettingsCategory currentCategory)
    {
        var colorOption = (GameOption<Microsoft.Xna.Framework.Color>)option;
        var colorPicker = new ColorPicker(colorOption.Name, xOffset, yOffset, 100, (Microsoft.Xna.Framework.Color)colorOption.GetValue());
        colorPicker.SetPosition(new Vector2(xOffset, yOffset));
        colorPicker.OnColorChanged += (sender, color) =>
        {
            colorOption.SetValue(color);
            OptionWasChanged = true;
        };
                        
        screen.AddControlToPage(currentCategory, colorPicker, currentPage);
    }

    public void Dispose()
    {
    }
}