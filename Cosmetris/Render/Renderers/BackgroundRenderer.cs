/*
 * BackgroundRenderer.cs is part of Cosmetris.
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
using Cosmetris.Game.Objects.Cosmonoes;
using Cosmetris.Render.Managers;
using Cosmetris.Settings;
using Cosmetris.Util.Background;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Renderers;

public class BackgroundRenderer
{
    private static readonly GraphicsDevice _graphicsDevice = Window.Instance.GetGraphicsDevice();
    private static readonly EffectsManager _effectsManager = EffectsManager.Instance;
    private static readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;

    public static readonly BackgroundRenderer Instance = new();
    private readonly bool _cosmonoRainEnabled = true; // set this to enable/disable the Cosmono Rain
    private readonly Effect _effect;


    private readonly EffectsManager.FX _fx;

    private readonly Texture2D _noiseTexture;
    private RenderTarget2D _backgroundTarget;

    private List<CosmonoShape> _cosmonoShapes = new();
    private Random _rand = new();

    public BackgroundRenderer()
    {
        var width = _graphicsDevice.Viewport.Width;
        var height = _graphicsDevice.Viewport.Height;

        _backgroundTarget = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color,
            DepthFormat.None, _graphicsDevice.PresentationParameters.MultiSampleCount,
            RenderTargetUsage.PreserveContents);

        // Load our background effect
        _fx = new EffectsManager.FX("galaxy", _scalingManager.DesiredWidth, _scalingManager.DesiredHeight);
        TextureManager.Instance.AddTexture("simplex", "FX/simplex");
        TextureManager.Instance.AddTexture("cloud", "FX/cloud");
        _noiseTexture = TextureManager.Instance.GetTexture2D("simplex");
        _fx.Effect.Parameters["NoiseTexture"].SetValue(_noiseTexture);
        _fx.Effect.Parameters["CloudTexture"].SetValue(TextureManager.Instance.GetTexture2D("cloud"));
        _effectsManager.AddEffect(_fx);
        _effect = _effectsManager.GetEffect("galaxy");
        
        
        // Get shader colors
        var galColOne = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 1");
        var galColTwo = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 2");
        var galColThree = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 3");
        var nebulaColor = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Nebula Color");
        var cloudColor = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Cloud Color");
        
        // Set shader colors
        _fx.Effect.Parameters["GalaxyColorOne"].SetValue(galColOne.ToVector3());
        _fx.Effect.Parameters["GalaxyColorTwo"].SetValue(galColTwo.ToVector3());
        _fx.Effect.Parameters["GalaxyColorThree"].SetValue(galColThree.ToVector3());
        _fx.Effect.Parameters["NebulaColor"].SetValue(nebulaColor.ToVector3());
        _fx.Effect.Parameters["CloudColor"].SetValue(cloudColor.ToVector3());
    }

    public float DecayFactor { get; set; } = 1.0f;

    public void RenderToTexture(SpriteBatch spriteBatch, Texture2D curTexture)
    {
        _graphicsDevice.SetRenderTarget(_backgroundTarget);
        // Clear buffer
        _graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

        // Render our background
        spriteBatch.Begin( SpriteSortMode.Immediate, _graphicsDevice.BlendState, null, _graphicsDevice.DepthStencilState, RasterizerState.CullNone, _effect );
        spriteBatch.Draw(curTexture,
            new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height),
            Microsoft.Xna.Framework.Color.White);
        spriteBatch.End();

        spriteBatch.Begin();

        CosmonoRain.Instance.DrawRain(spriteBatch);

        spriteBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        UpdateGalaxyPulse(gameTime);

        if (_cosmonoRainEnabled) CosmonoRain.Instance.UpdateRain(gameTime);
    }

    public void DrawTarget(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTarget,
            new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height),
            Microsoft.Xna.Framework.Color.White);
    }

    public void OnResize()
    {
        var width = _graphicsDevice.Viewport.Width;
        var height = _graphicsDevice.Viewport.Height;

        _backgroundTarget = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color,
            DepthFormat.None, _graphicsDevice.PresentationParameters.MultiSampleCount,
            RenderTargetUsage.PreserveContents);
        //CosmonoRain.Instance.Reset();
    }

    public void UpdateGalaxyPulse(GameTime gameTime)
    {
        if (_fx == null)
            return;

        var linesCompleted = _fx.Effect.Parameters["LinesCompleted"].GetValueSingle();

        if (DecayFactor <= 0.0f)
        {
            if (DecayFactor < 0.0f)
            {
                _fx.Effect.Parameters["LinesCompleted"].SetValue(0.0f);
                DecayFactor = 0.0f;
            }

            return;
        }

        linesCompleted *= DecayFactor;
        DecayFactor -= 0.015f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _fx.Effect.Parameters["LinesCompleted"].SetValue(linesCompleted);
    }
}