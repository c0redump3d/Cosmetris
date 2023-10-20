/*
 * NextCosmonoes.cs is part of Cosmetris.
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
using System.Linq;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects.Cosmonoes.Util;

/// <summary>
///     This class will create, handle, and draw the next cosmonoes for the current player.
/// </summary>
public class NextCosmonoes : IDisposable
{
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);
    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;

    private readonly TetrisGameManager _tetrisGameManager;
    private readonly BagRandomizer randomizer;
    private Texture2D _cachedTexture; // This will hold the cached rendering of the shapes.
    private bool _isClosing; // Used to correctly fade in & out the next cosmonoes.

    private bool _isDirty = true; // Will be used to determine whether or not to update the texture

    private float _maxWidth; // max width of the shapes

    private SpriteBatch _nextBatch; // This will be used to render the shapes to the texture.

    // Position of where the next shapes are rendered.
    private Vector2 _nextCosmonoPos;
    private RenderTarget2D _renderTarget; // This will be used to render the shapes to the texture.
    private float _totalHeight; // total height of the shapes
    private float opacity;

    public NextCosmonoes(TetrisGameManager tetrisGameManager)
    {
        _tetrisGameManager = tetrisGameManager;
        randomizer = new BagRandomizer(tetrisGameManager.CurrentCosmono());

        _nextBatch = new SpriteBatch(Window.Instance.GetGraphicsDevice());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void OnResize()
    {
        // Dispose of the old render target, and mark the texture as dirty.
        
        var grid = _tetrisGameManager.GetGrid();
        _renderTarget = new RenderTarget2D(Window.Instance.GetGraphicsDevice(), _scalingManager.ActualWidth,
            _scalingManager.ActualHeight);
        _nextCosmonoPos = new Vector2(_scalingManager.GetScaledX(grid.GetActualPosition().X + grid.GetFinalSize().X) + 35f, _scalingManager.GetScaledY(grid.GetActualPosition().Y + 35f));
        _maxWidth = 0;
        _totalHeight = 0;
        _nextBatch = new SpriteBatch(Window.Instance.GetGraphicsDevice());
        _isDirty = true;
    }

    public void MarkDirty()
    {
        _isDirty = true;
    }

    public CosmonoShape GetNextCosmonoShape()
    {
        MarkDirty();
        return randomizer.GetNextShape();
    }

    public void UpdateNextCosmonoesRender()
    {
        if (_nextBatch == null)
            return;

        // Handle the fade in/out of the next cosmonoes.
        HandleScreenTransition();

        // update the texture if marked dirty
        if (_isDirty)
        {
            var nextCosmonoPos = _nextCosmonoPos;
            UpdateCache(_nextBatch,
                nextCosmonoPos); // This function will update the _cachedTexture based on the current shapes.
            _isDirty = false; // Reset the flag.
        }
    }

    public Texture2D GetNextTexture()
    {
        return _cachedTexture;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (GetNextTexture() != null && opacity > 0f)
        {
            RenderUtil.DrawBorderRect(_nextCosmonoPos.X - _scalingManager.GetScaledX(10),
                _nextCosmonoPos.Y - _scalingManager.GetScaledY(20), _maxWidth + _scalingManager.GetScaledX(20),
                _totalHeight + _scalingManager.GetScaledY(20), Color.Black * 0.35f * opacity,
                Color.Black * 0.5f * opacity);
            _defaultFont.DrawLabel("Next", _nextCosmonoPos.X + _maxWidth / 2f,
                _nextCosmonoPos.Y - _scalingManager.GetScaledY(10), Color.White * opacity,
                TextHorizontalAlignment.Center, scaled: false);
            spriteBatch.Draw(GetNextTexture(), Vector2.Zero, Color.White * opacity);
        }
    }

    /// <summary>
    ///     Creates or updates the cached texture of all upcoming shapes.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="nextCosmonoPos"></param>
    public void UpdateCache(SpriteBatch spriteBatch, Vector2 nextCosmonoPos)
    {
        var defaultSize = new Vector2(24, 24);
        defaultSize = _scalingManager.GetScaledPosition(defaultSize);
        _maxWidth = 0;
        _totalHeight = 0;
        var scaleForNext = 1.85f;
        var regularScale = 1.0f;
        var marginBetweenCosmonoes = 5;

        var upcomingShapes = new List<CosmonoShape> { randomizer.PeekNextShape() };

        if (randomizer.CurrentBagCount() > 0)
            upcomingShapes.AddRange(randomizer.PeekRemainingShapes());
        else
            upcomingShapes.AddRange(randomizer.PeekNextBag().Skip(1));

        upcomingShapes.AddRange(randomizer.PeekNextBag());

        var shapesToShow = Math.Min(upcomingShapes.Count, 7);

        // First, compute the maximum width
        for (var i = 0; i < shapesToShow; i++)
        {
            var scale = i == 0 ? scaleForNext : regularScale;
            var shapeWidth = 4 * defaultSize.X * scale; // Assuming cosmonoes are 4 blocks wide
            _maxWidth = Math.Max(_maxWidth, shapeWidth);
        }

        spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.GraphicsDevice.Clear(Color.Transparent); // Clear with transparent color

        var referenceCenterX = _maxWidth / 2;

        spriteBatch.Begin();

        for (var i = 0; i < shapesToShow; i++)
        {
            var scale = i == 0 ? scaleForNext : regularScale;

            // Calculate bounding box for the shape
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            foreach (var offset in upcomingShapes[i].Offsets)
            {
                minX = Math.Min(minX, offset.X);
                maxX = Math.Max(maxX, offset.X);
                minY = Math.Min(minY, offset.Y);
                maxY = Math.Max(maxY, offset.Y);
            }

            var shapeWidth =
                (maxX - minX + 1) * defaultSize.X * scale; // +1 because both min and max offsets are inclusive
            var shapeHeight =
                (maxY - minY + 1) * defaultSize.Y * scale; // +1 because both min and max offsets are inclusive

            // Center the bounding box
            var shapeStartX = referenceCenterX - shapeWidth / 2;

            // Calculate the y-offset for centering
            var shapeStartY =
                (4 * defaultSize.Y * scale - shapeHeight) / 2; // Assuming cosmonoes are 4 blocks tall 

            var centeringOffset = new Vector2(shapeStartX - minX * defaultSize.X * scale,
                shapeStartY - minY * defaultSize.Y * scale);

            foreach (var offset in upcomingShapes[i].Offsets)
            {
                var position = nextCosmonoPos + centeringOffset + offset * defaultSize * scale;
                spriteBatch.Draw(upcomingShapes[i].Texture,
                    new Rectangle((int)position.X, (int)position.Y, (int)(defaultSize.X * scale),
                        (int)(defaultSize.Y * scale)), Color.White);
            }

            shapeHeight = 4 * defaultSize.Y * scale;
            _totalHeight += shapeHeight + marginBetweenCosmonoes;
            nextCosmonoPos.Y += 4 * defaultSize.Y * scale + marginBetweenCosmonoes;
        }

        _totalHeight -= marginBetweenCosmonoes;
        spriteBatch.End();

        _cachedTexture = _renderTarget;

        spriteBatch.GraphicsDevice.SetRenderTarget(null);
    }

    private void HandleScreenTransition()
    {
        if (Window.Instance.ScreenRenderer().GetNextScreen() != null && !_isClosing)
            _isClosing = true;

        if (opacity < 1f && !_isClosing)
            opacity += 0.008f * (float)_tetrisGameManager.GameTime.ElapsedGameTime.TotalMilliseconds;
        else if (opacity > 1f && !_isClosing)
            opacity = 1f;
        else if (_isClosing && opacity > 0f)
            opacity -= 0.008f * (float)_tetrisGameManager.GameTime.ElapsedGameTime.TotalMilliseconds;
        else if (_isClosing && opacity <= 0f) opacity = 0f;
    }

    ~NextCosmonoes()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cachedTexture?.Dispose();
            _nextBatch?.Dispose();
            _renderTarget?.Dispose();
        }
    }
}