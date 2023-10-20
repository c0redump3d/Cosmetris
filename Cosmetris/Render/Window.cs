/*
 * Window.cs is part of Cosmetris.
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
using Cosmetris.Game.Objects;
using Cosmetris.Game.Packs.TexturePacks;
using Cosmetris.Input;
using Cosmetris.Render.Managers;
using Cosmetris.Render.Particle;
using Cosmetris.Render.Renderers;
using Cosmetris.Render.UI.Screens;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Cosmetris.Sound;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cosmetris.Game;
using Cosmetris.Render.UI;
using Cosmetris.Util.Events;

namespace Cosmetris.Render;

public class Window
{
    public const string GameName = "Cosmetris";
    public const string GameVersion = "0.5.1a";

    // Default to LinearClamp -- Seems to look the best in general.
    private readonly SamplerState _defaultSampler = SamplerState.LinearClamp;
    private readonly GraphicsDeviceManager _graphics;
    private readonly Pointer _pointer;
    private readonly List<Renderer> _renderers = new();

    public readonly TexturePackManager PackManager;
    public readonly UIScalingManager ScalingManager;

    private FontRenderer _fontRenderer;

    private GameTime _gameTime;
    private EffectsManager.FX _glowEffect;
    private bool _isInitialized;
    private Renderer _primaryRenderer;

    private RenderTarget2D _renderTarget;
    private SoundManager _soundManager;
    private SpriteBatch _targetBatch;

    public EventHandler<GameTime> UpdateEvent;
    public EventHandler<SpriteBatch> DrawEvent;
    
    DepthStencilState depthStencilState = new DepthStencilState
    {
        DepthBufferEnable = true,
        DepthBufferWriteEnable = true,
        DepthBufferFunction = CompareFunction.LessEqual
    };

    public Window(GraphicsDeviceManager graphics)
    {
        Instance ??= this;

        _graphics = graphics;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        
        _graphics.PreparingDeviceSettings += (object s, PreparingDeviceSettingsEventArgs args) =>
        {
            args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        };
        
        _graphics.ApplyChanges();

        // Default to 1080p
        ScalingManager = new UIScalingManager(1920, 1080, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);

        _pointer = new Pointer();

        var fullscreen = GameSettings.Instance.GetValue<bool>("Graphics", "Fullscreen");
        var vsync = GameSettings.Instance.GetValue<bool>("Graphics", "VSync");

        // Set fullscreen
        SetFullscreen(fullscreen);
        
        // Set vsync
        _graphics.SynchronizeWithVerticalRetrace = vsync;

        // Apply saved controller settings
        Controller.Instance.LoadSettings();

        PackManager = new TexturePackManager(_graphics.GraphicsDevice);
    }

    public static Window Instance { get; private set; }

    public ScreenRenderer ScreenRenderer()
    {
        return _renderers.Find(x => x is ScreenRenderer) as ScreenRenderer;
    }

    public Pointer GetPointer()
    {
        return _pointer;
    }

    public void ContentLoad()
    {
        _targetBatch = new SpriteBatch(_graphics.GraphicsDevice);

        _renderTarget = new RenderTarget2D(_graphics.GraphicsDevice, ScalingManager.ActualWidth,
            ScalingManager.ActualHeight, false,
            _graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None,
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);

        // Load pointer content
        _pointer.ContentLoad();

        // Load all block textures
        PackManager.LoadDefaultBlocks();

        // Create font renderer
        _fontRenderer = new FontRenderer(_targetBatch);

        _renderers.Add(new ScreenRenderer());
        _primaryRenderer = _renderers[0];
        _renderers.Add(new GameRenderer());

        // Initialize all renderers
        foreach (var renderer in _renderers) renderer.Initialize(_graphics.GraphicsDevice);

        RenderUtil.Initialize(_targetBatch);

        // Load sounds & music
        _soundManager = new SoundManager();

        Cosmetris.Instance.Window.Title = $"{GameName} (v{GameVersion})";

        // Attempt to load saved texture pack if it exists
        PackManager.LoadSaved();

        // Add glow effect to manager
        var fx = new EffectsManager.FX("glow", ScalingManager.DesiredWidth, ScalingManager.DesiredHeight);
        EffectsManager.Instance.AddEffect(fx);
        _glowEffect = fx;
        // Set glow effect parameters
        _glowEffect.Effect.Parameters["GlowColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.35f));

        // Finally, set the screen to the main menu
        ScreenRenderer().SetScreen(new MainMenuScreen());
    }

    public void Draw(GameTime gameTime)
    {


        // Render background first
        BackgroundRenderer.Instance.RenderToTexture(_targetBatch, _renderTarget);
        
        // Set render target to the render target
        _targetBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        // Clear buffer
        _targetBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
        _targetBatch.GraphicsDevice.DepthStencilState = depthStencilState;

        _primaryRenderer.Draw(gameTime);
        // Draw all renderers except primary
        foreach (var renderer in _renderers)
            if (renderer != _primaryRenderer)
                renderer.Draw(gameTime);
        _primaryRenderer.DrawImportant(gameTime);
        

        // Draw particles
        _targetBatch.Begin(samplerState: _defaultSampler, sortMode: SpriteSortMode.BackToFront, rasterizerState: RasterizerState.CullNone);
        
        ParticleManager.Instance.Draw(_targetBatch, gameTime);
        _targetBatch.End();
        
        DrawEvent?.Invoke(this, _targetBatch);

        // Set render target to null
        _targetBatch.GraphicsDevice.SetRenderTarget(null);
        _targetBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

        _targetBatch.Begin(samplerState: _defaultSampler, sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, rasterizerState: RasterizerState.CullNone, depthStencilState: DepthStencilState.DepthRead);

        // Draw background
        BackgroundRenderer.Instance.DrawTarget(_targetBatch, gameTime);
        
        _targetBatch.Draw(_renderTarget, new Vector2(0, 0),
            new Rectangle(0, 0, _renderTarget.Width, _renderTarget.Height),
            Color.White);

        _targetBatch.End();

        // Draw pointer
        _targetBatch.Begin(SpriteSortMode.BackToFront, samplerState: _defaultSampler);
        _pointer.Draw(_targetBatch, gameTime);
        _targetBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        _gameTime = gameTime;

        // Make sure the window size is correct
        UpdateWindowSize();

        // Update input ASAP
        Controller.Instance.Update(gameTime);

        // Update pointer
        _pointer.Update(gameTime);

        // Update all particles
        ParticleManager.Instance.Update(gameTime);

        // Update all effects
        EffectsManager.Instance.Update(gameTime);

        _primaryRenderer.Update(gameTime);
        // Update all renderers except primary
        foreach (var renderer in _renderers)
            if (renderer != _primaryRenderer)
                renderer.Update(gameTime);
        
        UpdateEvent?.Invoke(this, gameTime);

        // Update timers
        Timer.Instance.UpdateTimers(gameTime);
    }

    public GameTime GetGameTime()
    {
        return _gameTime;
    }

    private void UpdateWindowSize()
    {
        // Check to see if the window size has changed since last update.
        if (_graphics.PreferredBackBufferWidth != GetGraphicsDevice().Viewport.Width ||
            _graphics.PreferredBackBufferHeight != GetGraphicsDevice().Viewport.Height)
        {
            _graphics.PreferredBackBufferWidth = GetGraphicsDevice().Viewport.Width;
            _graphics.PreferredBackBufferHeight = GetGraphicsDevice().Viewport.Height;

            _graphics.ApplyChanges();

            _renderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, false,
                _graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None,
                _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            
            _targetBatch = new SpriteBatch(_graphics.GraphicsDevice);

            // Resize ScalingManager
            ScalingManager.ResizeScale(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            // FontRenderer - Reload fonts
            _fontRenderer.ReloadFonts(_targetBatch);
            
            RenderUtil.OnResize(_targetBatch);

            // EffectsManager - Resize
            EffectsManager.Instance.OnResize();

            // Background - Resize
            BackgroundRenderer.Instance.OnResize();

            // Resize for renderers
            foreach (var renderer in _renderers) renderer.OnResize();

            // ObjectManager - Resize
            ObjectManager.Instance.Resize();

            // fix for background
            if (!_isInitialized)
            {
                // Don't know why, but this fixes the background
                _graphics.GraphicsDevice.PresentationParameters.BackBufferWidth = ScalingManager.DesiredWidth - 1;
                // apply changes
                _graphics.GraphicsDevice.Reset();
                _isInitialized = true;
            }
        }
    }

    public void SetFullscreen(bool fullscreen)
    {
        var wasFullscreen = _graphics.IsFullScreen;

        _graphics.IsFullScreen = fullscreen;

        _graphics.ApplyChanges();

        if (!fullscreen && wasFullscreen && _graphics.GraphicsDevice != null)
        {
            _graphics.GraphicsDevice.PresentationParameters.BackBufferWidth = ScalingManager.DesiredWidth;
            _graphics.GraphicsDevice.PresentationParameters.BackBufferHeight = ScalingManager.DesiredHeight;
            _graphics.GraphicsDevice.Reset();
        }

        _graphics.ApplyChanges();
    }
    
    public void SetVSync(bool vsync)
    {
        if(_graphics.SynchronizeWithVerticalRetrace == vsync)
            return;
        
        _graphics.SynchronizeWithVerticalRetrace = vsync;
        _graphics.ApplyChanges();
    }

    public GraphicsDevice GetGraphicsDevice()
    {
        return _graphics.GraphicsDevice;
    }

    public SoundManager GetSoundManager()
    {
        return _soundManager;
    }
}