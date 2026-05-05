using Engine.Core;
using Engine.Editor;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

public class Velocity : Component
{
    [EditorVisible]
    public Vector2 Speed { get; set; } = Vector2.Zero;

    public override void Update(GameTime gameTime)
    {
        Transform.Position += Speed * Time.DeltaTime;
    }
}
