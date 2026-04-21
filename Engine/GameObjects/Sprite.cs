using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.GameObjects;

/// <summary>
/// Renders a texture at the GameObject's Transform position.
/// </summary>
public class Sprite : Component
{
    /// <summary>The texture to draw.</summary>
    public Texture2D? Texture { get; set; }

    /// <summary>Optional source rectangle (for sprite sheets).</summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>Tint color. Defaults to White (no tint).</summary>
    public Color Tint { get; set; } = Color.White;

    public override void Draw(Renderer renderer)
    {
        if (Texture == null) return;

        renderer.DrawSprite(
            Texture,
            Transform.Position,
            Tint,
            Transform.Rotation,
            Transform.Scale,
            SourceRect
        );
    }
}
