using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Rendering;

/// <summary>
/// Wraps MonoGame's SpriteBatch to provide simple draw calls.
/// Automatically applies the Camera's view matrix.
/// </summary>
public class Renderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;
    private Texture2D? _pixel;

    public Camera Camera { get; }

    public Renderer(GraphicsDevice graphicsDevice, Camera camera)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        Camera = camera;

        // Create a 1x1 white pixel texture for drawing shapes
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>The underlying SpriteBatch, if you need direct access.</summary>
    public SpriteBatch SpriteBatch => _spriteBatch;

    /// <summary>Begin a draw batch with the camera transform applied.</summary>
    public void Begin()
    {
        _spriteBatch.Begin(
            transformMatrix: Camera.GetViewMatrix(),
            samplerState: SamplerState.PointClamp // pixel-perfect for 2D
        );
    }

    /// <summary>Begin a screen-space draw batch (no camera transform — for UI).</summary>
    public void BeginUI()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
    }

    /// <summary>End the current draw batch.</summary>
    public void End()
    {
        _spriteBatch.End();
    }

    // Draw helpers 

    /// <summary>Draw a texture at a position.</summary>
    public void DrawSprite(Texture2D texture, Vector2 position, Color? color = null,
        float rotation = 0f, Vector2? scale = null, Rectangle? sourceRect = null)
    {
        var origin = sourceRect.HasValue
            ? new Vector2(sourceRect.Value.Width / 2f, sourceRect.Value.Height / 2f)
            : new Vector2(texture.Width / 2f, texture.Height / 2f);

        _spriteBatch.Draw(
            texture,
            position,
            sourceRect,
            color ?? Color.White,
            rotation,
            origin,
            scale ?? Vector2.One,
            SpriteEffects.None,
            0f
        );
    }

    /// <summary>Draw a filled rectangle.</summary>
    public void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixel, rect, color);
    }

    /// <summary>Draw a filled rectangle at a position with a size.</summary>
    public void DrawRect(Vector2 position, Vector2 size, Color color)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(
            (int)(position.X - size.X / 2f),
            (int)(position.Y - size.Y / 2f),
            (int)size.X,
            (int)size.Y
        ), color);
    }

    /// <summary>Draw text using a SpriteFont.</summary>
    public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
    {
        _spriteBatch.DrawString(font, text, position, color);
    }
}
