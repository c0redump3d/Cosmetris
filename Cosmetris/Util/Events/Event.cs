/*
 * Event.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Util.Events;

public class Event
{
    private Func<object> func1;
    private object value;
    private Action<GameTime> func2;
    private Action<SpriteBatch> func3;
    private string name;
    
    private bool isReoccurring;
    private bool callOnChange;
    private bool? previousValue;

    public string Name => name;

    public Event(string name, Func<object> func1, object value, Action<GameTime> func2, Action<SpriteBatch> func3 = null, bool isReoccurring = false, bool callOnChange = false)
    {
        this.name = name;
        this.func1 = func1;
        this.value = value;
        this.func2 = func2;
        this.func3 = func3;
        this.isReoccurring = isReoccurring;
        this.callOnChange = callOnChange;

        // Subscribe to UpdateEvent
        Window.Instance.UpdateEvent += Update;
        if(func3 != null)
            Window.Instance.DrawEvent += Draw;
    }

    public void Update(object sender, GameTime gameTime)
    {
        var currentValue = (bool)func1.Invoke();

        if (callOnChange)
        {
            if (currentValue != previousValue && currentValue.Equals(value))
            {
                func2.Invoke(gameTime);
            }
        }
        else if (currentValue.Equals(value))
        {
            func2.Invoke(gameTime);
        }

        previousValue = currentValue;

        if (!isReoccurring && currentValue.Equals(value))
        {
            // Unsubscribe and mark for removal
            Window.Instance.UpdateEvent -= Update;
            if (func3 != null)
                Window.Instance.DrawEvent -= Draw;
            EventManager.Instance.EnqueueForRemoval(this);
        }
    }

    public void Draw(object sender, SpriteBatch spriteBatch)
    {
        func3?.Invoke(spriteBatch);
    }
}