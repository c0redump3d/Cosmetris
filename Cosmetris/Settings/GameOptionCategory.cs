/*
 * GameOptionCategory.cs is part of Cosmetris.
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
using System.Collections.Generic;

namespace Cosmetris.Settings;

public class GameOptionCategory
{
    public GameOptionCategory()
    {
        CategoryOptions = new List<GameOptionBase>();
    }

    public GameOptionCategory(string name)
    {
        CategoryName = name;
        CategoryOptions = new List<GameOptionBase>();
    }

    public string CategoryName { get; set; }
    public List<GameOptionBase> CategoryOptions { get; set; }

    public void AddOption(GameOptionBase gameOption)
    {
        // Make sure we don't have a duplicate option
        foreach (var option in CategoryOptions)
            if (option.Name.Equals(gameOption.Name, StringComparison.OrdinalIgnoreCase))
                return;
        
        CategoryOptions.Add(gameOption);
    }

    public GameOptionBase GetOptionValue(string name)
    {
        foreach (var option in CategoryOptions)
            if (option.Name == name)
                return option;

        throw new Exception($"Unable to find game option with name {name}.");
    }

    public void SetValue(string name, object value)
    {
        foreach (var option in CategoryOptions)
            if (option.Name == name)
            {
                if (option is GameOption<object> concreteOption)
                    concreteOption.SetValue(value);
                else
                    throw new Exception("Invalid value type for this game option.");

                return;
            }

        throw new Exception($"Unable to find game option with name {name}.");
    }
}