/*
 * Cosmono.cs is part of Cosmetris.
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
using Cosmetris.Game.Objects.Cosmonoes.Util;
using Cosmetris.Input;
using Cosmetris.Render;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Objects.Cosmonoes;

public class Cosmono : Object
{
    /// <summary>
    ///     How long to wait after spawning the new Cosmono before applying gravity.
    /// </summary>
    private const float SPAWN_DELAY = 500.0f;

    private static int _instanceCounter = 0;

    private readonly bool _isGhost;

    /// <summary>
    ///     The GameManager & Grid the Cosmono is attached to.
    /// </summary>
    private readonly TetrisGameManager _tetrisGameManager;

    private bool _hasGrounded;
    private bool _isFrozen;

    private bool _isGrounded;
    private bool _isInSpawnDelay;
    private bool _lastMoveWasRotation;

    private bool _lastRotationWasKick;
    private RotationData.Rotation _rotation;
    private int _rotationIndex;
    private float _spawnDelayTimer;

    public Cosmono(string name, TetrisGameManager gameManager, Vector2 position, Vector2 size, Texture2D texture,
        Color color)
        : base(name, position, size, texture, color)
    {
        // Kind of a sloppy way of checking, but it's good enough
        _isGhost = name.ToLower().Contains("ghost");

        _cosmonoShape = new CosmonoShape();
        _cosmonoShape = _cosmonoShape.Shapes[0];
        _tetrisGameManager = gameManager;
        GridPosition = position;

        Size = size;

        // Initialize to a random shape
        CosmonoParts = new List<CosmonoPiece>
        {
            new(_tetrisGameManager, "Cosmono_Main", Vector2.Zero, size, texture, color),
            new(_tetrisGameManager, "Cosmono_Left", new Vector2(-1, 0) * size, size, texture, color),
            new(_tetrisGameManager, "Cosmono_Right", new Vector2(1, 0) * size, size, texture, color),
            new(_tetrisGameManager, "Cosmono_Top", new Vector2(0, 1) * size, size, texture, color)
        };

        // Set the parent of each part to this cosmono
        foreach (var part in CosmonoParts) part.SetParent(this);

        if (_isGhost) return;

        HeldCosmono = new HeldCosmono(_tetrisGameManager);
        NextCosmonoes = new NextCosmonoes(_tetrisGameManager);
        SetCosmonoShape();
        _score = new Score(_tetrisGameManager);
    }

    /// <summary>
    ///     Each Cosmono is made up of 4 CosmonoPieces.
    /// </summary>
    public List<CosmonoPiece> CosmonoParts { get; set; }

    /// <summary>
    ///     The Next Cosmono bag that is used to generate the next Cosmono.
    /// </summary>
    public NextCosmonoes NextCosmonoes { get; set; }

    /// <summary>
    ///     The Cosmono that is currently being held.
    /// </summary>
    public HeldCosmono HeldCosmono { get; set; }

    /// <summary>
    ///     The position of the Cosmono on the grid.
    /// </summary>
    public Vector2 GridPosition { get; set; }

    /// <summary>
    ///     The current shape & offsets of the Cosmono.
    /// </summary>
    private CosmonoShape _cosmonoShape { get; set; }

    private Score _score { get; }

    public override void Update(GameTime gameTime)
    {
        // We need to wait for game manager to be initialized
        if (_tetrisGameManager == null) return;

        // Don't apply gravity until spawn delay is over.
        if (_isInSpawnDelay)
        {
            _spawnDelayTimer -= 1.0f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_spawnDelayTimer <= 0.0f) _isInSpawnDelay = false;
        }

        if (_score != null)
            // Check to see if player is ready to level up
            if (_score.CheckForLevelUp())
            {
                Window.Instance.GetSoundManager().PlaySFX("lvlup");
                // Update the gravity interval based on the new level
                _tetrisGameManager.UpdateGravity();
            }

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Hidden)
            return;

        if (_children != null)
            foreach (var child in _children)
                child.Draw(spriteBatch);
    }

    public override void OnButtonDown(object sender, Controller.ControllerButton e)
    {
        base.OnButtonDown(sender, e);

        if (_isFrozen || _tetrisGameManager.CurrentCosmono() != this || _isGhost)
            return;

        switch (e.Name)
        {
            case "Left":
                if (_tetrisGameManager.MoveLeft())
                {
                    Window.Instance.GetSoundManager().PlaySFX("move");
                    _tetrisGameManager.ResetLockDelay();
                    _lastMoveWasRotation = false;
                }

                break;
            case "Right":
                if (_tetrisGameManager.MoveRight())
                {
                    Window.Instance.GetSoundManager().PlaySFX("move");
                    _tetrisGameManager.ResetLockDelay();
                    _lastMoveWasRotation = false;
                }

                break;
            case "Hold":
                if (_tetrisGameManager.HandleHold())
                    _lastMoveWasRotation = false;
                break;
            case "Up":
                if (ShowDebug)
                {
                    // Set the board to the next dev board
                    _tetrisGameManager.GetGrid().LoadNextBoard();
                }
                break;
            case "Down":
                if (_tetrisGameManager.MoveDown())
                {
                    Window.Instance.GetSoundManager().PlaySFX("move");
                    _tetrisGameManager.ResetLockDelay();
                    _lastMoveWasRotation = false;
                }

                break;
            case "Rotate Right":
                if (_tetrisGameManager.RotateRight())
                {
                    Window.Instance.GetSoundManager().PlaySFX("rotate");
                    _tetrisGameManager.ResetLockDelay();
                }

                break;
            case "Rotate Left":
                if (_tetrisGameManager.RotateLeft())
                {
                    Window.Instance.GetSoundManager().PlaySFX("rotate");
                    _tetrisGameManager.ResetLockDelay();
                }

                break;
            case "Hard Drop":
                if (_tetrisGameManager.HardDrop())
                    _lastMoveWasRotation = false;
                break;
        }
    }

    /// <summary>
    ///     Sets the Cosmono to the next shape in the queue.
    ///     If a shape is provided, this will be skipped.
    ///     This function also sets the initial position of the Cosmono.
    /// </summary>
    /// <param name="shape">Optional pre-defined new shape</param>
    public void SetCosmonoShape(CosmonoShape shape = null)
    {
        var shouldUnlock = false;

        if (shape == null)
        {
            shouldUnlock = true;
            _cosmonoShape = NextCosmonoes.GetNextCosmonoShape();
        }
        else
        {
            _cosmonoShape = shape;
        }

        // Set initial spawning positions
        var spawnPosition = new Vector2(4, 0); // Default position for most cosmono shapes

        // SRS pushes the position of the I and O blocks down by one cell
        if (_cosmonoShape.Equals("I") || _cosmonoShape.Equals("O"))
            spawnPosition = new Vector2(4, -2);

        GridPosition = spawnPosition;

        // Apply the shape's offsets to the cosmono parts
        for (var i = 0; i < CosmonoParts.Count; i++)
        {
            CosmonoParts[i].Position = _cosmonoShape.Offsets[i];
            CosmonoParts[i].Texture = _cosmonoShape.Texture;
            CosmonoParts[i].PlacedTexture = _cosmonoShape.PlacedTexture;
        }

        // Always spawn with rotation index 0
        _rotationIndex = 0;
        _rotation = RotationData.Rotation.Zero;

        // Update World Position
        Position = GridPosition * Size;

        // Reset local states
        _lastRotationWasKick = false;
        _lastMoveWasRotation = false;
        _isGrounded = false;
        _hasGrounded = false;

        _tetrisGameManager.UpdatePosition(this);

        // If it's I or O cosmono, try to move it down by one cell if possible
        if (_cosmonoShape.Equals("I") || _cosmonoShape.Equals("O"))
            if (_tetrisGameManager.GetGrid().IsPartValid(GridPosition + CosmonoParts[0].Position + Vector2.UnitY))
            {
                GridPosition += Vector2.UnitY;
                _tetrisGameManager.UpdatePosition(this);
            }

        //Check if the piece can be placed. If not, it's game over.
        foreach (var part in CosmonoParts)
            if (!_tetrisGameManager.GetGrid().IsPartValid(GridPosition + part.Position))
            {
                _tetrisGameManager.GameOver();
                return;
            }

        if (shouldUnlock)
            HeldCosmono?.Unlock();

        _isInSpawnDelay = true;
        _spawnDelayTimer = SPAWN_DELAY;

        // Make sure gravity timer is reset
        _tetrisGameManager.ResetGravity();
    }

    /// <summary>
    ///     Attempt to rotate the Cosmono.
    /// </summary>
    /// <param name="left">Whether or not to rotate left.</param>
    /// <returns>True if the rotation was successful, false otherwise.</returns>
    public bool RotateCosmono(bool left)
    {
        _lastMoveWasRotation = true;
        _lastRotationWasKick = false;

        // Make sure our position is up to date
        _tetrisGameManager.UpdatePosition(this);

        // Cache the original rotation index and rotation
        var originalRotationIndex = _rotationIndex;
        var originalRotation = _rotation;

        // Save the original positions of the parts
        var originalPositions = CosmonoParts.Select(part => part.Position).ToList();

        Vector2 rotationCenter; // I and O use different rotation centers
        if (_cosmonoShape.Equals("I") || _cosmonoShape.Equals("O"))
            rotationCenter = new Vector2(0.5f, 0.5f);
        else
            rotationCenter = CosmonoParts[0].Position;

        // Perform the rotation
        foreach (var t in CosmonoParts)
        {
            var relativePosition = t.Position - rotationCenter;
            if (left)
                t.Position = rotationCenter + new Vector2(relativePosition.Y, -relativePosition.X);
            else
                t.Position = rotationCenter + new Vector2(-relativePosition.Y, relativePosition.X);
        }

        // Apply the rotation offset to the cosmono parts
        _tetrisGameManager.UpdatePosition(this);

        // Make sure to update the rotation index
        UpdateRotationIndex(left);

        // ghosts dont need all dis computation
        if (_isGhost)
            return true;

        // If the rotation resulted in a collision, try to resolve it with the kick data
        if (_tetrisGameManager
            .CheckCollision(
                this)) // Assume CheckCollision is a method that checks if this cosmono is colliding with the wall or other cosmonoes
        {
            var kickResolved = false;

            List<Vector2> kickData;
            Vector2 invert;
            var isI = _cosmonoShape.Equals("I");
            var isO = _cosmonoShape.Equals("O");

            // i dont even know anymore, inverting the x axis fixes the kick data
            if (left)
            {
                kickData = RotationData.GetKickData(originalRotation, _rotation, isI, isO);
                invert = new Vector2(1, -1);
            }
            else
            {
                kickData = RotationData.GetKickData(_rotation, originalRotation, isI, isO);
                invert = new Vector2(-1, 1);
            }

            foreach (var kick in kickData)
            {
                // Invert x axis??
                // Apply the kick
                GridPosition += kick * invert;
                _tetrisGameManager.UpdatePosition(this);

                // Check if the kick resolved the collision
                if (!_tetrisGameManager.CheckCollision(this))
                {
                    // No collision, we out
                    kickResolved = true;
                    _lastRotationWasKick = true;

                    if (ShowDebug)
                    {
                        var greenPixel = new Object("GreenPixel", Vector2.Zero, Size, RenderUtil.GreenPixel,
                            Color.Green);
                        greenPixel.Position = _tetrisGameManager.GetGrid().GridToScreenPosition(GridPosition);
                        ObjectManager.Instance.AddObject(greenPixel);

                        Timer.Instance.CreateTimer(1000f, (sender, args) =>
                        {
                            ObjectManager.Instance.RemoveObject(greenPixel);
                        });
                    }
                    break;
                }

                // If the kick didn't resolve the collision, undo the kick
                GridPosition -= kick * invert;
                _tetrisGameManager.UpdatePosition(this);

                if (ShowDebug)
                {
                    var redPixel = new Object("RedPixel", Vector2.Zero, Size, RenderUtil.RedPixel, Color.Red);
                    redPixel.Position = _tetrisGameManager.GetGrid().GridToScreenPosition(GridPosition);
                    
                    ObjectManager.Instance.AddObject(redPixel);

                    Timer.Instance.CreateTimer(1000f, (sender, args) =>
                    {
                        ObjectManager.Instance.RemoveObject(redPixel);
                    });
                }
            }

            // If none of the kicks resolved the collision, undo the rotation
            if (!kickResolved)
            {
                for (var i = 0; i < CosmonoParts.Count; i++)
                    CosmonoParts[i].Position = originalPositions[i];

                _tetrisGameManager.UpdatePosition(this);
                _lastMoveWasRotation = false;

                // Revert to the original rotation index
                UpdateRotationIndex(!left);
            }
        }

        return originalRotationIndex != _rotationIndex;
    }

    /// <summary>
    ///     Creates a new Cosmono Object based on the current Cosmono shape.
    /// </summary>
    /// <returns>A "Copy" of the current Cosmono</returns>
    public Cosmono CreateGhostPiece()
    {
        // If the game is over, and the ghost piece is still being drawn, we need to hide it.
        var shouldHide = _isFrozen || Hidden;

        var ghostPiece = new Cosmono(Name + "Ghost", _tetrisGameManager, GridPosition, Size, Texture, Color);
        ghostPiece.Hidden = shouldHide;
        ghostPiece.SetCosmonoShape(_cosmonoShape);
        ghostPiece.Position = Position;
        ghostPiece.GridPosition = GridPosition;
        ghostPiece._rotationIndex = _rotationIndex;
        ghostPiece._rotation = _rotation;

        for (var i = 0; i < ghostPiece.CosmonoParts.Count; i++)
            ghostPiece.CosmonoParts[i].Position = CosmonoParts[i].Position;

        return ghostPiece;
    }

    /// <summary>
    ///     Updates the Cosmono's ground state.
    /// </summary>
    /// <param name="isGrounded">Whether or not the Cosmono is grounded.</param>
    public void UpdateGroundState(bool isGrounded)
    {
        // Used for SFX purposes
        if (!_hasGrounded)
            _hasGrounded = _isGrounded;

        _isGrounded = isGrounded;
    }

    /// <summary>
    ///     Updates the rotation index based on the given direction.
    /// </summary>
    /// <param name="negative">Whether or not to rotate in the negative direction.</param>
    private void UpdateRotationIndex(bool negative)
    {
        _rotationIndex += negative ? -1 : 1;

        if (_rotationIndex < 0)
            _rotationIndex = 3;
        else if (_rotationIndex > 3)
            _rotationIndex = 0;

        // Update the rotation enum
        _rotation = RotationData.RotationIndexToRotation(_rotationIndex);
    }

    public override void Resize()
    {
        NextCosmonoes?.OnResize();
        HeldCosmono?.OnResize();
        base.Resize();
    }

    public void Freeze()
    {
        _isFrozen = true;
    }

    public void UnFreeze()
    {
        _isFrozen = false;
    }

    public CosmonoShape GetCosmonoShape()
    {
        return _cosmonoShape;
    }

    /// <summary>
    ///     Returns the score of the Cosmono.
    /// </summary>
    /// <returns>The score of the Cosmono.</returns>
    public Score GetScore()
    {
        return _score;
    }

    /// <summary>
    ///     Determines whether or not the Cosmono is a ghost piece.
    /// </summary>
    /// <returns>True if the Cosmono is a ghost piece, false otherwise.</returns>
    public bool IsGhost()
    {
        return _isGhost;
    }

    public bool InSpawnDelay()
    {
        return _isInSpawnDelay;
    }

    /// <summary>
    ///     Determines whether or not the last move was a rotation(excluding _ignoredButtons).
    /// </summary>
    /// <returns>True if the last move was a rotation, false otherwise.</returns>
    public bool LastRotationWasKick()
    {
        return _lastRotationWasKick;
    }

    /// <summary>
    ///     Returns the current rotation enum of the Cosmono.
    /// </summary>
    /// <returns>The current rotation enum of the Cosmono.</returns>
    public RotationData.Rotation ShapeRotation()
    {
        return _rotation;
    }

    /// <summary>
    ///     Determines whether or not the Cosmono is frozen.
    /// </summary>
    /// <returns>True if the Cosmono is frozen, false otherwise.</returns>
    public bool IsFrozen()
    {
        return _isFrozen;
    }

    /// <summary>
    ///     Determines whether or not the Cosmono is touching the ground.
    /// </summary>
    /// <returns>True if the Cosmono is touching the ground, false otherwise.</returns>
    public bool IsGrounded()
    {
        return _isGrounded;
    }

    /// <summary>
    ///     Determines whether or not the Cosmono has touched the ground.
    /// </summary>
    /// <returns>True if the Cosmono has touched the ground, false otherwise.</returns>
    public bool HasGrounded()
    {
        return _hasGrounded;
    }

    public override void Dispose()
    {
        HeldCosmono?.Dispose();
        NextCosmonoes?.Dispose();
        _tetrisGameManager?.Dispose();
        
        base.Dispose();
    }

    public bool LastMoveWasRotation()
    {
        return _lastMoveWasRotation;
    }
    
    public void SetWasLastMoveRotation(bool wasRotation)
    {
        _lastMoveWasRotation = wasRotation;
    }
}