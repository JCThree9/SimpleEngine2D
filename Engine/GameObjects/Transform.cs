using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

/// <summary>
/// Position, rotation, and scale of a GameObject in 2D space.
/// Every GameObject has exactly one Transform.
/// </summary>
public class Transform
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
}
