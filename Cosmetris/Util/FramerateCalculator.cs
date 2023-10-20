/*
 * FramerateCalculator.cs is part of Cosmetris.
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
using System.Text;
using Cosmetris.Render;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Text;
using Microsoft.Xna.Framework;

namespace Cosmetris.Util;

public class FramerateCalculator
{
    private const int MaxFramesForAverage = 100; // Take average over the last 100 frames

    private readonly Queue<double> _recentFrameTimes = new(); // For moving average

    private double _recentTotalFrameTime; // Change this to double for accumulating recent frame times
    private int _totalFrames;

    public double FrameRate { get; private set; }

    public double FrameTime { get; private set; }

    public double AverageFrameRate { get; private set; }

    public double AverageFrameTime { get; private set; }

    public void Update(GameTime gameTime)
    {
        FrameTime = gameTime.ElapsedGameTime.TotalMilliseconds;
        FrameRate = FrameTime > 0 ? 1000.0 / FrameTime : 0;

        _totalFrames++;

        // Moving average calculation
        _recentFrameTimes.Enqueue(FrameTime);
        _recentTotalFrameTime += FrameTime;

        if (_recentFrameTimes.Count > MaxFramesForAverage) _recentTotalFrameTime -= _recentFrameTimes.Dequeue();

        AverageFrameTime = _recentTotalFrameTime / _recentFrameTimes.Count;
        AverageFrameRate = 1000.0 / AverageFrameTime;
    }

    public void Reset()
    {
        FrameRate = 0;
        FrameTime = 0;
        AverageFrameRate = 0;
        AverageFrameTime = 0;
        _recentTotalFrameTime = 0;
        _totalFrames = 0;
        _recentFrameTimes.Clear();
    }
}

public class FramerateRenderer
{
    private readonly FramerateCalculator _calculator;
    private readonly Font _font;
    private readonly UIScalingManager _scalingManager = Window.Instance.ScalingManager;

    private readonly List<StringBuilder> _lines = new();

    public FramerateRenderer(FramerateCalculator calculator, Font font)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _font = font ?? throw new ArgumentNullException(nameof(font));

        _lines.Add(new StringBuilder());
        _lines.Add(new StringBuilder());
        _lines.Add(new StringBuilder());
        _lines.Add(new StringBuilder());
    }

    public void UpdateLines()
    {
        _lines[0].Clear();
        _lines[0].Append("FPS: ");
        _lines[0].Append(_calculator.FrameRate.ToString("0.00"));

        _lines[1].Clear();
        _lines[1].Append("Frame Time: ");
        _lines[1].Append(_calculator.FrameTime.ToString("0.00"));
        _lines[1].Append("ms");

        _lines[2].Clear();
        _lines[2].Append("Avg FPS: ");
        _lines[2].Append(_calculator.AverageFrameRate.ToString("0.00"));

        _lines[3].Clear();
        _lines[3].Append("Avg Frame Time: ");
        _lines[3].Append(_calculator.AverageFrameTime.ToString("0.00"));
        _lines[3].Append("ms");
    }

    public void Draw()
    {
        float width = 0;
        float height = 0;

        // Calculate the width and height of the text
        foreach (var line in _lines)
        {
            var lineWidth = _font.GetFontBase().MeasureString(line).X;
            if (lineWidth > width) width = lineWidth;

            height += _font.GetHeight();
        }

        // Render a bordered rectangle in the top right corner
        var x = _scalingManager.DesiredWidth - width;
        var w = width;
        var h = height + 5;
        RenderUtil.DrawBorderRoundRect(x, 0, w, h, 5, Color.Black * 0.5f, Color.White);

        // Render the text
        var y = 2.5f;
        foreach (var line in _lines)
        {
            var x2 = _scalingManager.DesiredWidth - width;
            _font.DrawLabel(line, x2, y, Color.White);
            y += _font.GetHeight();
        }
    }
}