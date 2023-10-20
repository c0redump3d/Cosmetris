/*
 * ControlColor.cs is part of Cosmetris.
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

namespace Cosmetris.Render.UI.Color;

/// <summary>
///     A class that holds all the colors for a control.
///     Important to note that a control should usually ALWAYS set the colors in the constructor.
/// </summary>
public abstract class ControlColor
{
    // Main Panel background/border -- holds all controls & text
    public Microsoft.Xna.Framework.Color PanelBGColor { get; set; } = new(20, 20, 20, 200);
    public Microsoft.Xna.Framework.Color PanelBorderColor { get; set; } = new(50, 50, 50, 220);
    
    public Microsoft.Xna.Framework.Color MessageBoxBGColor { get; set; } = new(20, 20, 20, 230);
    public Microsoft.Xna.Framework.Color MessageBoxBorderColor { get; set; } = new(50, 50, 50, 240);

// Normal controls -- used when there are controls inside a panel
    public Microsoft.Xna.Framework.Color ControlBorderColor { get; set; } = new(70, 70, 70, 230);
    public Microsoft.Xna.Framework.Color ControlHoverBorderColor { get; set; } = new(85, 85, 85, 240);
    public Microsoft.Xna.Framework.Color ControlPressedBorderColor { get; set; } = new(100, 100, 100, 250);
    public Microsoft.Xna.Framework.Color ControlBGColor { get; set; } = new(30, 30, 30, 210);
    public Microsoft.Xna.Framework.Color ControlHoverBGColor { get; set; } = new(40, 40, 40, 220);
    public Microsoft.Xna.Framework.Color ControlPressedBGColor { get; set; } = new(35, 35, 35, 240);

// Child controls -- used when there are more children controls inside a parent control (not panel)
    public Microsoft.Xna.Framework.Color ChildControlBorderColor { get; set; } = new(60, 60, 60, 230);
    public Microsoft.Xna.Framework.Color ChildControlHoverBorderColor { get; set; } = new(75, 75, 75, 240);
    public Microsoft.Xna.Framework.Color ChildControlPressedBorderColor { get; set; } = new(90, 90, 90, 250);
    public Microsoft.Xna.Framework.Color ChildControlBGColor { get; set; } = new(25, 25, 25, 210);
    public Microsoft.Xna.Framework.Color ChildControlHoverBGColor { get; set; } = new(35, 35, 35, 220);
    public Microsoft.Xna.Framework.Color ChildControlPressedBGColor { get; set; } = new(30, 30, 30, 240);

// Control Text Colors (Used for both parent and child controls)
    public Microsoft.Xna.Framework.Color ControlTextColor { get; set; } = new(150, 150, 150, 255); // Light Grey
    public Microsoft.Xna.Framework.Color ControlTextHoverColor { get; set; } = new(200, 200, 200, 255); // Lighter Grey

    public Microsoft.Xna.Framework.Color ControlTextPressedColor { get; set; } =
        new(200, 200, 200, 255); // Lighter Grey

    public Microsoft.Xna.Framework.Color ControlTextDisabledColor { get; set; } = new(80, 80, 80, 255); // Dark Grey

// Panel Text Colors (Used for both parent and child controls)
    public Microsoft.Xna.Framework.Color PanelHeaderTextColor { get; set; } = new(150, 150, 150, 255); // Light Grey
    public Microsoft.Xna.Framework.Color PanelInfoTextColor { get; set; } = new(150, 150, 150, 255); // Light Grey
}