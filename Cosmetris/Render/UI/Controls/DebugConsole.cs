/*
 * DebugConsole.cs is part of Cosmetris.
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
using System.Runtime.CompilerServices;
using Cosmetris.Render.UI.Text;
using Cosmetris.Render.UI.Text.Util;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls;

public class DebugConsole : Control
{
    private readonly List<MessageLine> consoleMessageList;
    private readonly Font debugFont = FontRenderer.Instance.GetFont("debug", 18);
    private int _total = 0;
    private float _maxWidth = 0;
    private string _longestLine = ""; 
    private Rectangle _consoleRect;
    
    private readonly Vector2 _consoleSize = new(600, 12);

    public DebugConsole()
    {
        consoleMessageList = new List<MessageLine>();
        Size = _consoleSize;
        Position = Vector2.Zero;
        IsGlobal = true;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var lines = 40; // how many messages can be on screen
        var spacing = 0;
        var pos = GetActualPosition();
        //spriteBatch.DrawBorderedRect(rec, new Color(30, 28, 28) * 0.8f,
        //new Color(43, 149, 223));
        RenderUtil.DrawBorderRect(pos.X, pos.Y, ScalingManager.GetScaledX(_consoleSize.X), Size.Y, PanelBGColor, PanelBorderColor);
        //spriteBatch.DrawCenteredString(Fonts.ConsoleFont, $"Tetris {Globals.Version} Debug Console",
        //new Vector2(Position.X + (Size.X/2f), (Position.Y+Size.Y) - total), Color.White);
        
        for (var msg = 0; msg < consoleMessageList.Count && msg < lines; msg++)
        {
            spacing = msg * (int)debugFont.GetSize();
            var s1 = consoleMessageList[msg].Message;
            
            debugFont.DrawFormattedString(s1, new Vector2(pos.X + 8, ((pos.Y + 15) + spacing)), Microsoft.Xna.Framework.Color.White);
        }

        debugFont.DrawLabel($"Cosmetris Debug Console", new Vector2((pos.X + _consoleSize.X/2f), pos.Y),
            Microsoft.Xna.Framework.Color.Gray, TextHorizontalAlignment.Center);
        base.Draw(spriteBatch, gameTime);
    }

    private void AddConsoleMessage(string s)
    {
        int i;
        
        var spacing = _consoleSize.X - 16;
        for (;
             debugFont.MeasureString(s).X > spacing;
             s = s.Substring(i)) //split any text that exceeds out spacing
        {
            for (i = 1; i < s.Length && debugFont.MeasureString(s.Substring(0, i + 1)).X <= spacing; i++)
            {
            }

            AddConsoleMessage(s.Substring(0, i));
        }

        var oldWidth = _maxWidth;
        _maxWidth = Math.Max(_maxWidth, debugFont.MeasureString(s).X);
        
        if (Math.Abs(oldWidth - _maxWidth) > 0.1f)
            _longestLine = s;
        
        consoleMessageList.Add(new MessageLine(s));
        for (; consoleMessageList.Count > 25; consoleMessageList.RemoveAt(0))
        {
        } // any lines that exceed 25 will be removed

        ResizeConsole();
    }

    private void ResizeConsole()
    {
        var pos = GetActualPosition();
        _total = consoleMessageList.Count * (int)debugFont.GetSize();
        _consoleRect = new Rectangle((int) pos.X, (int) (pos.Y), ScalingManager.GetScaledX(_consoleSize.X), ScalingManager.GetScaledY(_total + 20));
        Size = new Vector2(_consoleRect.Width, _consoleRect.Height);
        _maxWidth = 0;
        _maxWidth = Math.Max(_maxWidth, debugFont.MeasureString(_longestLine).X);
    }

    protected override void OnResize()
    {
        ResizeConsole();
        base.OnResize();
    }

    public void AddMessage(string message, [CallerMemberName] string caller = "", MessageType type = MessageType.Info)
    {
        var dateTime = DateTime.Now;
        var time = dateTime.ToString("HH:mm:ss");
        
        string color = type switch
        {
            MessageType.Info => "{gray}[INFO]",
            MessageType.Warning => "{yellow}[WARNING]",
            MessageType.Error => "{red}[ERROR]",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        string white = "{white}";
        
        AddConsoleMessage(caller == null ? $@"{color}[{time}]:{white}{message}": $@"{color}[{time}] {caller}:{white}{message}");
    }
}

public class MessageLine
{
    public string Message;
    public int UpdateCounter;

    public MessageLine(string s)
    {
        Message = s;
        UpdateCounter = 0;
    }
}

public enum MessageType
{
    Info,
    Warning,
    Error
}