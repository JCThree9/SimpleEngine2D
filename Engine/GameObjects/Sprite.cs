using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.GameObjects;

public class Sprite : Component
{
    public Texture2D? Texture { get; set; }

    public Rectangle? SourceRect { get; set; }

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
