/*
 * EventManager.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Util.Events;

public class EventManager
{
    private List<Event> events;
    private Queue<Event> eventsToRemove;

    private static EventManager instance;
    public static EventManager Instance => instance ??= new EventManager();

    private EventManager()
    {
        events = new List<Event>();
        eventsToRemove = new Queue<Event>();
        
        // Subscribe to UpdateEvent
        Window.Instance.UpdateEvent += Update;
    }

    public void CreateEvent(string name, Func<object> func1, object value, Action<GameTime> func2, Action<SpriteBatch> func3 = null, bool isReoccurring = false, bool callOnChange = false)
    {
        if (string.IsNullOrEmpty(name) || !events.Exists(e => e.Name == name))
        {
            var newEvent = new Event(name, func1, value, func2, func3, isReoccurring, callOnChange);
            events.Add(newEvent);
        }
    }

    public void CreateEvent(Func<object> func1, object value, Action<GameTime> func2, Action<SpriteBatch> func3 = null, bool isReoccurring = false, bool callOnChange = false)
    {
        CreateEvent(null, func1, value, func2, func3, isReoccurring, callOnChange);
    }

    public void Update(object sender, GameTime gameTime)
    {
        while (eventsToRemove.Count > 0)
        {
            var eventToRemove = eventsToRemove.Dequeue();
            events.Remove(eventToRemove);
        }
    }
    
    public void Remove(Event eventToRemove)
    {
        if (events.Contains(eventToRemove))
        {
            // First, unsubscribe the event from Window.Instance's events
            Window.Instance.UpdateEvent -= eventToRemove.Update;
            Window.Instance.DrawEvent -= eventToRemove.Draw;

            // Now, remove from the list
            events.Remove(eventToRemove);
        }
    }

    public void EnqueueForRemoval(Event eventToRemove)
    {
        eventsToRemove.Enqueue(eventToRemove);
    }
}