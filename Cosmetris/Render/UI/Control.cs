/*
 * Control.cs is part of Cosmetris.
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
using Cosmetris.Render.Managers;
using Cosmetris.Render.UI.Color;
using Cosmetris.Render.UI.Controls;
using Cosmetris.Render.UI.Controls.Animation;
using Cosmetris.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Cosmetris.Render.UI;

public class Control : ControlColor
{
    private static readonly Stack<Control> HoverStack = new();
    protected readonly UIScalingManager ScalingManager = Window.Instance.ScalingManager;

    // Events
    protected EventHandler<Vector2> BeginHover;
    protected EventHandler<Vector2> Hovering;
    protected EventHandler<Vector2> HoverRelease;
    public EventHandler<Vector2> OnClick;
    public EventHandler OnClose;
    
    protected ColorCache ColorCache { get; set; } = new();

    /// <summary>
    ///  The ID of the control, based on the current screen.
    /// </summary>
    public int ID { get; }

    /// <summary>
    ///  The position of the control.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    ///  The position offset of the control. This is used for child controls.
    ///  It is always set to the parents position.
    /// </summary>
    public Vector2 PositionOffset { get; set; }

    /// <summary>
    ///  The size of the control.
    /// </summary>
    public Vector2 Size { get; set; }
    
    /// <summary>
    ///  The tag of the control.
    ///  Can be used to easily find and modify a certain control or group of controls later.
    /// </summary>
    public string Tag { get; set; } = "";

    /// <summary>
    ///  The layer of the control.
    /// </summary>
    public float Layer { get; set; } = 0;

    public float HoverLerpAmount { get; set; }
    public float ClickLerpAmount { get; set; } = 0.0f;
    private bool Clicked { get; set; }
    
    /// <summary>
    ///  Returns whether or not a control should be removed when the screen is changed.
    /// </summary>
    public bool IsGlobal { get; set; } = false;
    
    /// <summary>
    ///  When true, on the next Update call, the control will be removed from the screen.
    /// </summary>
    public bool IsMarkedForDeletion { get; set; } = false;

    /// <summary>
    ///  Returns whether or not the control is currently closing. NOTE: A closing animation must be set for this to work.
    /// </summary>
    public bool IsClosing => _closingAnimation.IsClosing;

    /// <summary>
    ///  The text of the control.
    /// </summary>
    public string Text { get; set; } = "";
    
    /// <summary>
    ///  Returns whether or not the control is actively being hovered.
    /// </summary>
    protected bool Hover { get; private set; }
    
    /// <summary>
    ///  Returns whether or not the control can be hovered.
    /// </summary>
    public bool CanHover { get; set; } = true;
    
    /// <summary>
    ///  Returns whether or not the control is currently interactable.
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    ///  Returns the parent of the control if it has one.
    /// </summary>
    public Control Parent { get; set; }
    
    /// <summary>
    ///  Returns the child controls of the control.
    /// </summary>
    public readonly List<Control> ChildControls;

    /// <summary>
    ///  Returns whether or not the control is currently animating (opening or closing).
    /// </summary>
    public bool CurrentlyAnimating { get; set; }
    
    /// <summary>
    ///  Returns whether or not a control should be rendered later.
    /// </summary>
    /// <remarks>
    ///  This is used for controls like the MessageBox, so it appears over all other renderers.
    /// </remarks>
    public bool IsImportant { get; set; } = false;
    
    /// <summary>
    ///  Returns whether or not the control is currently hidden.
    /// </summary>
    public bool Hidden { get; set; } = false;
    
    private readonly float _creationTime;
    private readonly float _fadeTime;
    private bool _alreadyCreated;
    private IControlAnimation _animation;
    private IControlIClosingAnimation _closingAnimation;

    // Constructor
    protected Control()
    {
        var currentScreen = Window.Instance.ScreenRenderer().GetScreen();

        ID = currentScreen?.GetNextId() ?? 0;
        ChildControls = new List<Control>();
        
        Position = Vector2.Zero;
        Size = Vector2.Zero;
        PositionOffset = Vector2.Zero;

        BeginHover += ControlBeginHovering;
        HoverRelease += ControlStopHover;
        Hovering += ControlHover;

        var gameTime = Window.Instance.GetGameTime();
        var time = gameTime == null ? 0f : (float)gameTime.TotalGameTime.TotalSeconds;
        _creationTime = time;
        _fadeTime = _creationTime + 1f;

        OnClick += ControlClick;
        
        // Only register the click event if we were provided one.
        if (OnClick != null) Window.Instance.GetPointer().OnPrimaryClickRelease += HandleClickRelease;
    }

    protected virtual void Initialize()
    {
    }
    
    // Update and Draw methods
    public virtual void Update(GameTime gameTime)
    {
        //if (!IsVisible) return;

        UpdateAnimations(gameTime);

        HandleHover(gameTime);

        // Update position offset for child controls
        foreach (var con in ChildControls)
        {
            con.PositionOffset = GetActualPosition();
            con.Update(gameTime);
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Hidden) return;
        
        RenderChild(spriteBatch, gameTime);
    }

    /// <summary>
    ///   Draws the child controls of this control.
    /// </summary>
    /// <param name="spriteBatch"> The sprite batch to draw with. </param>
    /// <param name="gameTime"> The game time. </param>
    protected void RenderChild(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw child controls within the panel's boundaries
        foreach (var control in ChildControls){
            if (Hidden || IsMarkedForDeletion)
            {
                control.IsMarkedForDeletion = IsMarkedForDeletion;
                control.Hidden = Hidden;
            }

            if (!control.IsMarkedForDeletion && !control.Hidden)
            {
                control.Draw(spriteBatch, gameTime);
            }
        }
    }

    private void HandleHover(GameTime gameTime)
    {
        // If we have a parent, we inherit its canhover flag.
        if (Parent != null)
            CanHover = Parent.CanHover;
        
        // Only update hover & clicked state if we're not currently animating or hovering is disabled
        if (!CurrentlyAnimating && CanHover)
        {
            UpdateHoverState(gameTime);

            var pointer = Window.Instance.GetPointer();
            if (Hover && pointer.IsPrimaryClickPressed() && !Clicked)
                Clicked = true;
            else if (!pointer.IsPrimaryClickPressed()) Clicked = false;

            // Update ClickLerpAmount smoothly
            var clickTarget = Hover && pointer.IsPrimaryClickPressed() ? 1f : 0f;
            ColorCache.Update(this, clickTarget, gameTime);
        }
        else
        {
            // In case the controls hover and click state was changed while animating, reset it
            Hover = false;
            Clicked = false;
        }
    }

    private void ResetHoverState(Control exclude)
    {
        var controlsToCheck = Parent != null ? Parent.ChildControls : ChildControls;

        foreach (var control in controlsToCheck)
            if (control != exclude && control.Hover)
            {
                control.Hover = false;
                control.HoverRelease?.Invoke(control, Window.Instance.GetPointer().GetPointerPosition());
                control.HoverLerpAmount =
                    MathHelper.Clamp(
                        control.HoverLerpAmount -
                        (float)Window.Instance.GetGameTime().ElapsedGameTime.TotalSeconds *
                        ColorCache.HoverTransitionSpeed,
                        0f, 1f);
            }
    }

    private void UpdateHoverState(GameTime gameTime)
    {
        if (!CanHover)
        {
            Hover = false;
            return;
        }

        var pointer = Window.Instance.GetPointer().GetPointerPosition();
        var controlBox = GetHoverRect();

        // Make sure the pointer is within the control's boundaries
        var isPointerInside = controlBox.Contains(new Point2(pointer.X, pointer.Y));

        if (!isPointerInside)
        {

            // Exit the hover state if we're not hovering
            if (Hover)
            {
                HoverRelease?.Invoke(this, pointer);
                
                // Remove the control from the hover stack
                HoverStack.Pop();
            }

            // Update the hover state of the control
            HoverLerpAmount =
                MathHelper.Clamp(
                    HoverLerpAmount - (float)gameTime.ElapsedGameTime.TotalSeconds * ColorCache.HoverTransitionSpeed,
                    0f, 1f);

            Hover = false;

            return;
        }
        
        // If we are just entering the hover state, invoke the BeginHover event
        if (!Hover)
        {
            BeginHover?.Invoke(this, pointer);
            // Add the control to the hover stack
            HoverStack.Push(this);
            
            // Reset the hover state of all other controls
            ResetHoverState(this);
        }

        Hovering?.Invoke(this, pointer);
        HoverLerpAmount =
            MathHelper.Clamp(
                HoverLerpAmount + (float)gameTime.ElapsedGameTime.TotalSeconds * ColorCache.HoverTransitionSpeed,
                0f, 1f);

        Hover = true;
    }

    private void HandleClickRelease(object sender, EventArgs args)
    {
        if (Hover && Enabled && CanHover)
            OnClick?.Invoke(this, Window.Instance.GetPointer().GetPointerPosition());
    }
    
    private void UpdateAnimations(GameTime gameTime)
    {
        _animation?.Update(this, gameTime);
        _animation?.ApplyToChildControls(this);

        if (!_animation?.IsRunning ?? false)
            _animation = null;

        _closingAnimation?.Update(this, gameTime);
        _closingAnimation?.ApplyToChildControls(this);

        CurrentlyAnimating = Window.Instance.ScreenRenderer().CurrentlyAnimating();
    }
    
    public void SetClosingAnimation(IControlIClosingAnimation animation)
    {
        _closingAnimation = animation;
    }

    public void SetAnimation(IControlAnimation animation)
    {
        _animation = animation;
    }

    public void StartClosing()
    {
        _closingAnimation?.StartClosing();
    }

    protected bool JustFinishedCreation()
    {
        if(_alreadyCreated) return false;
        
        var gameTime = Window.Instance.GetGameTime();
        var elapsedTime = gameTime == null ? 0f : (float)gameTime.TotalGameTime.TotalSeconds - _creationTime;
        var baseOpacity = MathHelper.Clamp(elapsedTime / (_fadeTime - _creationTime), 0f, 1f);
        bool justFinished = baseOpacity >= 1f;
        
        if(justFinished) _alreadyCreated = true;
        return justFinished;
    }

    public bool FinishedClosing()
    {
        return _closingAnimation == null || !_closingAnimation.IsClosing;
    }

    public float GetOpacity()
    {
        if (_closingAnimation != null)
        {
            if(IsClosing)
                return _closingAnimation.GetOpacity();
        }
        
        var gameTime = Window.Instance.GetGameTime();
        var elapsedTime = gameTime == null ? 0f : (float)gameTime.TotalGameTime.TotalSeconds - _creationTime;

        // Calculate baseOpacity based on elapsedTime and _fadeTime
        var baseOpacity = MathHelper.Clamp(elapsedTime / (_fadeTime - _creationTime), 0f, 1f);

        var hoverOpacity = 0.8f;
        var normalOpacity = 1f;

        var currentOpacity = MathHelper.Lerp(normalOpacity, hoverOpacity, HoverLerpAmount);
        currentOpacity *= baseOpacity;


        if (_closingAnimation != null)
            if (_closingAnimation.IsClosing)
                currentOpacity = _closingAnimation.GetOpacity();

        return currentOpacity;
    }

    protected Vector2 GetScaleFactor()
    {
        var anim = Parent != null ? Parent._animation : _animation;
        if(anim != null)
            if (anim.IsRunning)
                return anim.GetScaleFactor();
        
        var closeAnimation = Parent != null ? Parent._closingAnimation : _closingAnimation;
        if (closeAnimation != null)
            return closeAnimation.GetScaleFactor();

        return Vector2.Zero;
    }

    protected virtual RectangleF GetHoverRect()
    {
        return new RectangleF(GetActualPosition().X, GetActualPosition().Y, Size.X, Size.Y);
    }

    public virtual void SetTextScale(float scale) { }

    protected virtual void ControlBeginHovering(object sender, Vector2 mousePos)
    {
        Hover = true;
    }

    protected virtual void ControlStopHover(object sender, Vector2 mousePos)
    {
        Hover = false;
    }

    protected virtual void ControlHover(object sender, Vector2 mousePos) { }

    protected virtual void ControlClick(object sender, Vector2 mousePos) { }

    protected virtual void OnResize() { }

    public void UpdateSize()
    {
        // For some reason waiting a small amount of time fixes the layout of certain controls.
        // Definitely seems to occur b/c size and or position is being modified after this function would normally be called.
        // TODO: Figure out why we need a timer here.
        Timer.Instance.CreateTimer(150f, (sender, args) => { OnResize(); });
    }
    
    public void SetParent(Panel panel)
    {
        Parent = panel;
    }

    public bool FinishedOpening()
    {
        bool finishedOpening = !_animation?.IsRunning ?? true;

        return finishedOpening;
    }

    public Vector2 GetActualPosition()
    {
        return new Vector2(Position.X + PositionOffset.X, Position.Y + PositionOffset.Y);
    }

    public List<Control> GetChildren()
    {
        return ChildControls;
    }

    public virtual void SetPosition(int i, int i1)
    {
        Position = new Vector2(i, i1);
    }

    public virtual void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void ScaleOut()
    {
        _closingAnimation = new ScaleClosingAnimation(this, Size, Vector2.One, 0.65f);
        _closingAnimation.StartClosing();
    }

    public void FadeOut(float duration)
    {
        _closingAnimation = new FadeAnimationClosing(this, duration);
        _closingAnimation.StartClosing();
    }
    
    protected void AddConsoleMessage(string message, [CallerMemberName] string caller = "", MessageType type = MessageType.Info)
    {
        Window.Instance.ScreenRenderer().GetScreen().AddConsoleMessage(message, caller, type);
    }

    ~Control()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from events
            BeginHover -= ControlBeginHovering;
            Hovering -= ControlHover;
            HoverRelease -= ControlStopHover;
            OnClick -= ControlClick;
            Window.Instance.GetPointer().OnPrimaryClickRelease -= HandleClickRelease;
            GC.SuppressFinalize(this);
        
            AddConsoleMessage($"Control {ID} disposed.");
        }
    }
}