/*
 * SettingsScreen.cs is part of Cosmetris.
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
using Cosmetris.Game;
using Cosmetris.Input;
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Screens.Settings.Categories;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.UI.Screens.Settings;

public class SettingsScreen : Screen
{
    private const int ItemsPerSide = 5; // Number of items per half of the page.
    private const int ItemsPerPage = ItemsPerSide * 2; // Total items per page.
    private const int PanelWidth = 1280;
    private const int PanelHeight = 720;
    private readonly Font _defaultFont = FontRenderer.Instance.GetFont("orbitron", 24);
    private Button _backButton;

    private struct OldOption
    {
        public string Name;
        public string Category;
        public object Value;
    }
    
    private List<OldOption> _oldOptions;
    
    private List<SettingsCategory> _controlsByCategory;
    private SettingsCategory _currentCategory;
    private string _lastButtonName;
    private BindableControllerKey _lastControllerButton;

    private BindableKeyboardKey _lastKeyboardKey;
    private Button _nextButton;

    private bool _optionsChanged = false;
    private Label _pageLabel;

    private Panel _panel;
    private Button _prevButton;
    private Button _waitingForButton;

    private bool _waitingForControllerInput;
    private bool _waitingForKeyboardInput;
    private float _waitTimer;
    private int TotalPages;
    private int CurrentPage;

    private Dictionary<Type, ICategoryHandler> _categoryHandlers;

    public SettingsScreen()
    {
        
        // initialize the handlers
        _categoryHandlers = new Dictionary<Type, ICategoryHandler>()
        {
            {typeof(GameOption<Microsoft.Xna.Framework.Color>), new ColorCategoryHandler()},
            {typeof(GameOption<Controller.ButtonDelay>), new ButtonDelayCategoryHandler()},
            {typeof(GameOption<bool>), new BoolCategoryHandler()},
            {typeof(GameOption<float>), new FloatCategoryHandler()},
            {typeof(GameOption<Controller.ControllerButton>), new ButtonCategoryHandler()}
        };
        
        foreach (var handler in _categoryHandlers.Values)
        {
            handler.Initialize();
        }
        
        CreateControls(); // Create controls once

        LayoutControls += Relayout; // Assign the layout method to the event
        TotalPages = 0;
        CurrentPage = 0;

        Relayout();

        AddControl(_panel);

        Cosmetris.UpdateGameState(GameState.Settings);
        TotalPages = _controlsByCategory.Sum(c => c.TotalPages+1);
        CheckEnabledButtons(CurrentPage);
        _oldOptions = new List<OldOption>();

        foreach (var categories in GameSettings.Instance.GetCategories())
        {
            int index = 0;
            foreach (var option in categories.CategoryOptions)
            {
                var oldOption = new OldOption()
                {
                    Name = option.Name,
                    Category = categories.CategoryName,
                    Value = option.GetValue()
                };
                if(!_oldOptions.Contains(oldOption)) _oldOptions.Add(oldOption);
            }
        }
    }

    private void CreateControls()
    {
        _controlsByCategory = new List<SettingsCategory>();
        _panel = new Panel(Vector2.Zero, new Vector2(PanelWidth, PanelHeight)); // Position will be set in Relayout

        // Initialize all controls (buttons, labels, sliders, etc.)
        // Don't set their positions here; only create them.
        _backButton = new Button("Back", 0, 0,
            (sender, args) =>
            {
                
                _optionsChanged = _categoryHandlers.Values.Any(handler => handler.OptionWasChanged);
                
                if (_optionsChanged)
                {
                    
                    var messageBox = new MessageBox("Unsaved Changes",
                        "You have unsaved changes.\n\nWould you like to apply these changes?", MessageBoxButton.YESNOCANCEL);
                    messageBox.OnButtonClick += (o, button) =>
                    {
                        switch (button)
                        {
                            case MessageBoxButtonType.YES: 
                                Close();
                                break;
                            case MessageBoxButtonType.NO:
                                foreach (var option in _oldOptions)
                                {
                                    GameSettings.Instance.SetValue(option.Category, option.Name, option.Value);
                                }
                                Close();
                                break;
                            case MessageBoxButtonType.CANCEL:
                                break;
                        }
                    };
                    AddControl(messageBox);
                }
                else
                {
                    Close();
                }
            },
            _defaultFont);
        _prevButton = new Button("Previous", 0, 0, (sender, args) =>
        {
            if (!_currentCategory.ChangePage(ref _panel, false))
            {
                // If we can't go back a page, go to the previous category
                var index = _controlsByCategory.IndexOf(_currentCategory);
                if (index > 0)
                {
                    _currentCategory = _controlsByCategory[index - 1];

                    _currentCategory.Layout(ref _panel);
                }
            }
            CurrentPage--;
            _pageLabel.SetText(_currentCategory.CategoryName);
            CheckEnabledButtons(CurrentPage);
        }, _defaultFont);
        _nextButton = new Button("Next", 0, 0, (sender, args) =>
        {
            if (!_currentCategory.ChangePage(ref _panel, true))
            {
                // If we can't go forward a page, go to the next category
                var index = _controlsByCategory.IndexOf(_currentCategory);
                if (index < _controlsByCategory.Count - 1)
                {
                    _currentCategory = _controlsByCategory[index + 1];

                    _currentCategory.Layout(ref _panel);
                }
            }
            CurrentPage++;
            _pageLabel.SetText(_currentCategory.CategoryName);
            CheckEnabledButtons(CurrentPage);
        }, _defaultFont);
        _panel.AddControl(_backButton);
        _panel.AddControl(_prevButton);
        _panel.AddControl(_nextButton);

        UpdateControls(); // This method creates controls based on game settings and adds them to the dictionary

        _pageLabel = new Label(_currentCategory.CategoryName, new Vector2(PanelWidth / 2f, 25), _defaultFont,
            Microsoft.Xna.Framework.Color.White, Label.Align.Center);

        _panel.AddControl(_pageLabel);

        CheckEnabledButtons(_controlsByCategory.IndexOf(_currentCategory));
    }
    
    private void Close()
    {
        GameSettings.Instance.Save();
        
        Window.Instance.GetSoundManager().UpdateVolumes();

        // See if we enabled fullscreen or not
        var fullscreen = GameSettings.Instance.GetValue<bool>("Graphics", "Fullscreen");
        Window.Instance.SetFullscreen(fullscreen);
                
        var vsync = GameSettings.Instance.GetValue<bool>("Graphics", "VSync");
        Window.Instance.SetVSync(vsync);
        
        // Get shader colors
        var galColOne = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 1");
        var galColTwo = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 2");
        var galColThree = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Galaxy Color 3");
        var nebulaColor = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Nebula Color");
        var cloudColor = GameSettings.Instance.GetValue<Microsoft.Xna.Framework.Color>("Background Options", "Cloud Color");
        
        // Set shader colors
        EffectsManager.Instance.GetEffect("galaxy").Parameters["GalaxyColorOne"].SetValue(galColOne.ToVector3());
        EffectsManager.Instance.GetEffect("galaxy").Parameters["GalaxyColorTwo"].SetValue(galColTwo.ToVector3());
        EffectsManager.Instance.GetEffect("galaxy").Parameters["GalaxyColorThree"].SetValue(galColThree.ToVector3());
        EffectsManager.Instance.GetEffect("galaxy").Parameters["NebulaColor"].SetValue(nebulaColor.ToVector3());
        EffectsManager.Instance.GetEffect("galaxy").Parameters["CloudColor"].SetValue(cloudColor.ToVector3());
        
        // Re-update the controller bindings
        Controller.Instance.LoadSettings();

        Window.Instance.ScreenRenderer().SetScreen(new MainMenuScreen());
    }

    private void CheckEnabledButtons(int index)
    {
        _nextButton.Enabled = index < TotalPages-1;

        _prevButton.Enabled = index > 0;
    }

    private void Relayout()
    {
        // Calculate positions and set them
        var center = new Vector2(ScalingManager.DesiredWidth / 2.0f, ScalingManager.DesiredHeight / 2.0f);
        _panel.SetPosition(center);
        
        // call resize for color picker
        foreach (var category in _controlsByCategory)
        {
            foreach (var page in category.Pages)
            {
                foreach (var control in page.CategoryOptions)
                {
                    if (control is ColorPicker colorPicker)
                    {
                        colorPicker.UpdateSize();
                    }
                }
            }
        }

        // Position buttons and other controls (like you did in the original code)
        const float middlePos = PanelWidth / 2.0f;
        _backButton.SetPosition((int)middlePos, PanelHeight + 100);

        var leftPos = middlePos - _prevButton.Size.X / 2 - 25;
        _prevButton.SetPosition((int)leftPos, PanelHeight + 50);

        var rightPos = middlePos + _nextButton.Size.X / 2 + 25;
        _nextButton.SetPosition((int)rightPos, PanelHeight + 50);
    }

    private void UpdateControls()
    {
        var currentPage = 0;
        var controlIndex = 0;

        // Calculate total height of controls for centering
        const int totalControlsHeight = ItemsPerPage * 65;
        const int spaceBetweenControls = (PanelHeight + totalControlsHeight) / ItemsPerPage;
        var startYOffset = 50;

        // Define x offsets for each side of the panel
        const int leftSideXOffset = PanelWidth / 4; // Center of the left half
        const int rightSideXOffset = PanelWidth - PanelWidth / 4; // Center of the right half

        foreach (var category in GameSettings.Instance.GetCategories())
        {
            const int categoryHeight = 0; // Track the height of the current category

            dynamic cat = category;

            // Create a new category page
            if (_currentCategory == null)
            {
                _currentCategory = new SettingsCategory(category.CategoryName)
                {
                    TotalPages = cat.CategoryOptions.Count / ItemsPerPage
                };
                _controlsByCategory.Add(_currentCategory);
            }

            // if control index of 0, remove the category
            if (_currentCategory.CategoryName != category.CategoryName)
            {
                // Make sure we dont already have a category with this name
                _currentCategory = _controlsByCategory.FirstOrDefault(c => c.CategoryName == category.CategoryName);

                if (_currentCategory == null)
                {
                    _currentCategory = new SettingsCategory(category.CategoryName)
                    {
                        TotalPages = cat.CategoryOptions.Count / ItemsPerPage
                    };
                    _controlsByCategory.Add(_currentCategory);
                }

                currentPage = 0;
                controlIndex = 0;
                startYOffset = 50;
            }

            float delayHeight = 0;

            foreach (var option in cat.CategoryOptions)
            {
                
                var yOffset = startYOffset + controlIndex % ItemsPerSide * spaceBetweenControls;

                // Decide x based on which side of the panel the control should appear
                var xOffset = controlIndex < ItemsPerSide ? leftSideXOffset : rightSideXOffset;
                
                var handlerType = option.GetType();
            
                if (_categoryHandlers.ContainsKey(handlerType))
                {
                    var handler = _categoryHandlers[handlerType];
                    controlIndex = UpdateControlIndex(controlIndex, handler.Size, ref currentPage, ref controlIndex, ref delayHeight,
                        ref startYOffset, ref xOffset, ref yOffset, spaceBetweenControls, leftSideXOffset, rightSideXOffset);
                    handler.Handle(this, option, xOffset, yOffset, currentPage, _currentCategory);
                } 
            }

            // Find if any controls exist at all, if not remove the category
            if (_currentCategory.ActivePage.CategoryOptions.Count == 0) _controlsByCategory.Remove(_currentCategory);

            startYOffset += categoryHeight;
        }

        // set all category pages to 0
        foreach (var category in _controlsByCategory) category.SetCurrentPage(0);

        // Sort the categories by name (alphabetical)
        _controlsByCategory.Sort((x, y) => string.Compare(x.CategoryName, y.CategoryName, StringComparison.Ordinal));


        _currentCategory = _controlsByCategory.FirstOrDefault();
        _currentCategory.Layout(ref _panel);
    }

    private int UpdateControlIndex(int currentIndex, int size, ref int currentPage, ref int controlIndex,
        ref float delayHeight, ref int startYOffset, ref int xOffset, ref int yOffset, int spaceBetweenControls, int leftSideXOffset, int rightSideXOffset)
    {
        currentIndex += size;
        
        if (currentIndex-1 >= ItemsPerPage)
        {
            currentPage++;
            currentIndex = size;
            controlIndex = 0;
            delayHeight = 0;
            startYOffset = 50;
             yOffset = startYOffset + 10 % ItemsPerSide * spaceBetweenControls;
            xOffset = leftSideXOffset;
            _currentCategory.SetCurrentPage(currentPage);
            _currentCategory.TotalPages = currentPage;
        }
        
        return currentIndex;
    }

    public void DisableControls()
    {
        foreach (var control in _controlsByCategory.SelectMany(category => category.ActivePage.CategoryOptions))
            if (control is Button button)
            {
                if (button != _waitingForButton)
                    button.Enabled = false;
            }
            else
            {
                control.Enabled = false;
            }
    }

    public void EnableControls()
    {
        // Renable all buttons
        foreach (var control in _controlsByCategory.SelectMany(category => category.ActivePage.CategoryOptions))
            control.Enabled = true;
    }

    public void AddControlToPage(SettingsCategory category, Control control, int page)
    {
        control.SetParent(_panel);
        category.AddControlToPage(control, page);
    }
    
    public void RemoveControlFromPage(SettingsCategory category, Control control, int page)
    {
        category.RemoveControlFromPage(control, page);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
    }

    public override void Update(GameTime gameTime)
    {

        base.Update(gameTime);
    }

    public override void OnResize()
    {
        Relayout();
        base.OnResize();
    }

    public override void OnClose()
    {
        foreach (var handler in _categoryHandlers.Values)
        {
            handler.Dispose();
        }
        base.OnClose();
    }
}