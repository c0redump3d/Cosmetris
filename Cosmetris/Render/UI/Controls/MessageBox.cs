/*
 * MessageBox.cs is part of Cosmetris.
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
using Cosmetris.Render.UI.Controls.Animation;
using Cosmetris.Render.UI.Text;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Controls;

public enum MessageBoxButton
{
    OK,
    YESNO,
    YESNOCANCEL,
    OKCANCEL
    // ... add more button types as needed
}

public enum MessageBoxButtonType
{
    OK,
    YES,
    NO,

    CANCEL
    // ... add more button types as needed
}

public class MessageBox : Panel
{
    private static readonly Font _font = FontRenderer.Instance.GetFont("orbitron", 48);
    private static readonly Font _mediumFont = FontRenderer.Instance.GetFont("orbitron", 28);

    private static readonly Vector2 FinalSize = new(500, 280);
    private readonly List<Button> _buttons = new();
    private readonly Label _messageLabel;
    private readonly Label _titleLabel;

    public EventHandler<MessageBoxButtonType> OnButtonClick;

    public MessageBox(string title, string message, MessageBoxButton buttonType)
        : base(
            new Vector2(Window.Instance.ScalingManager.DesiredWidth / 2f,
                Window.Instance.ScalingManager.DesiredHeight / 2f), FinalSize)
    {
        IsImportant = true; // Calculate text heights
        Layer = 0.9f;
        
        var lineCounts = message.Split('\n').Length;

        var titleSize = CalculateWrappedHeight(title, _font, FinalSize.X);
        var messageSize = CalculateWrappedHeight(message, _mediumFont, FinalSize.X);
        
        // Calculate button height (assuming fixed height for now, adjust if needed)
        const float buttonHeight = 50; // adjust to your button's height

        // Calculate padding/margin
        const float padding = 15; // adjust as needed
        
        // Calculate total height
        var totalHeight =
            titleSize + messageSize + buttonHeight +
            3 * padding; // 3 paddings: title-to-message, message-to-button, button-to-bottom

        // Adjust the size of the MessageBox
        _finalSize = new Vector2(FinalSize.X, totalHeight);

        //LockAllControls = true;
        // Add title label
        _titleLabel = new Label(title, new Vector2(FinalSize.X / 2f, 15), _font, Microsoft.Xna.Framework.Color.DodgerBlue,
            Label.Align.Center, maxLineWidth: (int)_finalSize.X);
        AddControl(_titleLabel);

        var divider = new Divider(new Vector2((FinalSize.X / 2f), 65), new Vector2(FinalSize.X - 125, 2),
            Microsoft.Xna.Framework.Color.DarkGray * 0.5f);
        
        AddControl(divider);
        
        // Add message label
        _messageLabel = new Label(message, new Vector2(FinalSize.X / 2f, 80), _mediumFont,
            Microsoft.Xna.Framework.Color.Gray, Label.Align.Center, maxLineWidth: (int)_finalSize.X);
        AddControl(_messageLabel);

        AddButtons(buttonType);
    }

    public MessageBox(string title, string[] messages, MessageBoxButton buttonType) : base(
        new Vector2(Window.Instance.ScalingManager.DesiredWidth / 2f,
            Window.Instance.ScalingManager.DesiredHeight / 2f), FinalSize)
    {
        IsImportant = true; // Calculate text heights
        Layer = 0.9f;
        
        var titleSize = CalculateWrappedHeight(title, _font, FinalSize.X);
        var messageSize = 0f;
        foreach (var message in messages)
        {
            messageSize += CalculateWrappedHeight(message, _mediumFont, FinalSize.X);
        }
        
        // Calculate button height (assuming fixed height for now, adjust if needed)
        const float buttonHeight = 50; // adjust to your button's height

        // Calculate padding/margin
        const float padding = 15; // adjust as needed
        
        // Calculate total height
        var totalHeight =
            titleSize + messageSize + buttonHeight +
            3 * padding; // 3 paddings: title-to-message, message-to-button, button-to-bottom

        // Adjust the size of the MessageBox
        _finalSize = new Vector2(FinalSize.X, totalHeight);

        //LockAllControls = true;
        // Add title label
        _titleLabel = new Label(title, new Vector2(FinalSize.X / 2f, 15), _font, Microsoft.Xna.Framework.Color.DodgerBlue,
            Label.Align.Center, maxLineWidth: (int)_finalSize.X);
        AddControl(_titleLabel);

        var divider = new Divider(new Vector2((FinalSize.X / 2f), 65), new Vector2(FinalSize.X - 125, 2),
            Microsoft.Xna.Framework.Color.DarkGray * 0.5f);
        
        AddControl(divider);
        
        // Add message label
        var y = 80f;
        foreach (var message in messages)
        {
            var label = new Label(message, new Vector2(FinalSize.X / 2f, y), _mediumFont,
                Microsoft.Xna.Framework.Color.Gray, Label.Align.Center, maxLineWidth: (int)_finalSize.X);
            AddControl(label);
            y += label.Size.Y;
        }

        AddButtons(buttonType);
    }

    protected override void Initialize()
    {
        SetAnimation(new ScaleAnimation(this, Size, _finalSize, 0.25f));
        SetClosingAnimation(new ScaleClosingAnimation(this, _finalSize, Vector2.Zero, 0.25f));
    }
    
    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Hidden) return;

        var scaledPos = GetActualPosition();
        RenderUtil.DrawBorderRoundRect(scaledPos.X, scaledPos.Y, Size.X, Size.Y, BorderRadius, MessageBoxBGColor,
            MessageBoxBorderColor, 4.75f, Layer);
        
        RenderChild(spriteBatch, gameTime);
    }

    private void AddButtons(MessageBoxButton type)
    {
         Window.Instance.GetSoundManager().PlaySFX("popup2");

        // Add buttons based on type
        switch (type)
        {
            case MessageBoxButton.OK:
                var okButton = new Button("Ok", FinalSize.X / 2f, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.OK);
                    Close();
                }, _mediumFont);
                AddControl(okButton);
                _buttons.Add(okButton);
                break;

            case MessageBoxButton.YESNO:
                var yesButton = new Button("Yes", FinalSize.X / 4, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.YES);
                    Close();
                }, _mediumFont);
                var noButton = new Button("No", 3 * FinalSize.X / 4, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.NO);
                    Close();
                }, _mediumFont);
                AddControl(yesButton);
                AddControl(noButton);
                _buttons.Add(yesButton);
                _buttons.Add(noButton);
                break;

            case MessageBoxButton.OKCANCEL:
                var okButton2 = new Button("Ok", FinalSize.X / 3, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.OK);
                    Close();
                }, _mediumFont);
                var cancelButton = new Button("Cancel", 2 * FinalSize.X / 3, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.CANCEL);
                    Close();
                }, _mediumFont);
                AddControl(okButton2);
                AddControl(cancelButton);
                _buttons.Add(okButton2);
                _buttons.Add(cancelButton);
                break;
            case MessageBoxButton.YESNOCANCEL:
                var yesButton2 = new Button("Yes", FinalSize.X / 4, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.YES);
                    Close();
                }, _mediumFont);
                var noButton2 = new Button("No", FinalSize.X / 2, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.NO);
                    Close();
                }, _mediumFont);
                var cancelButton2 = new Button("Cancel", 3 * FinalSize.X / 4, FinalSize.Y - 50, (s, o) =>
                {
                    OnButtonClick?.Invoke(this, MessageBoxButtonType.CANCEL);
                    Close();
                }, _mediumFont);
                AddControl(yesButton2);
                AddControl(noButton2);
                AddControl(cancelButton2);
                _buttons.Add(yesButton2);
                _buttons.Add(noButton2);
                _buttons.Add(cancelButton2);
                break;

        }
    }

    private float CalculateWrappedHeight(string text, Font font, float maxWidth)
    {
        var words = text.Split(' ');
        float currentLineWidth = 0;
        var lineHeight = font.MeasureString("A").Y; // Assumes height is consistent for all characters
        var totalHeight = lineHeight; // Start with one line

        foreach (var word in words)
        {
            var wordWidth = font.MeasureString(word).X;
            if (currentLineWidth + wordWidth <= maxWidth)
            {
                currentLineWidth += wordWidth;
            }
            else
            {
                totalHeight += lineHeight;
                currentLineWidth = wordWidth;
            }
        }

        return totalHeight;
    }

    protected override void OnResize()
    {
        Position = new Vector2(Window.Instance.ScalingManager.DesiredWidth / 2f,
            Window.Instance.ScalingManager.DesiredHeight / 2f);
        
        Position -= Size / 2f;
        base.OnResize();
    }

    private void Close()
    {
        StartClosing();
    }
}