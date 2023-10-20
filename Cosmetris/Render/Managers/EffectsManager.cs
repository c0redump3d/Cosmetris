/*
 * EffectsManager.cs is part of Cosmetris.
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
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Managers;

public class EffectsManager
{
    private static readonly GraphicsDevice _graphicsDevice = Window.Instance.GetGraphicsDevice();
    private static readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    public static readonly EffectsManager Instance = new();

    private readonly List<FX> _effects = new();

    public void AddEffect(FX effect)
    {
        _effects.Add(effect);
    }

    public void Update(GameTime gameTime)
    {
        for (var i = 0; i < _effects.Count; i++)
        {
            _effects[i].Update(gameTime);
            if (_effects[i].IsFinished())
            {
                _effects.RemoveAt(i);
                i--;
            }
        }
    }

    public Effect GetEffect(string name)
    {
        return _effects.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()))?.Effect;
    }

    public FX GetFX(string name)
    {
        return _effects.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));
    }

    public void OnResize()
    {
        // Update all effects
        foreach (var effect in _effects)
        {
            effect.SetResolution(_scalingManager.ActualWidth, _scalingManager.ActualHeight);
            effect.ApplyEffect();
        }
    }

    public class FX
    {
        private static readonly GraphicsDevice _graphicsDevice = Window.Instance.GetGraphicsDevice();

        private readonly Dictionary<string, string> _parameters = new();

        public FX(string path, float w, float h, float duration = -1)
        {
            Name = path;
            Duration = duration;
            Width = w;
            Height = h;

            var loadedEffect = ShaderUtil.LoadEffect(_graphicsDevice, path);
            Effect = loadedEffect;

            // Add parameters
            foreach (var parameter in loadedEffect.Parameters)
                _parameters.Add(parameter.Name.ToLower(), parameter.Name);
        }

        public string Name { get; }
        public Effect Effect { get; }
        public float Time { get; private set; }
        public float Duration { get; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public void Update(GameTime gameTime)
        {
            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check if effect has any kind of Time parameter
            if (_parameters.Keys.Contains("time")) Effect.Parameters["Time"].SetValue(Time);
        }

        public void ApplyEffect()
        {
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public bool IsFinished()
        {
            if ((int)Duration == -1)
                return false;

            return Time >= Duration;
        }

        public void SetResolution(int viewportWidth, int viewportHeight)
        {
            Width = viewportWidth;
            Height = viewportHeight;

            // Check if effect has any kind of Resolution parameter
            string[] resolutionParameters = { "resolution" };

            for (var i = 0; i < resolutionParameters.Length; i++)
                if (_parameters.Keys.Contains(resolutionParameters[i]))
                {
                    var value = _parameters[resolutionParameters[i]];
                    Effect.Parameters[value].SetValue(new Vector2(Width, Height));
                }
        }
    }
}