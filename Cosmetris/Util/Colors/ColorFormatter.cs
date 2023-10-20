/*
 * ColorFormatter.cs is part of Cosmetris.
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

namespace Cosmetris.Util.Colors;

public static class ColorFormatter
{
    private static readonly Dictionary<string, Microsoft.Xna.Framework.Color> _colorMap;

    static ColorFormatter()
    {
        _colorMap = new Dictionary<string, Microsoft.Xna.Framework.Color>
        {
            { "red", Microsoft.Xna.Framework.Color.Red },
            { "green", Microsoft.Xna.Framework.Color.Green },
            { "blue", Microsoft.Xna.Framework.Color.Blue },
            { "yellow", Microsoft.Xna.Framework.Color.Yellow },
            { "cyan", Microsoft.Xna.Framework.Color.Cyan },
            { "magenta", Microsoft.Xna.Framework.Color.Magenta },
            { "white", Microsoft.Xna.Framework.Color.White },
            { "black", Microsoft.Xna.Framework.Color.Black },
            { "gray", Microsoft.Xna.Framework.Color.Gray },
            { "darkgray", Microsoft.Xna.Framework.Color.DarkGray },
            { "lightgray", Microsoft.Xna.Framework.Color.LightGray },
            { "orange", Microsoft.Xna.Framework.Color.Orange },
            { "brown", Microsoft.Xna.Framework.Color.Brown },
            { "pink", Microsoft.Xna.Framework.Color.Pink },
            { "purple", Microsoft.Xna.Framework.Color.Purple },
            { "transparent", Microsoft.Xna.Framework.Color.Transparent },
            { "cornflowerblue", new Microsoft.Xna.Framework.Color(100, 149, 237) }
        };
    }

    public static IEnumerable<(string text, Microsoft.Xna.Framework.Color currentColor)> ParseFormattedString(string formattedText,
        Microsoft.Xna.Framework.Color defaultColor)
    {
        var currentColor = defaultColor;
        var textStart = 0;
        var textLength = 0;
        var inFormatSpecifier = false;

        for (var i = 0; i < formattedText.Length; i++)
            if (formattedText[i] == '{')
            {
                if (textLength > 0)
                {
                    yield return (formattedText.Substring(textStart, textLength), currentColor);
                    textLength = 0;
                }

                inFormatSpecifier = true;
                textStart = i + 1;
            }
            else if (formattedText[i] == '}' && inFormatSpecifier)
            {
                var colorName = formattedText.Substring(textStart, i - textStart);

                if (_colorMap.TryGetValue(colorName, out var color)) currentColor = color;

                inFormatSpecifier = false;
                textLength = 0;
                textStart = i + 1;
            }
            else if (!inFormatSpecifier)
            {
                textLength++;
            }

        // Handle any remaining text after the last format specifier or if there are no format specifiers
        if (textLength > 0) yield return (formattedText.Substring(textStart, textLength), currentColor);
    }
}