using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Core;

/// <summary>
/// Tracks current and previous frame input state so you can detect
/// pressed/held/released for keyboard and mouse.
/// </summary>
public class InputManager
{
    private KeyboardState _currentKeys;
    private KeyboardState _previousKeys;
    private MouseState _currentMouse;
    private MouseState _previousMouse;

    /// <summary>Call once per frame, before any game logic.</summary>
    public void Update()
    {
        _previousKeys = _currentKeys;
        _currentKeys = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }

    // ── Keyboard 

    /// <summary>True only on the frame the key was first pressed.</summary>
    public bool IsKeyPressed(Keys key) =>
        _currentKeys.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);

    /// <summary>True every frame the key is held down.</summary>
    public bool IsKeyHeld(Keys key) =>
        _currentKeys.IsKeyDown(key);

    /// <summary>True only on the frame the key was released.</summary>
    public bool IsKeyReleased(Keys key) =>
        !_currentKeys.IsKeyDown(key) && _previousKeys.IsKeyDown(key);

    // ── Mouse 

    /// <summary>Current mouse position in screen coordinates.</summary>
    public Vector2 MousePosition => _currentMouse.Position.ToVector2();

    /// <summary>True only on the frame the left mouse button was first pressed.</summary>
    public bool IsLeftMousePressed() =>
        _currentMouse.LeftButton == ButtonState.Pressed &&
        _previousMouse.LeftButton == ButtonState.Released;

    /// <summary>True every frame the left mouse button is held.</summary>
    public bool IsLeftMouseHeld() =>
        _currentMouse.LeftButton == ButtonState.Pressed;

    /// <summary>True only on the frame the left mouse button was released.</summary>
    public bool IsLeftMouseReleased() =>
        _currentMouse.LeftButton == ButtonState.Released &&
        _previousMouse.LeftButton == ButtonState.Pressed;

    /// <summary>True only on the frame the right mouse button was first pressed.</summary>
    public bool IsRightMousePressed() =>
        _currentMouse.RightButton == ButtonState.Pressed &&
        _previousMouse.RightButton == ButtonState.Released;

    /// <summary>Mouse scroll wheel delta since last frame.</summary>
    public int ScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
}
