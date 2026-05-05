using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Rendering;

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

    public SpriteBatch SpriteBatch => _spriteBatch;

    public void Begin()
    {
        _spriteBatch.Begin(
            transformMatrix: Camera.GetViewMatrix(),
            samplerState: SamplerState.PointClamp // pixel-perfect for 2D
        );
    }

    public void BeginUI()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
    }

    public void End()
    {
        _spriteBatch.End();
    }

    // Draw helpers 

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

    public void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixel, rect, color);
    }

    public void DrawRect(Vector2 position, Vector2 size, Color color)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(
            (int)(position.X - size.X / 2f),
            (int)(position.Y - size.Y / 2f),
            (int)size.X,
            (int)size.Y
        ), color);
    }

    public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
    {
        _spriteBatch.DrawString(font, text, position, color);
    }
}
