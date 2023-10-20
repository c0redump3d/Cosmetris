/*
 * TetrisGrid.cs is part of Cosmetris.
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
using Cosmetris.Game.Grid.Util;
using Cosmetris.Game.Grid.Util.Animators;
using Cosmetris.Game.Objects;
using Cosmetris.Game.Objects.Cosmonoes;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.Renderers;
using Cosmetris.Render.UI;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Controls.Animation;
using Cosmetris.Render.UI.Text;
using Cosmetris.Util;
using Cosmetris.Util.Numbers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Game.Grid;

public class TetrisGrid : Control
{
    private static EffectsManager.FX _dissolveEffect;
    private static EffectsManager.FX _galaxyEffect;

    private readonly BoardLoader _boardLoader;
    private readonly int _borderWidth;
    private readonly int _cellSize;
    private readonly int _columns;

    private readonly DebrisAnimator _debrisAnimator;


    private readonly Vector2 _finalSize;
    private readonly Texture2D _gridTexture;
    private readonly int _lineWidth;
    private readonly Font _messageFont = FontRenderer.Instance.GetFont("orbitron", 96);
    private readonly List<Label> _messageLabels = new();
    private readonly int _rows;
    private readonly List<int> _rowsBeingDeleted = new(); // row number -> timestamp marked for deletion

    private readonly Dictionary<int, double>
        _rowsPendingDeletion = new(); // row number -> timestamp marked for deletion

    private readonly Font _scoreFont = FontRenderer.Instance.GetFont("orbitron", 48);

    private readonly List<Label> _scoreLabels = new();
    private readonly ShakeAnimator _shakeAnimator;
    private readonly bool _shouldShowPlacedTexture = true;
    private readonly TSpinHandler _tSpinHandler;
    private Cell[,] _cells;

    private TetrisGameManager _gameManager;
    private readonly Texture2D _lockedTexture;

    private bool _wasPerfectClear;
    private bool _wasTSpin;

    /// <summary>
    ///     Event that is invoked when the grid finishes dissolving.
    /// </summary>
    public EventHandler<EventArgs> OnGridDissolveComplete;

    /// <summary>
    ///     Event that is invoked when the grid starts dissolving.
    /// </summary>
    public EventHandler<EventArgs> OnGridDissolveStart;

    /// <summary>
    ///     Event that is invoked when all queued rows are removed.
    /// </summary>
    public EventHandler<RowsRemovedEventArgs> OnRowsRemoved;

    public TetrisGrid(int rows, int columns, int cellSize, int lineWidth, int borderWidth)
    {
        _rows = rows + 2;
        _columns = columns;
        _cellSize = cellSize;
        _lineWidth = lineWidth;
        _borderWidth = borderWidth;
        Size = Vector2.One;
        _finalSize = new Vector2(_columns * (_cellSize + _lineWidth) + 2 * _borderWidth,
            rows * (_cellSize + _lineWidth) + 2 * _borderWidth);

        _gridTexture = RenderUtil.GenerateGridTexture(rows, columns, cellSize, borderWidth, lineWidth, _finalSize,
            Cosmetris.Instance.GraphicsDevice);

        // Initialize the cells
        InitializeCells();

        _boardLoader = new BoardLoader(this);

        _tSpinHandler = new TSpinHandler(this);
        _debrisAnimator = new DebrisAnimator(this);
        _shakeAnimator = new ShakeAnimator();

        SetAnimation(new ScaleAnimation(this, Size, _finalSize, 0.65f));
        SetClosingAnimation(new ScaleClosingAnimation(this, _finalSize, Size, 0.65f));

        // See if we have a current texture pack
        var pack = TextureManager.Instance.CurrentTexturePack;
        if (pack != null)
        {
            var showPlaced = !pack.SingleBlockTexture;
            _shouldShowPlacedTexture = showPlaced;
        }

        // Load the dissolve shader if needed
        if (_dissolveEffect == null)
        {
            _dissolveEffect = new EffectsManager.FX("disolve",
                ScalingManager.DesiredWidth, ScalingManager.DesiredHeight);
            _galaxyEffect = EffectsManager.Instance.GetFX("galaxy");
            EffectsManager.Instance.AddEffect(_dissolveEffect);
            var dissolveMask = Cosmetris.Instance.Content.Load<Texture2D>("Textures/FX/noise");
            _dissolveEffect.Effect.Parameters["DissolveMask"].SetValue(dissolveMask);
        }

        OnGridDissolveComplete += DeleteRows;

        // "locked" or the "garbage" texture
        _lockedTexture = TextureManager.Instance.GetTexture2D("x_block");

        Initialize();
    }

    /// <summary>
    ///     Draws the grid and the placed blocks.
    /// </summary>
    /// <param name="spriteBatch"> The sprite batch to use. </param>
    /// <param name="gameTime"> The game time. </param>
    /// <param name="isGridDissolving"> Whether or not the grid is dissolving. </param>
    public void DrawGrid(SpriteBatch spriteBatch, GameTime gameTime, bool isGridDissolving)
    {
        // Use standard batch call when not dissolving
        if (!isGridDissolving)
            spriteBatch.Begin();

        // Draw the grid
        spriteBatch.Draw(_gridTexture,
            new Rectangle(ScalingManager.GetScaledX(GetActualPosition().X),
                ScalingManager.GetScaledY(GetActualPosition().Y), ScalingManager.GetScaledX(Size.X),
                ScalingManager.GetScaledY(Size.Y)), Color.White * GetOpacity());

        // Also need to end the batch call when not dissolving
        if (!isGridDissolving)
            spriteBatch.End();

        // Draw the placed blocks; this function purposely ignores the top two rows
        for (var row = 2; row < _rows; row++)
        {
            var isRowBeingDeleted = _rowsPendingDeletion.ContainsKey(row);

            // If the row is marked for deletion, adjust the shader parameters
            if (isRowBeingDeleted && !isGridDissolving)
            {
                _rowsPendingDeletion[row] -= 1.0f * gameTime.ElapsedGameTime.TotalMilliseconds;
                var elapsedTimeSinceMarked = (float)_rowsPendingDeletion[row];
                var threshold = Math.Clamp(elapsedTimeSinceMarked / 500.0f, 0f, 1f);

                // Update the threshold
                _dissolveEffect.Effect.Parameters["Threshold"].SetValue(threshold);
                _dissolveEffect.ApplyEffect();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null,
                    _dissolveEffect.Effect);
            }
            else
            {
                if (!isGridDissolving)
                    spriteBatch.Begin();
            }

            for (var col = 0; col < _columns; col++)
            {
                // No need to draw empty cells
                if (!_cells[row, col].IsOccupied) continue;

                if (!isGridDissolving)
                {
                    // Update the current texture being sampled
                    _dissolveEffect.Effect.Parameters["TextureSampler"].SetValue(_cells[row, col].Texture);
                    _dissolveEffect.ApplyEffect();
                }

                // Convert our grid position to screen position
                var screenPosition = GridToScreenPosition(new Vector2(col, row - 2)) +
                                     new Vector2(_lineWidth, _lineWidth);

                // If the row is being deleted, draw the block as white
                var drawColor =
                    isRowBeingDeleted ? Color.White : _cells[row, col].Color;

                // Important to note that each position and size is scaled based on the current screen size
                spriteBatch.Draw(
                    _cells[row, col].Texture,
                    new Rectangle(
                        ScalingManager.GetScaledX(screenPosition.X),
                        ScalingManager.GetScaledY(screenPosition.Y),
                        ScalingManager.GetScaledX(_cellSize),
                        ScalingManager.GetScaledY(_cellSize)),
                    drawColor * ObjectManager.Instance.GetOpacity());
            }

            if (!isGridDissolving)
                spriteBatch.End();
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderChild(spriteBatch, gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        List<Control> controls = null;
        // check if any children are marked for deletion
        foreach (var child in ChildControls)
            if (child.IsMarkedForDeletion)
            {
                controls ??= new List<Control>();
                controls.Add(child);
            }
        
        // remove any marked children
        if (controls != null)
            foreach (var control in controls)
                ChildControls.Remove(control);

        base.Update(gameTime);
    }

    /// <summary>
    ///     Creates a label that displays the last score action.
    /// </summary>
    /// <param name="text"> The text to display. </param>
    /// <param name="color"> The color of the text. </param>
    public void CreateScoreLabel(string text, Color color = default)
    {
        var pos = GetActualPosition();
        var yOffset = _scoreLabels.Count * 50f;

        // Center the label to the right of the grid
        var length = _scoreFont.MeasureString(text).X;
        var label = new Label(text, new Vector2(pos.X - length - 25f, pos.Y + Size.Y / 2 + yOffset), _scoreFont,
            color == default ? Color.White : color);
        _scoreLabels.Add(label);

        // Add the label to the screen
        Window.Instance.ScreenRenderer().GetScreen().GetControls().Add(label);
        // Create a timer to remove the label
        Timer.Instance.CreateTimer(650f, (sender, e) =>
        {
            label.ScaleOut();
            _scoreLabels.Remove(label);
        });
    }

    public void CreateMessageLabel(string text, Color color = default, float time = 1250f)
    {
        var pos = Position;
        var yOffset = 0f;

        // Center label with the grid
        var label = new Label(text, new Vector2(Size.X / 2, Size.Y / 2 - _messageFont.GetHeight()),
            _messageFont, color == default ? Color.White : color, Label.Align.Center, Label.Overflow.Wrap, 0.95f,
            _gridTexture.Width);
        //label.SetAnimation(new ScaleAnimation(label, Vector2.Zero, new Vector2(1.5f, 1.5f), 0.5f));
        _messageLabels.Add(label);

        // Add the label to the screen
        ChildControls.Add(label);
        // Create a timer to remove the label
        Timer.Instance.CreateTimer(time, (sender, e) =>
        {
            label.FadeOut(time/4f);
        });
    }

    public void SetGameManager(TetrisGameManager manager)
    {
        _gameManager = manager;
    }

    private void InitializeCells()
    {
        _cells = new Cell[_rows, _columns];
        for (var row = 0; row < _rows; row++)
        for (var col = 0; col < _columns; col++)
            _cells[row, col] = new Cell();
    }

    private bool CanPlaceCosmono(Cosmono cosmono, Vector2 position)
    {
        return !(from part in cosmono.CosmonoParts
            select position + part.Position * _cellSize + new Vector2(_cellSize / 2.0f, _cellSize / 2.0f)
            into worldPosition
            select ScreenToGridPosition(worldPosition)
            into gridPosition
            let row = (int)gridPosition.Y + 2
            let col = (int)gridPosition.X
            where row < 0 || row >= _rows || col < 0 || col >= _columns || _cells[row, col].IsOccupied
            select row).Any();
    }

    /// <summary>
    ///     Adds the current cosmono to the grid based on its position.
    /// </summary>
    /// <param name="cosmono"> The cosmono to add. </param>
    /// <returns> Whether or not the cosmono was added. </returns>
    public bool AddCosmonoToGrid(Cosmono cosmono)
    {
        if (!CanPlaceCosmono(cosmono, cosmono.Position) || cosmono.IsGhost()) return false;

        var rowsThisTurn = 0;

        foreach (var part in cosmono.CosmonoParts)
        {
            var worldPosition = cosmono.Position + part.Position * _cellSize +
                                new Vector2(_cellSize / 2.0f, _cellSize / 2.0f);

            var gridPosition = ScreenToGridPosition(worldPosition);

            var row = (int)gridPosition.Y + 2; // Row is offset by 2
            var col = (int)gridPosition.X;

            // Set the cell to occupied
            _cells[row, col].IsOccupied = true;
            _cells[row, col].Texture = _shouldShowPlacedTexture ? part.PlacedTexture : part.Texture;
            _cells[row, col].Color = part.Color;

            // Check if row is full
            if (!IsRowFull(row)) continue;

            _rowsPendingDeletion.Add(row, 500);

            OnGridDissolveStart?.Invoke(this, EventArgs.Empty);

            // Create a timer for row deletion
            Timer.Instance.CreateTimer(500, (sender, e) => OnGridDissolveComplete?.Invoke(sender, e), "removeTimer");

            rowsThisTurn++;
        }

        var rowsRemoved = rowsThisTurn;

        // Check for T-Spin
        _wasTSpin = _tSpinHandler.IsTSpin(cosmono, rowsRemoved);
        _wasPerfectClear = false;

        if (!_wasTSpin)
            if (cosmono.GetCosmonoShape().Equals("I"))
            {
                var wasTetris = rowsRemoved == 4;
                if (cosmono.GetScore().LastWasBackToBack && wasTetris) cosmono.GetScore().HandleBackToBack();
            }

        // TODO: Make sure this isn't actually needed
        //cosmono.GetScore().LastWasBackToBack = false;
        // Update the score
        cosmono.GetScore().HandleLinesCleared(rowsRemoved, _wasTSpin);

        if (rowsRemoved > 0)
        {
            if (rowsRemoved >= 4 || _wasPerfectClear)
                Window.Instance.GetSoundManager().PlaySFX("crushperfect");
            else
                Window.Instance.GetSoundManager().PlaySFX("crush");
        }

        return true;
    }


    /// <summary>
    ///     Adds garbage to the grid.
    /// </summary>
    /// <param name="rows"> The number of rows to add. </param>
    public void AddGarbage(int rows)
    {
        if (rows == 0)
            return;

        // move rows up based on how many we need to add
        for (var row = 0; row < _rows - rows; row++)
        for (var col = 0; col < _columns; col++)
        {
            _cells[row, col].IsOccupied = _cells[row + rows, col].IsOccupied;
            _cells[row, col].Texture = _cells[row + rows, col].Texture;
            _cells[row, col].Color = _cells[row + rows, col].Color;
        }

        // add the garbage, always leave one column open
        for (var row = _rows - rows; row < _rows; row++)
        {
            var randCol = RandomUtil.Next(0, _columns - 1);
            for (var col = 0; col < _columns; col++)
            {
                if (col == randCol)
                {
                    _cells[row, col].IsOccupied = false;
                    continue;
                }

                _cells[row, col].IsOccupied = true;
                _cells[row, col].Texture = _lockedTexture;
                _cells[row, col].Color = Color.White;
            }
        }
    }

    /// <summary>
    ///     Event function that will remove all rows marked for deletion.
    /// </summary>
    /// <param name="sender"> The sender. </param>
    /// <param name="e"> The event args. </param>
    private void DeleteRows(object sender, EventArgs e)
    {
        var rowsRemoved = 0;

        // Remove rows marked for deletion
        foreach (var row in _rowsPendingDeletion)
        {
            _rowsBeingDeleted.Add(row.Key);
            rowsRemoved++;
        }

        _rowsPendingDeletion.Clear();

        // Sort the rows in descending order
        _rowsBeingDeleted.Sort((a, b) => b.CompareTo(a));

        for (var i = 0; i < _rowsBeingDeleted.Count; i++)
        {
            // Clear the row
            RemoveRow(_rowsBeingDeleted[i] + i);
            // Add debris particles for each row
            _debrisAnimator.EmitDebrisFromRow(_rowsBeingDeleted[i] + i, 1);
        }

        _rowsBeingDeleted.Clear();

        // Update the background renderers decay factor
        BackgroundRenderer.Instance.DecayFactor = 1.0f;

        // Update the galaxy shader
        _galaxyEffect.Effect.Parameters["LinesCompleted"].SetValue((float)rowsRemoved);
        _galaxyEffect.ApplyEffect();

        var fullRows = rowsRemoved;

        // This stuff should only be done if we have actually completed a row
        if (fullRows > 0)
        {
            // Check for perfect clear
            _wasPerfectClear = CheckPerfectClear(_gameManager.CurrentCosmono());

            if (fullRows >= 4 || _wasPerfectClear)
                Window.Instance.GetSoundManager().PlaySFX("rowfallbig");
            else
                Window.Instance.GetSoundManager().PlaySFX("rowfall");
        }

        // Invoke the rows removed event
        var eventArgs = new RowsRemovedEventArgs(fullRows, _wasTSpin, _wasPerfectClear);
        OnRowsRemoved?.Invoke(this, eventArgs);

        // Fix the ghost's position
        _gameManager.UpdateGhostPiece();
    }

    /// <summary>
    ///     Clears a row.
    /// </summary>
    /// <param name="row"> The row to clear. </param>
    public void RemoveRow(int row)
    {
        // Make sure row is in bounds
        if (row < 0 || row >= _rows) return;

        // Move all rows above down
        for (var r = row - 1; r >= 0; r--)
        for (var col = 0; col < _columns; col++)
        {
            _cells[r + 1, col].IsOccupied = _cells[r, col].IsOccupied;
            _cells[r + 1, col].Texture = _cells[r, col].Texture;
            _cells[r + 1, col].Color = _cells[r, col].Color;
        }

        // Clear the top row
        for (var col = 0; col < _columns; col++) _cells[0, col].IsOccupied = false;
    }


    /// <summary>
    ///     Checks if the grid is empty.
    /// </summary>
    /// <param name="cosmono"> The current cosmono. </param>
    /// <returns> Whether or not the grid is empty. </returns>
    private bool CheckPerfectClear(Cosmono cosmono)
    {
        var allClear = true;
        for (var r = 0; r < _rows; r++)
        {
            for (var col = 0; col < _columns; col++)
            {
                if (!_cells[r, col].IsOccupied) continue;

                allClear = false;
                break;
            }

            if (!allClear) break;
        }

        // Credit the player for a perfect clear
        if (allClear) cosmono.GetScore().HandlePerfectClear();

        return allClear;
    }

    /// <summary>
    ///     Converts a screen position to a grid position.
    /// </summary>
    /// <param name="screenPosition"> The position on the screen. </param>
    /// <returns> The position on the grid. </returns>
    public Vector2 ScreenToGridPosition(Vector2 screenPosition)
    {
        var cellSize = GetCellSize();
        var lineWidth = GetLineWidth();
        var borderWidth = GetBorderWidth();

        var gridOffset = GetActualPosition() + new Vector2(borderWidth, borderWidth);

        return new Vector2(
            (int)((screenPosition.X - gridOffset.X) / (cellSize + lineWidth)),
            (int)((screenPosition.Y - gridOffset.Y) / (cellSize + lineWidth))
        );
    }

    /// <summary>
    ///     Converts a grid position to a screen position.
    /// </summary>
    /// <param name="gridPosition"> The position on the grid. </param>
    /// <returns> The position on the screen. </returns>
    public Vector2 GridToScreenPosition(Vector2 gridPosition)
    {
        var cellSize = GetCellSize();
        var lineWidth = GetLineWidth();
        var borderSize = GetBorderWidth();

        var gridOffset = GetActualPosition() + new Vector2(borderSize, borderSize);

        var complete = new Vector2(
            gridPosition.X * (cellSize + lineWidth) + gridOffset.X,
            gridPosition.Y * (cellSize + lineWidth) + gridOffset.Y
        );

        return complete;
    }

    /// <summary>
    ///     Checks if a part of an object is valid.
    /// </summary>
    /// <param name="partPos"> The position of the part. </param>
    /// <param name="fixY"> Whether or not to fix the Y position (typically for rendering). </param>
    /// <returns> Whether or not the part is valid. </returns>
    public bool IsPartValid(Vector2 partPos, bool fixY = true)
    {
        var row = (int)partPos.Y;
        var col = (int)partPos.X;

        if (fixY)
            row += 2;

        // Check out of bounds
        if (row < 0 || row >= _rows || col < 0 || col >= _columns) return false;

        // Check for collision with placed blocks
        return !_cells[row, col].IsOccupied;
    }

    /// <summary>
    ///     Checks if a cosmono would be valid at a given position.
    /// </summary>
    /// <param name="cosmono"> The cosmono to check. </param>
    /// <param name="position"> The position to check. </param>
    /// <returns> Whether or not the cosmono would be valid. </returns>
    public bool WouldBeValid(Cosmono cosmono, Vector2 position)
    {
        if (cosmono.IsGhost())
            return true;

        foreach (var part in cosmono.CosmonoParts)
        {
            var gridPosition = position + part.Position;
            var row = (int)gridPosition.Y + 2;
            var col = (int)gridPosition.X;

            // Check out of bounds
            if (row < 0 || row >= _rows || col < 0 || col >= _columns) return false;

            // Check for collision with placed blocks
            if (_cells[row, col].IsOccupied) return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if a cosmono is colliding with the ground or another placed block.
    /// </summary>
    /// <param name="cosmono"> The cosmono to check. </param>
    /// <returns> Whether or not the cosmono is colliding with the ground or another placed block. </returns>
    public bool IsCollidingOrAtBottom(Cosmono cosmono)
    {
        if (cosmono.IsGhost())
            return false;

        foreach (var part in cosmono.CosmonoParts)
        {
            var gridPosition = cosmono.GridPosition + part.Position;
            var row = (int)gridPosition.Y + 2;
            var col = (int)gridPosition.X;

            // Check out of bounds
            if (row + 1 >= _rows)
            {
                cosmono.UpdateGroundState(true);
                return true;
            }

            // Make sure the part is not outside the grid
            if (row + 1 >= _rows) continue;

            // Check for collision with placed blocks
            if (!_cells[row + 1, col].IsOccupied) continue;

            cosmono.UpdateGroundState(true);
            return true;
        }

        cosmono.UpdateGroundState(false);
        return false;
    }

    /// <summary>
    ///     Checks if the grid is empty.
    /// </summary>
    /// <returns> Whether or not the grid is empty. </returns>
    public bool IsEmpty()
    {
        for (var row = 0; row < _rows; row++)
        for (var col = 0; col < _columns; col++)
            if (_cells[row, col].IsOccupied)
                return false;

        return true;
    }

    /// <summary>
    ///     Checks if a row is full.
    /// </summary>
    /// <param name="row"> The row to check. </param>
    /// <returns> Whether or not the row is full. </returns>
    private bool IsRowFull(int row)
    {
        for (var col = 0; col < _columns; col++)
            if (!_cells[row, col].IsOccupied)
                return false;

        return true;
    }

    /// <summary>
    ///     A enumerable of all the rows that are currently full.
    /// </summary>
    /// <returns> An enumerable of all the rows that are currently full. </returns>
    public IEnumerable<int> GetRows()
    {
        var rows = new List<int>();
        for (var row = 0; row < _rows; row++)
        for (var col = 0; col < _columns; col++)
        {
            if (!_cells[row, col].IsOccupied) continue;

            rows.Add(row);
            break;
        }

        return rows;
    }

    /// <summary>
    ///     Gets the texture used for the grid.
    /// </summary>
    /// <returns> The texture used for the grid. </returns>
    public Texture2D GetGridTexture()
    {
        return _gridTexture;
    }

    /// <summary>
    ///     Gets the size of each cell in the grid.
    /// </summary>
    /// <returns> The size of each cell in the grid. </returns>
    public int GetCellSize()
    {
        return _cellSize;
    }

    public int GetLineWidth()
    {
        return _lineWidth;
    }

    public int GetBorderWidth()
    {
        return _borderWidth;
    }

    public Cell[,] GetCurrentGrid()
    {
        return _cells;
    }

    public int TotalRows()
    {
        return _rows;
    }

    public int TotalColumns()
    {
        return _columns;
    }

    public ShakeAnimator GetShakeAnimator()
    {
        return _shakeAnimator;
    }

    public DebrisAnimator GetDebrisAnimator()
    {
        return _debrisAnimator;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _gridTexture?.Dispose();
        base.Dispose(disposing);
    }

    ~TetrisGrid()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Loads the next dev board.
    /// </summary>
    public void LoadNextBoard()
    {
        var newGrid = _boardLoader.CycleToNextDevBoard();

        if (newGrid == null)
            return;

        _cells = newGrid;
    }

    public Vector2 GetFinalSize()
    {
        return _finalSize;
    }

    public void AddGarbageAtPosition(int row, int col, Color color)
    {
        _cells[row, col].IsOccupied = true;
        _cells[row, col].Texture = _lockedTexture;
        _cells[row, col].Color = color;
    }
}