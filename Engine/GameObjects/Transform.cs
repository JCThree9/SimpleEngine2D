using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

public class Transform
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Size { get; set; } = new(40f, 40f);
}
