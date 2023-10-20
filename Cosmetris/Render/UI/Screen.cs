/*
 * Screen.cs is part of Cosmetris.
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cosmetris.Input;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI;

public class Screen
{
    private int _idCounter;
    private readonly List<Control> _controlsToRemove = new();

    protected readonly List<Control> Controls;
    protected readonly UIScalingManager ScalingManager = Window.Instance.ScalingManager;

    /// <summary>
    ///     Add controls to this action to be added to the screen.
    ///     This will be called at initialization and when the BufferWidth has changed.
    /// </summary>
    protected Action LayoutControls;

    protected MessageBox MessageBox;
    public DebugConsole DebugConsole;

    public Screen()
    {
        Timer.Instance.ClearTimers();
        Window.Instance.GetPointer().ClearEvents();
        Controller.Instance.ClearEvents();

        Controls = new List<Control>();
    }

    public virtual void OnInit()
    {
    }

    public virtual void OnClose()
    {
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, sortMode: SpriteSortMode.BackToFront);
        foreach (var control in Controls)
            if (!control.IsImportant)
                control.Draw(spriteBatch, gameTime);
        spriteBatch.End();
    }

    public virtual void DrawImportant(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);

        foreach (var control in Controls)
            if (control.IsImportant && !(control is MessageBox))
                control.Draw(spriteBatch, gameTime);

        foreach (var control in Controls)
            if (control.IsImportant && control is MessageBox box)
            {
                box.Draw(spriteBatch, gameTime);
                MessageBox = box;
            }

        spriteBatch.End();
    }

    public virtual void Update(GameTime gameTime)
    {
        foreach (var control in Controls)
        {
            
            if (MessageBox != null)
            {
                if (control is MessageBox box)
                    box.CanHover = true;
                else
                    control.CanHover = false;
            }else
                control.CanHover = true;

            control.Update(gameTime);

            if (control.IsMarkedForDeletion) _controlsToRemove.Add(control);
        }

        foreach (var control in _controlsToRemove)
        {
            if (control is MessageBox)
                MessageBox = null;

            Controls.Remove(control);
        }
    }

    public void AddControl(Control control)
    {
        if (control is MessageBox)
            if (Controls.Find(c => c is MessageBox) is MessageBox)
                return;
        if (control is DebugConsole)
        {
            if (DebugConsole != null)
                control = DebugConsole;
        }
        
        if(Controls.Contains(control))
            return;

        Controls.Add(control);
    }

    public void RemoveControl(Control control)
    {
        Controls.Remove(control);
        control.Dispose();
    }
    
    public void AddConsoleMessage(string message, [CallerMemberName] string caller = "", MessageType type = MessageType.Info)
    {
        DebugConsole?.AddMessage(message, caller, type);
    }

    public virtual void OnResize()
    {
        LayoutControls?.Invoke();
        foreach (var control in Controls)
        {
            control.UpdateSize();
            if(control.ChildControls.Count > 0)
                control.UpdateSize();
        }
    }

    public int GetNextId()
    {
        return _idCounter++;
    }

    public List<Control> GetControls()
    {
        return Controls;
    }

    public void ClearControls()
    {
        Controls.Clear();
    }
}