/*
 * HeldCosmono.cs is part of Cosmetris.
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
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects.Cosmonoes.Util;

/// <summary>
///     Renders and handles the held cosmono for a player.
/// </summary>
public class HeldCosmono : IDisposable
{
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);
    private readonly Texture2D _lockedTexture; // Texture to display when the cosmono is locked

    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;
    private readonly TetrisGameManager _tetrisGameManager;

    private Texture2D _cachedTexture; // Cache the texture to avoid re-rendering every frame
    private Vector2 _centeringOffset; // Offset to center the cosmono in the render target
    private Vector2 _defaultSize; // Default size of the cosmono
    private SpriteBatch _heldBatch; // Batch to draw the cosmono to the render target
    private Vector2 _heldCosmonoPos; // Position of the render target
    private RenderTarget2D _heldRenderTarget; // Render target to draw the cosmono to

    private CosmonoShape _heldShape;
    private bool _isClosing; // Whether or not the cosmono is closing
    private bool _isDirty = true; // Whether or not the render target needs to be updated
    private bool _isLocked; // Whether or not the cosmono is locked

    private float _opacity; // Opacity of the render target

    public HeldCosmono(TetrisGameManager gameManager)
    {
        _tetrisGameManager = gameManager;

        // Locked texture of the held cosmono is always the x-block, or the "gray" block
        _lockedTexture = null;
        _lockedTexture = TextureManager.Instance.GetTexture("x_block").Texture2D;
    }

    private float RectWidth => _scalingManager.GetScaledX(200f);
    private float RectHeight => _scalingManager.GetScaledY(200f);

    public void Dispose()
    {
        _cachedTexture?.Dispose();
        _heldBatch?.Dispose();
        _heldRenderTarget?.Dispose();
    }

    /// <summary>
    ///     Swaps the held cosmono with the current cosmono
    /// </summary>
    /// <param name="currentShape"> The current cosmono shape </param>
    /// <returns> The previous held cosmono shape </returns>
    public CosmonoShape SwapHeldShape(CosmonoShape currentShape)
    {
        // Don't swap if the cosmono is locked
        if (_isLocked)
            return null;

        // Lock the cosmono
        _isLocked = true;

        // Update the render target
        MarkDirty();

        // Swap the cosmonoes
        var previousHeld = _heldShape;
        _heldShape = currentShape;

        Window.Instance.GetSoundManager().PlaySFX("hold");

        return previousHeld;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_opacity <= 0f) return;

        var rectStartPos = new Vector2(_heldCosmonoPos.X, _heldCosmonoPos.Y);
        RenderUtil.DrawBorderRect(rectStartPos.X, rectStartPos.Y, RectWidth, RectHeight,
            Color.Black * 0.35f * _opacity, Color.Black * 0.5f * _opacity);
        _defaultFont.DrawLabel("Held", rectStartPos.X + RectWidth / 2f,
            rectStartPos.Y + _scalingManager.GetScaledY(10), Color.White * _opacity, TextHorizontalAlignment.Center,
            scaled: false);
        if (GetHeldTexture() != null) spriteBatch.Draw(GetHeldTexture(), Vector2.Zero, Color.White * _opacity);
    }

    public void OnResize()
    {
        // Dispose of the old render target and create a new one
        _heldBatch?.Dispose();
        _heldRenderTarget = new RenderTarget2D(Window.Instance.GetGraphicsDevice(), _scalingManager.ActualWidth,
            _scalingManager.ActualHeight);
        
        var grid = _tetrisGameManager.GetGrid();
        _heldCosmonoPos = _scalingManager.GetScaledPosition(grid.GetActualPosition())+ new Vector2(-RectWidth - 27.5f, 15f);
        _heldBatch = new SpriteBatch(Window.Instance.GetGraphicsDevice());
        _isDirty = true;
    }

    public Texture2D GetHeldTexture()
    {
        return _cachedTexture;
    }

    public void MarkDirty()
    {
        _isDirty = true;
    }

    public bool IsLocked()
    {
        return _isLocked;
    }

    public void Unlock()
    {
        _isLocked = false;
        MarkDirty();
    }

    public void Lock()
    {
        _isLocked = true;
        MarkDirty();
    }

    public void UpdateHeldCosmonoRender()
    {
        if (_heldBatch == null)
            return;

        if (Window.Instance.ScreenRenderer().GetNextScreen() != null && !_isClosing)
            _isClosing = true;

        if (_opacity < 1f && !_isClosing)
            _opacity += 0.008f * (float)_tetrisGameManager.GameTime.ElapsedGameTime.TotalMilliseconds;
        else if (_opacity > 1f && !_isClosing)
            _opacity = 1f;
        else if (_isClosing && _opacity > 0f)
            _opacity -= 0.008f * (float)_tetrisGameManager.GameTime.ElapsedGameTime.TotalMilliseconds;
        else if (_isClosing && _opacity <= 0f) _opacity = 0f;

        if (_isDirty)
        {
            UpdateCache(_heldBatch, _heldCosmonoPos);
            _isDirty = false;
        }
    }

    private void UpdateCache(SpriteBatch spriteBatch, Vector2 heldCosmonoPos)
    {
        if (_heldShape == null) return;

        _defaultSize = _scalingManager.GetScaledPosition(new Vector2(24, 24));
        var scaleForHeld = 1.85f;

        // Calculate bounding box for the shape
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;

        foreach (var offset in _heldShape.Offsets)
        {
            minX = Math.Min(minX, offset.X);
            maxX = Math.Max(maxX, offset.X);
            minY = Math.Min(minY, offset.Y);
            maxY = Math.Max(maxY, offset.Y);
        }

        var shapeWidth = (maxX - minX + 1) * _defaultSize.X * scaleForHeld;
        var shapeHeight = (maxY - minY + 1) * _defaultSize.Y * scaleForHeld;

        // Centering logic
        var shapeStartX = RectWidth / 2 - shapeWidth / 2;
        var shapeStartY = RectHeight / 2 - shapeHeight / 2;

        _centeringOffset = new Vector2(shapeStartX - minX * _defaultSize.X * scaleForHeld,
            shapeStartY - minY * _defaultSize.Y * scaleForHeld);

        spriteBatch.GraphicsDevice.SetRenderTarget(_heldRenderTarget);
        spriteBatch.GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin();

        foreach (var offset in _heldShape.Offsets)
        {
            var position = heldCosmonoPos + _centeringOffset + offset * _defaultSize * scaleForHeld;
            spriteBatch.Draw(_isLocked ? _lockedTexture : _heldShape.Texture,
                new Rectangle((int)position.X, (int)position.Y, (int)(_defaultSize.X * scaleForHeld),
                    (int)(_defaultSize.Y * scaleForHeld)), Color.White);
        }

        spriteBatch.End();
        _cachedTexture = _heldRenderTarget;
        spriteBatch.GraphicsDevice.SetRenderTarget(null);
    }
}