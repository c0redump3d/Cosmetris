/*
 * BagRandomizer.cs is part of Cosmetris.
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

namespace Cosmetris.Game.Objects.Cosmonoes.Util;

/// <summary>
///     7-bag randomizer for Cosmonoes.
/// </summary>
public class BagRandomizer
{
    private readonly Cosmono _currentCosmono;

    private readonly Random _random = new();
    private List<CosmonoShape> _currentBag;
    private List<CosmonoShape> _nextBag;
    private readonly CosmonoShape cosmonoes;

    public BagRandomizer(Cosmono cosmono)
    {
        cosmonoes = new CosmonoShape();
        _currentBag = GenerateAndShuffleBag();
        _nextBag = GenerateAndShuffleBag();
    }

    private List<CosmonoShape> GetAllShapes()
    {
        return _currentCosmono.GetCosmonoShape().AllShapes();
    }

    /// <summary>
    ///     Returns the next shape in the bag and removes it.
    /// </summary>
    /// <returns> The next shape in the bag. </returns>
    public CosmonoShape GetNextShape()
    {
        if (_currentBag.Count == 0)
        {
            _currentBag = _nextBag;
            _nextBag = GenerateAndShuffleBag();
        }

        var nextShape = _currentBag[0];
        _currentBag.RemoveAt(0);
        return nextShape;
    }

    /// <summary>
    ///     Returns the next shape in the bag without removing it.
    /// </summary>
    /// <returns> The next shape in the bag. </returns>
    public CosmonoShape PeekNextShape()
    {
        return _currentBag.Count > 0 ? _currentBag[0] : _nextBag[0];
    }

    /// <summary>
    ///     Returns the remaining shapes in the bag without removing them.
    /// </summary>
    /// <returns> The remaining shapes in the bag. </returns>
    public IEnumerable<CosmonoShape> PeekRemainingShapes()
    {
        return new List<CosmonoShape>(_currentBag.Skip(1)); // Skip the next one.
    }

    /// <summary>
    ///     Returns the next bag of shapes without removing them.
    /// </summary>
    /// <returns> The next bag of shapes. </returns>
    public IEnumerable<CosmonoShape> PeekNextBag()
    {
        return new List<CosmonoShape>(_nextBag);
    }

    public int CurrentBagCount()
    {
        return _currentBag.Count;
    }

    // Shuffles the static list of CosmonoShapes
    private List<CosmonoShape> GenerateAndShuffleBag()
    {
        var bag = new List<CosmonoShape>(cosmonoes.Shapes); // Create a copy of the static list

        var n = bag.Count;
        while (n > 1)
        {
            n--;
            var k = _random.Next(n + 1);
            (bag[k], bag[n]) = (bag[n], bag[k]);
        }

        return bag;
    }
}