/*
 * Object.cs is part of Cosmetris.
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
using Cosmetris.Input;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects;

public class Object : IDisposable
{
    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;

    public readonly Font DebugFont = FontRenderer.Instance.GetFont("debug", 12);
    // A mask color for the texture

    protected List<Object> _children;

    public Object(string name, Vector2 position, Vector2 size, Texture2D texture, Color color)
    {
        Name = name;
        Position = position;
        Size = size;
        Color = color;
        Texture = texture;
        Hidden = false;

        // Size is going to be based on the scale of the current texture and the size of the screen
        Size = new Vector2(Size.X / Texture.Width, Size.Y / Texture.Height);

        InitialSize = Size;

        Resize();

        Layer = 0f;

        Controller.Instance.OnButtonPress += OnButtonPress;
        Controller.Instance.OnButtonDown += OnButtonDown;
        Controller.Instance.OnButtonRelease += OnButtonRelease;
    }

    protected float Opacity => ObjectManager.Instance.GetOpacity();

    public string Name { get; set; }

    public Vector2 Position { get; set; }

    public Vector2 Size { get; set; }

    public Vector2 InitialSize { get; set; }

    public float Rotation { get; set; }

    public float Layer { get; set; }

    public Color Color { get; set; }

    public Texture2D Texture { get; set; }

    public bool Hidden { get; set; }

    public List<Object> Children => _children;

    public Vector2 Origin { get; set; } = Vector2.Zero;

    public Vector2 Scale { get; set; } = Vector2.One;

    public bool WorldSpace { get; set; } = true;

    public Object Parent { get; private set; }

    public static bool ShowDebug => Window.Instance.ScreenRenderer().IsDebugEnabled();

    public virtual void Dispose()
    {
        Hidden = true;
        Controller.Instance.OnButtonPress -= OnButtonPress;
        Controller.Instance.OnButtonDown -= OnButtonDown;
        Controller.Instance.OnButtonRelease -= OnButtonRelease;
        if (_children != null)
            foreach (var child in _children)
            {
                child.Dispose();

                Controller.Instance.OnButtonPress -= child.OnButtonPress;
                Controller.Instance.OnButtonDown -= child.OnButtonDown;
                Controller.Instance.OnButtonRelease -= child.OnButtonRelease;
            }

        Controller.Instance.ResetState();
        _children = null;
    }

    public void SetParent(Object parent)
    {
        Parent = parent;
        parent.AddChild(this);
    }

    public void AddChild(Object child)
    {
        if (_children == null)
            _children = new List<Object>();

        _children.Add(child);
    }

    public void RemoveChild(Object child)
    {
        _children.Remove(child);
    }

    public virtual void Update(GameTime gameTime)
    {
        if (_children != null)
            foreach (var child in _children)
                child.Update(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!Hidden)
        {
            // Clamp rotation to 0-360
            var rotation = Rotation % 360;

            // Calculate position based on parent's position
            var position = Parent != null ? Parent.Position + Position * Parent.Size : Position;

            spriteBatch.Draw(Texture, _scalingManager.GetScaledPosition(position), null, Color * Opacity, rotation,
                _scalingManager.GetScaledPosition(Origin), _scalingManager.GetScaledPosition(Size), SpriteEffects.None,
                Layer);
        }

        if (_children != null)
            foreach (var child in _children)
                child.Draw(spriteBatch);

        if (ShowDebug)
        {
        }
    }

    public virtual void OnButtonDown(object sender, Controller.ControllerButton e)
    {
    }

    public virtual void OnButtonPress(object sender, Controller.ControllerButton e)
    {
    }

    public virtual void OnButtonRelease(object sender, Controller.ControllerButton e)
    {
    }

    public Vector2 GetAbsolutePosition()
    {
        if (Parent == null || WorldSpace) return Position;

        return Parent.GetAbsolutePosition() + Position * Parent.Size;
    }

    public float GetAbsoluteRotation()
    {
        if (Parent == null) return Rotation;

        return Parent.GetAbsoluteRotation() + Rotation;
    }

    public virtual void Resize()
    {
        if (_children != null)
            foreach (var child in _children)
                child.Resize();
    }
}