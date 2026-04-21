using Engine.Core;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

/// <summary>
/// Applies a velocity to the owner's Transform position each frame.
/// </summary>
public class Velocity : Component
{
    /// <summary>Movement speed in pixels per second.</summary>
    public Vector2 Speed { get; set; } = Vector2.Zero;

    public override void Update(GameTime gameTime)
    {
        Transform.Position += Speed * Time.DeltaTime;
    }
}
