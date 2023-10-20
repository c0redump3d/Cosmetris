/*
 * Timer.cs is part of Cosmetris.
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

namespace Cosmetris.Util;

public class Timer
{
    private static Timer instance;

    //Allow for multiple timers to be created at once.
    private readonly List<TimerFunc> timers = new();
    private readonly List<TimerFunc> timersToAdd = new(); // New list to collect timers to be added
    private readonly List<TimerFunc> timersToRemove = new();
    private int totalID = -1;

    private Timer()
    {
        timers = new List<TimerFunc>();
    }

    public static Timer Instance
    {
        get
        {
            if (instance == null)
                instance = new Timer();
            return instance;
        }
    }

    /// <summary>
    ///     Creates a timer with the specified time in milliseconds.
    /// </summary>
    /// <param name="time">Time in milliseconds</param>
    /// <param name="runFunc">Function to run after timer has completed (Optional)</param>
    public void CreateTimer(float time, EventHandler runFunc, string name = "", bool ignoreRemoval = false)
    {
        //If a timer name is specified, we assume we do not want another of the same timer running concurrently!
        if (!name.Equals(""))
            foreach (var timer in timers)
                if (name.Equals(timer.Name) && timer.Time >= 0)
                    //Gui.Instance.AddDebugMessage($"Timer with name {name} already created!");
                    return;

        totalID++;
        var t = new TimerFunc(totalID, name, time);
        t.Completed += runFunc;
        t.IgnoreRemoval = ignoreRemoval;
        timersToAdd.Add(t);
    }

    /// <summary>
    ///     Creates a timer with the specified time in milliseconds.
    /// </summary>
    /// <param name="time">Time in milliseconds</param>
    public void CreateTimer(float time, string name = "")
    {
        if (!name.Equals(""))
            foreach (var timer in timers)
                if (name.Equals(timer.Name) && timer.Time >= 0)
                    //Gui.Instance.AddDebugMessage($"Timer with name {name} already created!");
                    return;

        totalID++;
        timersToAdd.Add(new TimerFunc(totalID, name, time));
    }

    public void UpdateTimers(GameTime gameTime)
    {
        if ((timersToAdd.Count <= 0 && timers.Count <= 0) || Window.Instance.ScreenRenderer().CurrentlyAnimating())
            return;

        if (timersToRemove.Count > 0)
        {
            foreach (var toRemove in timersToRemove) timers.Remove(toRemove);
            timersToAdd.Clear();
            return;
        }

        var completedTimers = new List<TimerFunc>();

        // subtract from our total time for active timer
        foreach (var timer in timers)
            if (timer.Time >= 0)
            {
                // Raise timer tick event
                timer.OnTick(new EventArgs());
                timer.Time -= 1.0f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                timer.OnCompleted(new EventArgs());
                completedTimers.Add(timer);
            }

        // Remove completed timers and add new timers after iterating
        foreach (var completedTimer in completedTimers) timers.Remove(completedTimer);

        timers.AddRange(timersToAdd); // Add all the timers from timersToAdd list
        timersToAdd.Clear(); // Clear the timersToAdd list for next time
    }

    /// <summary>
    ///     Allows access to a specific timer if name is provided
    /// </summary>
    /// <param name="name">Name of timer</param>
    /// <returns>TimerUtil</returns>
    public TimerFunc GetTimer(string name)
    {
        foreach (var timer in timers)
            if (timer.Name.Equals(name))
                return timer;

        //Gui.Instance.AddDebugMessage($"Unable to find timer with name: {name}");
        return null;
    }

    public void ClearTimers()
    {
        timersToRemove.Clear();
        foreach (var activeTimer in timers)
        {
            if (activeTimer.IgnoreRemoval)
                continue;

            timersToRemove.Add(activeTimer);
        }

        timersToAdd.Clear();
    }
}

public class TimerFunc
{
    public EventHandler Completed;
    public EventHandler Tick;

    public TimerFunc(int id, string name, float time)
    {
        ID = id;
        Name = name;
        Time = time;
    }

    public int ID { get; }
    public string Name { get; }
    public bool IgnoreRemoval { get; set; }
    public float Time { get; set; }
    public Action Function { get; }

    public void OnTick(EventArgs e)
    {
        var handler = Tick;
        handler?.Invoke(this, e);
    }

    public void OnCompleted(EventArgs e)
    {
        var handler = Completed;
        handler?.Invoke(this, e);
    }
}