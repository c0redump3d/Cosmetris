/*
 * TextRenderOperationPool.cs is part of Cosmetris.
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

namespace Cosmetris.Render.UI.Text.Util;

public class TextRenderOperationPool
{
    private readonly int _maxCount;
    private readonly Stack<TextRenderOperation> _pool;

    public TextRenderOperationPool(int maxCount)
    {
        _pool = new Stack<TextRenderOperation>(maxCount);
        _maxCount = maxCount;

        for (var i = 0; i < maxCount; i++) _pool.Push(new TextRenderOperation());
    }

    public TextRenderOperation? Get()
    {
        if (_pool.Count > 0)
            return _pool.Pop();
        // If the pool is empty, we either return null, or create a new instance
        // depending on the behavior you want when the pool is exhausted.
        return null;
    }

    public void Return(TextRenderOperation item)
    {
        if (_pool.Count < _maxCount) _pool.Push(item);
        // If the pool is full, we just let the item be collected by the GC
    }
}