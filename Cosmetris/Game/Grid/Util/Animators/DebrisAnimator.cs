/*
 * DebrisAnimator.cs is part of Cosmetris.
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
using Cosmetris.Render.Particle;
using Cosmetris.Render.Particle.Particles;
using Cosmetris.Util.Numbers;
using Microsoft.Xna.Framework;

namespace Cosmetris.Game.Grid.Util.Animators;

public class DebrisAnimator
{
    private const int CrumbleChecks = 8; // Number of crumble phases
    private const float CrumbleInterval = 0.25f; // Time interval between each crumble check in seconds
    private readonly int _cellSize;
    private readonly int _columns;
    private readonly TetrisGrid _grid;
    private readonly int _lineWidth;
    private readonly int _rows;
    private float _crumbleTime; // Time tracker for the crumble sequence
    private int _currentCheck; // Current crumble phase

    public DebrisAnimator(TetrisGrid grid)
    {
        _grid = grid;
        _cellSize = grid.GetCellSize();
        _columns = grid.TotalColumns();
        _rows = grid.TotalRows();
        _lineWidth = grid.GetLineWidth();
    }

    public void EmitDebrisFromRow(int rowIndex, int lines)
    {
        // Base number of particles per cell, increasing with more lines cleared
        var baseParticles = 2;
        var additionalParticles = lines - 1; // 1 line means no additional particles
        var numberOfParticlesPerCell = baseParticles + additionalParticles;

        float minSpeed = 35 + 5 * lines;
        float maxSpeed = 42 + 5 * lines;

        var grid = _grid.GetCurrentGrid();

        for (var col = 0; col < _columns; col++)
        {
            if (!grid[rowIndex, col].IsOccupied) continue;

            for (var i = 0; i < numberOfParticlesPerCell; i++)
            {
                var emissionOffset = new Vector2(
                    RandomUtil.NextFloat(-_cellSize / 4.0f, _cellSize / 4.0f),
                    RandomUtil.NextFloat(-_cellSize / 4.0f, _cellSize / 4.0f)
                );

                var emissionPoint = _grid.GridToScreenPosition(new Vector2(col, rowIndex - 2)) +
                                    new Vector2(_lineWidth, _lineWidth) +
                                    new Vector2(_cellSize / 2.0f, _cellSize / 2.0f) +
                                    emissionOffset;

                var angle = RandomUtil.NextFloat(0, 2 * (float)Math.PI);
                var speed = RandomUtil.NextFloat(minSpeed, maxSpeed);

                // Slight color variation for each rubble
                var colorVariation = RandomUtil.NextFloat(0.6f, 1.25f);
                var variedColor = new Color(
                    (int)(grid[rowIndex, col].Color.R * colorVariation),
                    (int)(grid[rowIndex, col].Color.G * colorVariation),
                    (int)(grid[rowIndex, col].Color.B * colorVariation));

                var particle = new Rubble(
                    emissionPoint.X,
                    emissionPoint.Y,
                    RandomUtil.NextFloat(0.5f, 1.2f),
                    speed,
                    angle,
                    variedColor
                );

                ParticleManager.Instance.AddParticle(particle);
            }
        }
    }

    public bool CrumbleGrid(GameTime gameTime)
    {
        if (_currentCheck >= CrumbleChecks)
            return false; // Return false if crumble sequence is complete (all checks have been made

        _crumbleTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!(_crumbleTime > CrumbleInterval) || _currentCheck >= CrumbleChecks) return true;

        _grid.GetShakeAnimator().StartShake(); // Start shake animation on first check

        EmitRandomDebris(); // Emit debris from random grid location
        Window.Instance.GetSoundManager().PlaySFX("crush"); // Play crumble sound effect
        _crumbleTime = 0; // Reset timer
        _currentCheck++; // Move to next check

        return true;
    }

    private void EmitRandomDebris()
    {
        var numberOfParticlesPerSpot = RandomUtil.Next(12, 18); // Number of particles per crumble spot

        // Random grid cell for crumble spot
        var randomRow = RandomUtil.Next(0, _rows);
        var randomCol = RandomUtil.Next(0, _columns);

        var grid = _grid.GetCurrentGrid();

        for (var i = 0; i < numberOfParticlesPerSpot; i++)
        {
            var emissionOffset = new Vector2(
                RandomUtil.NextFloat(-_cellSize, _cellSize),
                RandomUtil.NextFloat(-_cellSize, _cellSize)
            );

            var emissionPoint = _grid.GridToScreenPosition(new Vector2(randomCol, randomRow - 2)) +
                                new Vector2(_lineWidth, _lineWidth) +
                                new Vector2(_cellSize / 2.0f, _cellSize / 2.0f) +
                                emissionOffset;

            var angle = RandomUtil.NextFloat(0, 2 * (float)Math.PI);
            var speed = RandomUtil.NextFloat(20, 50); // Vary speed for variety

            var cellColor = grid[randomRow, randomCol].IsOccupied
                ? grid[randomRow, randomCol].Color
                : Color.Gray; // Default to gray if cell is not occupied

            var particle = new Rubble(
                emissionPoint.X,
                emissionPoint.Y,
                RandomUtil.NextFloat(0.5f, 1.5f),
                speed,
                angle,
                cellColor
            );

            ParticleManager.Instance.AddParticle(particle);
        }
    }
}