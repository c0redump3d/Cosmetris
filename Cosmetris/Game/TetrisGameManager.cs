/*
 * TetrisGameManager.cs is part of Cosmetris.
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
using Cosmetris.Game.GameModes;
using Cosmetris.Game.Grid;
using Cosmetris.Game.Objects;
using Cosmetris.Game.Objects.Cosmonoes;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.Particle.Particles;
using Cosmetris.Render.UI.Screens;
using Cosmetris.Util;
using Cosmetris.Util.Background;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game;

public class TetrisGameManager
{
    private const int MaxMoveResets = 15; // The maximum number of times the player can move the piece while grounded
    private readonly Cosmono _currentCosmono;
    private readonly EffectsManager.FX _dissolveEffect;
    private readonly TetrisGrid _grid;
    private readonly float _opacityChange = 0.00045f; // This is the rate at which opacity changes
    private float _curGhostOpacity = 0.35f;
    private int _destroyCount;
    private bool _destroyingGrid; // This flag tells us if the grid is being destroyed (Post-dissolve)
    private float _destroyWaitTimer;
    private float _dissolveThreshold = 1.0f;
    private bool _dissolvingGrid; // This flag tells us if the grid is being dissolved

    private readonly bool _ghostPieceEnabled;
    private float _gravity; // The current gravity value
    private readonly float _gravityMultiplier = 1.0f; // The current gravity multiplier
    private float _gravityTimer; // The timer for the gravity
    private readonly bool _holdEnabled;
    private bool _isFadingOut = true; // This flag tells us if we're fading out or in
    private bool _justCrumbled; // This flag tells us if the grid just crumbled
    private readonly float _lockDelay = 500.0f; // The current lock delay
    private float _lockDelayTimer; // The timer for the lock delay
    public bool InCountdown = true;
    public EventHandler CountdownComplete;

    private int
        _moveResetCounter; // Active count of how many times the player has tried to move the piece while grounded

    public GameMode ActiveGameMode;

    public TetrisGameManager(GameMode gameMode, TetrisGrid grid, float gravityMultiplier, float lockDelay,
        bool holdEnabled, bool ghostPieceEnabled)
    {
        _gravityMultiplier = gravityMultiplier;
        _lockDelay = lockDelay;
        _holdEnabled = holdEnabled;
        _ghostPieceEnabled = ghostPieceEnabled;

        _grid = grid;
        ActiveGameMode = gameMode;
        _dissolveEffect = EffectsManager.Instance.GetFX("disolve");

        var defTex = TextureManager.Instance.GetTexture2D("i_block");
        _currentCosmono = new Cosmono("player", this, new Vector2(4, -1),
            new Vector2(grid.GetCellSize(), grid.GetCellSize()), defTex, Color.White);
        ObjectManager.Instance.AddObject(_currentCosmono);

        // Freeze & hide player
        _currentCosmono.Hidden = true;
        FreezeCosmono(true);

        UpdatePosition(_currentCosmono);

        // Create the ghost piece
        var ghostPiece = _currentCosmono.CreateGhostPiece();

        Window.Instance.GetSoundManager().StopMusic();
        Rubble.LoadTextures(TextureManager.Instance.CurrentTexturePack);

        // hide ghost piece
        ghostPiece.Hidden = true;

        GhostPiece = ghostPiece;

        if (ActiveGameMode != null) ActiveGameMode.GameModeInit(this, _currentCosmono);

        CountdownComplete += (sender, args) =>
        {
            UpdateGravity();
            _currentCosmono.HeldCosmono.OnResize();
            _currentCosmono.NextCosmonoes.OnResize();

            // Unfreeze & show player
            _currentCosmono.Hidden = false;
            FreezeCosmono(false);

            ghostPiece.Hidden = !_ghostPieceEnabled;

            UpdateGhostPiece();

            Window.Instance.GetSoundManager().PlayMusicSpecial("MusicMain-Start", "MusicMain-Loop");
        };
        
        Timer.Instance.CreateTimer(350f, (sender, args) =>
        {
            Countdown();
        });
    }

    public Cosmono GhostPiece { get; set; }
    public GameTime GameTime => Window.Instance.GetGameTime();

    public Cosmono CurrentCosmono()
    {
        return _currentCosmono;
    }

    public void Countdown()
    {
        Cosmetris.UpdateGameState(GameState.Paused);
        InCountdown = true;
        _grid.CreateMessageLabel("Ready?", Microsoft.Xna.Framework.Color.White, 1000f);
        Timer.Instance.CreateTimer(1000f, (sender, args) =>
        {
            _grid.CreateMessageLabel("3", Microsoft.Xna.Framework.Color.White, 1000f);
            Window.Instance.GetSoundManager().PlaySFX("count");
            
            Timer.Instance.CreateTimer(1000f, (sender, args) =>
            {
                _grid.CreateMessageLabel("2", Microsoft.Xna.Framework.Color.White, 1000f);
                Window.Instance.GetSoundManager().PlaySFX("count");

                Timer.Instance.CreateTimer(1000f, (sender, args) =>
                {
                    _grid.CreateMessageLabel("1", Microsoft.Xna.Framework.Color.White, 1000f);
                    Window.Instance.GetSoundManager().PlaySFX("count");

                    Timer.Instance.CreateTimer(1000f, (sender, args) =>
                    {
                        InCountdown = false;
                        Cosmetris.UpdateGameState(GameState.InGame);
                        _grid.CreateMessageLabel("GO!", Microsoft.Xna.Framework.Color.White, 1000f);
                        Window.Instance.GetSoundManager().PlaySFX("go");
                        CountdownComplete?.Invoke(this, EventArgs.Empty);
                    });
                });
            });
        });
    }

    public bool MoveLeft()
    {
        // Check if left movement is valid
        // Update _currentCosmono's position if valid
        if (_grid.WouldBeValid(_currentCosmono, _currentCosmono.GridPosition - Vector2.UnitX))
        {
            _currentCosmono.GridPosition -= Vector2.UnitX;
            UpdateGhostPiece();
            return true;
        }

        UpdateGhostPiece();
        return false;
    }

    public bool MoveRight()
    {
        // Check if right movement is valid
        // Update _currentCosmono's position if valid
        if (_grid.WouldBeValid(_currentCosmono, _currentCosmono.GridPosition + Vector2.UnitX))
        {
            _currentCosmono.GridPosition += Vector2.UnitX;
            UpdateGhostPiece();
            return true;
        }

        UpdateGhostPiece();
        return false;
    }

    public bool MoveDown(bool hardDrop = false)
    {
        // Check if downward movement is valid
        // Update _currentCosmono's position if valid
        if (_grid.WouldBeValid(_currentCosmono, _currentCosmono.GridPosition + Vector2.UnitY))
        {
            _currentCosmono.GridPosition += Vector2.UnitY;
            UpdateGhostPiece();

            if (!hardDrop)
                _currentCosmono.GetScore().HandleSoftDrop();
            else
                _currentCosmono.GetScore().HandleHardDrop();

            _currentCosmono.SetWasLastMoveRotation(false);
            return true;
        }

        UpdateGhostPiece();
        return false;
    }

    public bool RotateLeft()
    {
        var succeed = _currentCosmono.RotateCosmono(true);

        UpdateGhostPiece();

        return succeed;
    }

    public bool RotateRight()
    {
        var succeed = _currentCosmono.RotateCosmono(false);

        UpdateGhostPiece();

        return succeed;
    }

    public bool CheckCollision(Cosmono cosmono, bool fixY = true)
    {
        if (cosmono.IsGhost())
            return false;

        var position = cosmono.GridPosition;
        foreach (var part in cosmono.CosmonoParts)
        {
            // Calculate the absolute grid position of the part
            var partGridPosition = position + part.Position;

            // If the position is not valid, return true (indicating a collision)
            if (!_grid.IsPartValid(partGridPosition, fixY)) return true;
        }

        // If all parts are valid, return false (no collision)
        return false;
    }

    public void UpdateGhostPiece()
    {
        if (GhostPiece == null || GhostPiece.Hidden)
            return;

        UpdatePosition(_currentCosmono);
        
        GhostPiece.Texture = _currentCosmono.Texture;

        // Reset the ghost piece's position
        GhostPiece.GridPosition = _currentCosmono.GridPosition;
        UpdatePosition(GhostPiece);

        for (var i = 0; i < GhostPiece.CosmonoParts.Count; i++){
            GhostPiece.CosmonoParts[i].Position = _currentCosmono.CosmonoParts[i].Position;
            GhostPiece.CosmonoParts[i].Texture = _currentCosmono.CosmonoParts[i].Texture;
        }

        // Move the ghost piece down until it collides
        for (var i = 0; i < 22; i++)
            if (_grid.WouldBeValid(_currentCosmono, GhostPiece.GridPosition + new Vector2(0, 1)))
            {
                GhostPiece.GridPosition += new Vector2(0, 1);
                UpdatePosition(GhostPiece);
            }
            else
            {
                break;
            }
    }

    public bool HardDrop()
    {
        // Drop the _currentCosmono until it collides
        // Lock the _currentCosmono in place
        for (var i = 0; i < 22; i++)
            if (MoveDown(true))
                UpdatePosition(_currentCosmono);
            else
                break;

        if (_grid.AddCosmonoToGrid(_currentCosmono))
        {
            _currentCosmono.SetCosmonoShape();

            var wasHidden = GhostPiece.Hidden;
            GhostPiece = _currentCosmono.CreateGhostPiece();
            GhostPiece.Hidden = !_ghostPieceEnabled || wasHidden;

            UpdateGhostPiece();

            Window.Instance.GetSoundManager().PlaySFX("harddrop");

            return true;
        }

        return false;
    }

    public void UpdatePosition(Cosmono cosmono)
    {
        // Update the _currentCosmono's position
        // Check if the _currentCosmono has collided
        // If collided, lock the _currentCosmono in place
        // Check for completed lines
        // Spawn a new _currentCosmono
        var screenPosition = GridToScreenPosition(cosmono.GridPosition);
        cosmono.Position = screenPosition - new Vector2(cosmono.Size.X / 2.0f, cosmono.Size.Y / 2.0f);
    }

    public void Update(GameTime gameTime)
    {
        UpdatePosition(_currentCosmono);

        _currentCosmono.Update(gameTime);

        _grid.IsCollidingOrAtBottom(_currentCosmono);

        HandleGravity(gameTime);

        UpdateGridDestroyProgress(gameTime);

        UpdatePosition(_currentCosmono);

        GhostPiece.Update(gameTime);

        ActiveGameMode.Update(gameTime);

        if (_isFadingOut)
        {
            _curGhostOpacity -= _opacityChange * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_curGhostOpacity <= 0.15f)
            {
                _curGhostOpacity = 0.15f;
                _isFadingOut = false;
            }
        }
        else
        {
            _curGhostOpacity += _opacityChange * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_curGhostOpacity >= 0.35f)
            {
                _curGhostOpacity = 0.35f;
                _isFadingOut = true;
            }
        }

        if (_grid.GetShakeAnimator().IsShaking) _grid.GetShakeAnimator().Update(gameTime);

        GhostPiece.Color = Color.White * _curGhostOpacity;
        foreach (var part in GhostPiece.CosmonoParts) part.Color = Color.White * _curGhostOpacity;

        _currentCosmono.NextCosmonoes.UpdateNextCosmonoesRender();
        _currentCosmono.HeldCosmono.UpdateHeldCosmonoRender();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_grid?.GetGridTexture() == null) return;
        var shaking = _grid.GetShakeAnimator().IsShaking;

        if (_dissolvingGrid)
        {
            _dissolveThreshold -= 0.0008f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var threshold = _dissolveThreshold;
            _dissolveEffect.Effect.Parameters["Threshold"].SetValue(threshold);
            _dissolveEffect.Effect.Parameters["TextureSampler"].SetValue(_grid.GetGridTexture());
            _dissolveEffect.ApplyEffect();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null,
                _dissolveEffect.Effect);
        }

        if (shaking) spriteBatch.Begin(transformMatrix: _grid.GetShakeAnimator().GetCurrentMatrix());

        _grid.DrawGrid(spriteBatch, gameTime, _dissolvingGrid || shaking);

        if (_dissolvingGrid || shaking)
            spriteBatch.End();

        spriteBatch.Begin();
        GhostPiece.Draw(spriteBatch);

        _currentCosmono.NextCosmonoes.Draw(spriteBatch);

        _currentCosmono.HeldCosmono.Draw(spriteBatch);

        ActiveGameMode.Draw(spriteBatch);

        spriteBatch.End();
    }

    private void HandleGravity(GameTime gameTime)
    {
        if (_currentCosmono.IsGhost() || _currentCosmono.Hidden || _currentCosmono.IsFrozen() ||
            _currentCosmono.InSpawnDelay())
            return;

        _gravityTimer += 1.0f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (_gravityTimer >= _gravity)
        {
            _gravityTimer = 0.0f;

            MoveDown();
        }

        if (!_currentCosmono.IsGrounded()) return;

        if (!_currentCosmono.HasGrounded() && _moveResetCounter < 1)
            Window.Instance.GetSoundManager().PlaySFX(_currentCosmono.GetScore().Level < 8 ? "fall" : "fallhard");

        _lockDelayTimer += 1.0f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (_lockDelayTimer >= _lockDelay) LockCosmono();
    }

    public bool HandleHold()
    {
        var locked = _currentCosmono.HeldCosmono.IsLocked() || !_holdEnabled;

        if (locked || _currentCosmono.IsFrozen() || _currentCosmono.IsGhost())
        {
            if (locked)
                Window.Instance.GetSoundManager().PlaySFX("holdfail");

            return false;
        }

        var heldShape = _currentCosmono.HeldCosmono.SwapHeldShape(_currentCosmono.GetCosmonoShape());

        if (heldShape != null)
        {
            _currentCosmono.SetCosmonoShape(heldShape);

            var wasHidden = GhostPiece.Hidden;
            GhostPiece = _currentCosmono.CreateGhostPiece();
            GhostPiece.Hidden = !_ghostPieceEnabled || wasHidden;
        }
        else
        {
            _currentCosmono.SetCosmonoShape();
            _currentCosmono.HeldCosmono.Lock();
        }

        UpdateGhostPiece();
        _lockDelayTimer = 0.0f;
        _moveResetCounter = 0;  

        return true;
    }

    public void ResetLockDelay()
    {
        if (_currentCosmono.IsGrounded())
        {
            _moveResetCounter++;
            if (_moveResetCounter >= MaxMoveResets)
                LockCosmono();
            else
                _lockDelayTimer = 0.0f;
        }
        else if (_moveResetCounter <= MaxMoveResets)
        {
            _lockDelayTimer = 0.0f;
        }
    }

    private void LockCosmono()
    {
        UpdatePosition(_currentCosmono);

        if (!_grid.IsCollidingOrAtBottom(_currentCosmono))
            return;

        _lockDelayTimer = 0.0f; // Reset the lock delay timer for the next piece
        _moveResetCounter = 0; // Reset the move reset counter for the next piece
        _grid.AddCosmonoToGrid(_currentCosmono);
        _currentCosmono.SetCosmonoShape();

        var wasHidden = GhostPiece.Hidden;
        GhostPiece = _currentCosmono.CreateGhostPiece();
        GhostPiece.Hidden = !_ghostPieceEnabled || wasHidden;
        UpdateGhostPiece();
    }

    public void UpdateGravity()
    {
        var level = _currentCosmono.GetScore().Level;

        if (level > 20)
            level = 20;

        _gravity = (float)(Math.Round(Math.Pow(0.8 - (level - 1) * 0.007, level - 1), 5) *
                           (1000.0f / _gravityMultiplier));
    }

    public void FreezeCosmono(bool freeze)
    {
        if (freeze)
            _currentCosmono.Freeze();
        else
            _currentCosmono.UnFreeze();
    }

    public Vector2 GridToScreenPosition(Vector2 gridPosition)
    {
        var cellSize = _grid.GetCellSize();
        var lineWidth = _grid.GetLineWidth();
        var borderSize = _grid.GetBorderWidth();

        var gridOffset = _grid.GetActualPosition() + new Vector2(lineWidth + borderSize, lineWidth + borderSize - 1);

        var complete = new Vector2(
            gridPosition.X * (cellSize + lineWidth) + gridOffset.X + cellSize / 2.0f,
            gridPosition.Y * (cellSize + lineWidth) + gridOffset.Y + cellSize / 2.0f
        );

        return complete;
    }

    public TetrisGrid GetGrid()
    {
        return _grid;
    }

    public void GameOver()
    {
        if (Cosmetris.GameState == GameState.GameOver)
            return;

        _grid.CreateMessageLabel("Game Over!", Color.Red);
        _currentCosmono.Freeze();
        _currentCosmono.Hidden = true;
        GhostPiece.Hidden = true;

        // Start destroying the grid
        _destroyingGrid = true;

        Cosmetris.UpdateGameState(GameState.GameOver);

        Window.Instance.GetSoundManager().StopMusic();
    }

    public void UpdateGridDestroyProgress(GameTime gameTime)
    {
        if (_destroyingGrid)
        {
            if (_grid.GetDebrisAnimator().CrumbleGrid(gameTime))
                return;

            if (!_justCrumbled)
            {
                _justCrumbled = true;
                Window.Instance.GetSoundManager().PlaySFX("gameover");
            }

            if (_destroyWaitTimer < 100.0f)
            {
                _destroyWaitTimer += 1.0f * (float)GameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }

            // Check to see if there are any rows left to destroy
            if (_destroyCount >= _grid.TotalRows())
            {
                _destroyingGrid = false;
                _dissolvingGrid = true;

                Timer.Instance.CreateTimer(2000, (sender, e) =>
                {
                    Window.Instance.ScreenRenderer().SetScreen(new GameOverScreen(this));
                    CosmonoRain.Instance.ShowRain();
                });

                _destroyWaitTimer = 0.0f;
                _destroyCount = 0;
            }
            else
            {
                // Destroy the next row
                _grid.RemoveRow(_destroyCount);
                _destroyWaitTimer = 0.0f;
                _destroyCount++;
            }
        }
    }

    public void ResetGravity()
    {
        _gravityTimer = 0.0f;
    }

    public Dictionary<string, int> GetScoreList()
    {
        return _currentCosmono.GetScore().GetScoreList();
    }

    public void Dispose()
    {
        _grid.Dispose();
        ActiveGameMode?.Dispose();
    }
}