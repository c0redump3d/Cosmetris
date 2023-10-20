/*
 * ScreenRenderer.cs is part of Cosmetris.
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
using Cosmetris.Input;
using Cosmetris.Render.UI;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Text;
using Cosmetris.Settings;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Cosmetris.Render.Renderers;

public class ScreenRenderer : Renderer
{
    private Screen _currentScreen;
    private Screen _nextScreen;
    private List<Control> _globalControls;
    
    private FramerateRenderer _framerate;
    private FramerateCalculator _framerateCalc;
    private Font _font;
    
    private Control _lastAnimatingControl;
    private bool _justOpened = false;
    private bool _justClosed = false;
    
    private bool _debug = false;

    public override void Initialize(GraphicsDevice graphicsDevice)
    {
        base.Initialize(graphicsDevice);
        
        _framerateCalc = new FramerateCalculator();
        _font = FontRenderer.Instance.GetFont("debug", 18);
        _framerate = new FramerateRenderer(_framerateCalc, _font);

        Controller.Instance.DebugKeyReleased += OnInstanceRawKeyboardKeyReleased;
    }

    private void OnInstanceRawKeyboardKeyReleased(object sender, BindableDebugKeys key)
    {
        if (key.Key == Keys.F3)
        {
            _debug = !_debug;

            if (_debug && _currentScreen.DebugConsole == null)
            {
                DebugConsole debugConsole = new();
                _currentScreen.DebugConsole = debugConsole;
                _currentScreen.AddControl(debugConsole);
        
                debugConsole.AddMessage("Welcome to Cosmetris!", null);
            }else if(!_debug && _currentScreen.DebugConsole != null){
                _currentScreen.RemoveControl(_currentScreen.DebugConsole);
                _currentScreen.DebugConsole = null;
            }
            
        }
    }

    public void SetScreen(Screen screen)
    {
        if (_currentScreen != null)
        {
            _nextScreen = screen;
            _nextScreen.OnInit();
            _nextScreen.DebugConsole = _currentScreen.DebugConsole;

            // Start the closing animations for the controls of the current screen
            foreach (var control in _currentScreen.GetControls()) 
                if(!control.IsGlobal)
                    control.StartClosing();
                else
                {
                   _nextScreen.AddControl(control);
                }
        }
        else
        {
            // If there's no current screen, set the given screen immediately
            _currentScreen = screen;
        }
    }

    public Screen GetScreen()
    {
        return _currentScreen;
    }

    public Screen GetNextScreen()
    {
        return _nextScreen;
    }

    public override void Draw(GameTime gameTime)
    {
        _currentScreen?.Draw(_spriteBatch, gameTime);


        if (_debug)
        {
            // Draw Framerate
            _framerate.Draw();
        }

        base.Draw(gameTime);
    }

    public override void DrawImportant(GameTime gameTime)
    {
        _currentScreen?.DrawImportant(_spriteBatch, gameTime);
        base.DrawImportant(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        // Check if we have a next screen and the current screen's closing animation is finished
        if (_nextScreen != null && !AnyControlAnimating(_currentScreen.GetControls()))
        {
            // Transition to the next screen
            _currentScreen.OnClose();
            _currentScreen = _nextScreen;
            _lastAnimatingControl = null;
            _nextScreen = null;
        }
        _currentScreen?.Update(gameTime);

        BackgroundRenderer.Instance.Update(gameTime);
        base.Update(gameTime);
        
        if(_debug){
            _framerateCalc.Update(gameTime); 
            _framerate.UpdateLines();
        }
    }

    public override void OnResize()
    {
        _currentScreen?.OnResize();
        base.OnResize();
    }

    public bool CurrentlyAnimating()
    {
        return _currentScreen != null && AnyControlAnimating(_currentScreen.GetControls());
    }

    private bool AnyControlAnimating(IEnumerable<Control> controls)
    {
        if(_lastAnimatingControl != null && !_lastAnimatingControl.FinishedClosing() && !_lastAnimatingControl.FinishedOpening())
            return true;

        if (_lastAnimatingControl == null)
        {
            foreach (var control in controls)
                if (!control.FinishedClosing() || !control.FinishedOpening())
                {
                    _lastAnimatingControl = control;
                    _justOpened = false;
                    return true;
                }
        }
        else
        {
            if (!_lastAnimatingControl.FinishedClosing() || !_lastAnimatingControl.FinishedOpening())
                return true;

            if (_lastAnimatingControl.FinishedOpening() && !_justOpened)
            {
                _justOpened = true;
                _lastAnimatingControl = null;
            }
            
        }

        return false;
    }
    
    public bool IsDebugEnabled()
    {
        return _debug;
    }
    
    ~ScreenRenderer()
    {
        Controller.Instance.DebugKeyReleased -= OnInstanceRawKeyboardKeyReleased;
    }
}