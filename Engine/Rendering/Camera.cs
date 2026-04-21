using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Rendering;

/// <summary>
/// 2D camera with position, zoom, and rotation.
/// Produces a view matrix for the Renderer.
/// </summary>
public class Camera
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;

    private Viewport _viewport;

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
    }

    /// <summary>Update the viewport (call if window resizes).</summary>
    public void UpdateViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    /// <summary>Center of the viewport in screen coordinates.</summary>
    public Vector2 Center => new(_viewport.Width / 2f, _viewport.Height / 2f);

    /// <summary>
    /// The view transformation matrix. Pass this to SpriteBatch.Begin().
    /// </summary>
    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f) *
            Matrix.CreateTranslation(new Vector3(Center, 0f));
    }

    /// <summary>Convert screen coordinates to world coordinates.</summary>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));
    }

    /// <summary>Convert world coordinates to screen coordinates.</summary>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition, GetViewMatrix());
    }
}
