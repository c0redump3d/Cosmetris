/*
 * BoolCategoryHandler.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Screens.Settings.Categories;

public class BoolCategoryHandler : ICategoryHandler
{
    public int Size => 1;
    public bool OptionWasChanged { get; set; }
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);

    public void Handle(SettingsScreen screen, GameOptionBase option, float xOffset, float yOffset, int currentPage,
        SettingsCategory currentCategory)
    {
        if (currentCategory == null) throw new ArgumentNullException(nameof(currentCategory));
        var boolOption = (GameOption<bool>)option;
        var toggleButton = new Button(boolOption.Name + ": " + boolOption.Value,
            0,
            yOffset, // Assuming you have the width of the bool option button as boolOptionWidth
            (sender, e) =>
            {
                GameSettings.Instance.SetValue(currentCategory.CategoryName, boolOption.Name, !boolOption.Value);
                ((Button)sender)?.SetText(boolOption.Name + ": " + boolOption.Value);
                OptionWasChanged = true;
            }, _defaultFont);

        toggleButton.Position = new Vector2(xOffset - toggleButton.Size.X / 2, toggleButton.Position.Y);

        screen.AddControlToPage(currentCategory, toggleButton, currentPage);
    }

    public void Dispose()
    {
    }
}