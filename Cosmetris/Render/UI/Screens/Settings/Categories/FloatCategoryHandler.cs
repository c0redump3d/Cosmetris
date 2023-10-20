/*
 * FloatCategoryHandler.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;

namespace Cosmetris.Render.UI.Screens.Settings.Categories;

public class FloatCategoryHandler : ICategoryHandler
{
    public int Size => 1;
    public bool OptionWasChanged { get; set; }
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);

    public void Handle(SettingsScreen screen, GameOptionBase option, float xOffset, float yOffset, int currentPage,
        SettingsCategory currentCategory)
    {
        var floatOption = (GameOption<float>)option;
        var slider = new Slider(
            new Vector2(0,
                yOffset + 15), // Assuming you have the width of the slider as sliderWidth
            new Vector2(300, 40),
            Slider.SliderType.Rectangle, 0f, 1f, floatOption.Value, Slider.ValueFormat.Percentage,
            "orbitron", 24);
        slider.OnValueChanged += (sender, value) =>
        {
            floatOption.SetValue(value.Value);
            //_optionsChanged = true;
            Window.Instance.GetSoundManager().UpdateVolumes();
            OptionWasChanged = true;
        };

        var label = new Label(floatOption.Name,
            new Vector2(0,
                yOffset), // Assuming you have the width of the label as labelWidth
            _defaultFont,
            Microsoft.Xna.Framework.Color.White);

        label.Position = new Vector2(xOffset - label.Size.X / 2, label.Position.Y);

        slider.Position = new Vector2(xOffset - slider.Size.X / 2,
            slider.Position.Y + label.Size.Y / 2.0f + 2);

        screen.AddControlToPage(currentCategory, slider, currentPage);
        screen.AddControlToPage(currentCategory, label, currentPage);
    }
    
    public void Dispose()
    {
    }
}