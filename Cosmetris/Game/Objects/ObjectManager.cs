/*
 * ObjectManager.cs is part of Cosmetris.
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

namespace Cosmetris.Game.Objects;

public class ObjectManager
{
    private static readonly object _lock = new();
    private static ObjectManager _instance;
    private readonly Dictionary<string, Object> _objectLookup;
    private readonly List<Object> _objects;

    private bool _isClosing;
    private bool _needsSorting;
    private float _opacity = 1f;

    private ObjectManager()
    {
        _objects = new List<Object>();
        _objectLookup = new Dictionary<string, Object>(StringComparer.OrdinalIgnoreCase);
    }

    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null)
                lock (_lock)
                {
                    if (_instance == null) _instance = new ObjectManager();
                }

            return _instance;
        }
    }

    public void AddObject(Object obj)
    {
        _objects.Add(obj);
        _objectLookup[obj.Name.ToLower()] = obj;
        _needsSorting = true;
    }

    public void RemoveObject(Object obj)
    {
        _objects.Remove(obj);
        _objectLookup.Remove(obj.Name.ToLower());
        obj.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in _objects) obj.Update(gameTime);

        HandleScreenTransition();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_needsSorting)
        {
            _objects.Sort((a, b) => a.Layer.CompareTo(b.Layer));
            _needsSorting = false;
        }

        foreach (var obj in _objects)
        {
            if (_isClosing)
                obj.Color = new Color(obj.Color, _opacity);

            obj.Draw(spriteBatch);
        }
    }

    public float GetOpacity()
    {
        return _opacity;
    }

    private void HandleScreenTransition()
    {
        if (Window.Instance.ScreenRenderer().GetNextScreen() != null && !_isClosing)
            _isClosing = true;
        else if (Window.Instance.ScreenRenderer().GetNextScreen() == null && _isClosing)
            _isClosing = false;

        if (_opacity < 1f && !_isClosing)
            _opacity += 0.008f * (float)Window.Instance.GetGameTime().ElapsedGameTime.TotalMilliseconds;
        else if (_opacity > 1f && !_isClosing)
            _opacity = 1f;
        else if (_isClosing && _opacity > 0f)
            _opacity -= 0.008f * (float)Window.Instance.GetGameTime().ElapsedGameTime.TotalMilliseconds;
        else if (_isClosing && _opacity <= 0f) _opacity = 0f;
    }

    public void Resize()
    {
        foreach (var obj in _objects) obj.Resize();
    }

    public void Dispose()
    {
        foreach (var obj in _objects) obj.Dispose();

        _objects.Clear();
        _objectLookup.Clear();
    }

    public List<Object> GetObjects()
    {
        return _objects;
    }

    public Object GetObject(string objName)
    {
        if (_objectLookup.TryGetValue(objName.ToLower(), out var foundObject)) return foundObject;

        throw new Exception("Object not found");
    }
}