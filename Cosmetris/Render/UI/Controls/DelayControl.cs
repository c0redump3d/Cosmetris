/*
 * DelayControl.cs is part of Cosmetris.
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
using Cosmetris.Input;
using Cosmetris.Render.UI.Screens.Settings;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cosmetris.Render.UI.Text.Util;

namespace Cosmetris.Render.UI.Controls;

public class DelayControl : Control
{
    private readonly Slider _delaySlider;
    private readonly Slider _reductionRateSlider;
    private readonly Slider _minDelaySlider;

    private readonly Label _delayLabel;
    private readonly Label _reductionRateLabel;
    private readonly Label _minDelayLabel;
    private readonly float _delayLength = 0;
    private readonly float _reductionRateLength  = 0;
    private readonly float _minDelayLength = 0;

    private readonly GameOption<Controller.ButtonDelay> _buttonDelayOption;
    private readonly Font _font;
    private const float VerticalSpacing = 10f;
    private const float LabelSliderSpacing = 5f;

    public DelayControl(GameOption<Controller.ButtonDelay> buttonDelayOption, Vector2 position, string font, int fontSize,
        SettingsScreen settingsScreen, SettingsCategory category, int page)
    {
        _buttonDelayOption = buttonDelayOption;
        _font = FontRenderer.Instance.GetFont(font, fontSize);
        Size = new Vector2(250, 3 * (20 + VerticalSpacing)); // Adjusted width for labels + sliders
        Position = position;

        // Create labels for each slider
        _delayLabel = new Label("Delay (ms):", GetActualPosition(), _font);
        _reductionRateLabel = new Label("Reduction Rate (ms):", GetActualPosition() + new Vector2(0, 25 + VerticalSpacing), _font);
        _minDelayLabel = new Label("Minimum Delay (ms):", GetActualPosition() + new Vector2(0, 50 + 2 * VerticalSpacing), _font);
        _delayLength = _font.MeasureString("Delay (ms):").X;
        _reductionRateLength = _font.MeasureString("Reduction Rate (ms):").X; // Adjusted X for some spacing to the right
        _minDelayLength = _font.MeasureString("Minimum Delay (ms):").X;

        // Add labels to the settings screen
        settingsScreen.AddControlToPage(category, _delayLabel, page);
        settingsScreen.AddControlToPage(category, _reductionRateLabel, page);
        settingsScreen.AddControlToPage(category, _minDelayLabel, page);

        // Adjust slider positions to account for label widths + spacing
        var sliderPositionOffset = new Vector2(_delayLength + LabelSliderSpacing, 0);
        _delayLabel.SetPosition(GetActualPosition() - sliderPositionOffset);
        _reductionRateLabel.SetPosition(GetActualPosition() + new Vector2(0, 25 + VerticalSpacing) -
                                        new Vector2(_reductionRateLength + LabelSliderSpacing, 0));
        _minDelayLabel.SetPosition(GetActualPosition() + new Vector2(0, 50 + 2 * VerticalSpacing) - new Vector2(_minDelayLength + LabelSliderSpacing, 0));

        // Initialize the sliders with appropriate settings
        _delaySlider = new Slider(GetActualPosition() + sliderPositionOffset, new Vector2(200, 20), Slider.SliderType.Rectangle,
            0f, 1000f,
            _buttonDelayOption.Value.Delay, Slider.ValueFormat.Int, _font.GetName(), _font.GetSize()- 4);
        _delaySlider.OnValueChanged += (_, _) =>
        {
            _buttonDelayOption.Value.SetDelay(_delaySlider.Value);
            OnValueChanged?.Invoke(this, EventArgs.Empty);
        };
        settingsScreen.AddControlToPage(category, _delaySlider, page);

        _reductionRateSlider = new Slider(GetActualPosition() + sliderPositionOffset + new Vector2(0, 25 + VerticalSpacing),
            new Vector2(200, 20), Slider.SliderType.Rectangle, 0f, 1000f, _buttonDelayOption.Value.DelayReductionRate,
            Slider.ValueFormat.Int, _font.GetName(), _font.GetSize() - 4);
        _reductionRateSlider.OnValueChanged +=
            (_, _) =>
            {
                _buttonDelayOption.Value.SetDelayReductionRate(_reductionRateSlider.Value);
                OnValueChanged?.Invoke(this, EventArgs.Empty);
            };
        settingsScreen.AddControlToPage(category, _reductionRateSlider, page);

        _minDelaySlider = new Slider(GetActualPosition() + sliderPositionOffset + new Vector2(0, 50 + 2 * VerticalSpacing),
            new Vector2(200, 20), Slider.SliderType.Rectangle, 0f, 1000f, _buttonDelayOption.Value.MinDelay,
            Slider.ValueFormat.Int, _font.GetName(), _font.GetSize()- 4);
        _minDelaySlider.OnValueChanged += (_, _) =>
        {
            _buttonDelayOption.Value.SetMinDelay(_minDelaySlider.Value);
            OnValueChanged?.Invoke(this, EventArgs.Empty);
        };
        settingsScreen.AddControlToPage(category, _minDelaySlider, page);
    }

    public EventHandler OnValueChanged { get; set; }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw the main label for the button
        _font.DrawLabel(_buttonDelayOption.Name, GetActualPosition().X, GetActualPosition().Y - (25 * GetScaleFactor().Y),
            Microsoft.Xna.Framework.Color.White, scale: GetScaleFactor().X); // Adjusted Y for some spacing above
    }
    
    ~DelayControl()
    {
        _delaySlider.Dispose();
        _reductionRateSlider.Dispose();
        _minDelaySlider.Dispose();
        _delayLabel.Dispose();
        _reductionRateLabel.Dispose();
        _minDelayLabel.Dispose();
    }
}