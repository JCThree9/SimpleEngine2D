using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Core;

public class InputManager
{
    private KeyboardState _currentKeys;
    private KeyboardState _previousKeys;
    private MouseState _currentMouse;
    private MouseState _previousMouse;

    public void Update()
    {
        _previousKeys = _currentKeys;
        _currentKeys = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }

    // ── Keyboard 

    public bool IsKeyPressed(Keys key) =>
        _currentKeys.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);

    public bool IsKeyHeld(Keys key) =>
        _currentKeys.IsKeyDown(key);

    public bool IsKeyReleased(Keys key) =>
        !_currentKeys.IsKeyDown(key) && _previousKeys.IsKeyDown(key);

    // ── Mouse 

    public Vector2 MousePosition => _currentMouse.Position.ToVector2();

    public bool IsLeftMousePressed() =>
        _currentMouse.LeftButton == ButtonState.Pressed &&
        _previousMouse.LeftButton == ButtonState.Released;

    public bool IsLeftMouseHeld() =>
        _currentMouse.LeftButton == ButtonState.Pressed;

    public bool IsLeftMouseReleased() =>
        _currentMouse.LeftButton == ButtonState.Released &&
        _previousMouse.LeftButton == ButtonState.Pressed;

    public bool IsRightMousePressed() =>
        _currentMouse.RightButton == ButtonState.Pressed &&
        _previousMouse.RightButton == ButtonState.Released;

    public int ScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
}
