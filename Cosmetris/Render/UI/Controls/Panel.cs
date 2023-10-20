/*
 * Panel.cs is part of Cosmetris.
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
using System.Linq;
using Cosmetris.Render.UI.Controls.Animation;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Cosmetris.Render.UI.Controls;

public class Panel : Control
{
    protected Vector2 _finalSize;

    public Panel(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = Vector2.One; // start with a small size
        _finalSize = size;
        Layer = 0.15f;

        // Add a scale animation to expand the panel from the center
        Initialize();
    }

    protected override void Initialize()
    {
        SetAnimation(new ScaleAnimation(this, Size, _finalSize, 0.40f));
        SetClosingAnimation(new ScaleClosingAnimation(this, _finalSize, Vector2.Zero, 0.30f));
    }

    protected float BorderRadius { get; } = 12.5f;

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var scaledPos = GetActualPosition();
        RenderUtil.DrawBorderRoundRect(scaledPos.X, scaledPos.Y, Size.X, Size.Y, BorderRadius, PanelBGColor,
            PanelBorderColor, 2.75f, Layer);
        base.Draw(spriteBatch, gameTime);
    }

    protected override RectangleF GetHoverRect()
    {
        // panels do not need a hover rect
        return new RectangleF(0, 0, 0, 0);
    }

    public override void SetPosition(int i, int i1)
    {
        Position = new Vector2(i, i1);
    }

    public override void SetPosition(Vector2 position)
    {
        Position = position;

        Position -= Size / 2f;
    }

    public void AddControl(Control control)
    {
        control.PositionOffset = GetActualPosition();
        control.Parent = this;
        control.Layer = Layer + 0.01f;
        ChildControls.Add(control);
    }

    public void RemoveControl(Control control)
    {
        control.Parent = null;
        ChildControls.Remove(control);
        control.Dispose();
    }

    public void RemoveControl(string controlName)
    {
        List<Control> controlsToRemove = new();
        foreach (var control in ChildControls.Where(control => control.Tag.Equals(controlName)))
            controlsToRemove.Add(control);

        foreach (var control in controlsToRemove)
        {
            control.Parent = null;
            ChildControls.Remove(control);
            control.Dispose();
        }
    }

    public void RemoveControlTaggedContains(string name)
    {
        List<Control> controlsToRemove = new();
        foreach (var control in ChildControls.Where(control => control.Tag.Contains(name)))
            controlsToRemove.Add(control);

        foreach (var control in controlsToRemove)
        {
            control.Parent = null;
            ChildControls.Remove(control);
        }
    }
    
    public Vector2 GetFinalSize()
    {
        return _finalSize;
    }
}