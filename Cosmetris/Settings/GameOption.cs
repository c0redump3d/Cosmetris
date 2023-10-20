/*
 * GameOption.cs is part of Cosmetris.
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

﻿namespace Cosmetris.Settings;

public class GameOptionBase
{
    public string Name { get; init; }
    public string CategoryName { get; init; }

    public virtual object GetValue()
    {
        // This is just a placeholder; 
        // the real implementation will be in the derived class.
        return null;
    }

    public virtual void SetValue(object value)
    {
        // This is just a placeholder; 
        // the real implementation will be in the derived class.
    }
}

public class GameOption<T> : GameOptionBase
{
    /// <summary>
    ///     A generic type that can store any value.
    /// </summary>
    public T Value;

    //NEEDED FOR XML SERIALIZATION!!
    public GameOption()
    {
    }

    public GameOption(string category, string name, T defaultValue)
    {
        CategoryName = category;
        Name = name;
        Value = defaultValue;
    }

    public override void SetValue(object value)
    {
        // We attempt to cast the object back to the specific type T
        // before assigning it. If the cast fails, an exception will be thrown.
        Value = (T)value;
    }

    public override object GetValue()
    {
        return Value;
    }
}