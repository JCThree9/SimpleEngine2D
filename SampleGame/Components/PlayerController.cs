using Engine.Core;
using Engine.Editor;
using Engine.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SampleGame.Components;

public class PlayerController : Component
{
    [EditorVisible]
    public float MoveSpeed { get; set; } = 200f;

    public override void Update(GameTime gameTime)
    {
        var direction = Vector2.Zero;

        if (Input.IsKeyHeld(Keys.W) || Input.IsKeyHeld(Keys.Up))
            direction.Y -= 1;
        if (Input.IsKeyHeld(Keys.S) || Input.IsKeyHeld(Keys.Down))
            direction.Y += 1;
        if (Input.IsKeyHeld(Keys.A) || Input.IsKeyHeld(Keys.Left))
            direction.X -= 1;
        if (Input.IsKeyHeld(Keys.D) || Input.IsKeyHeld(Keys.Right))
            direction.X += 1;

        // Normalize so diagonal movement isn't faster, this is a classic video game problem
        if (direction != Vector2.Zero)
            direction = Vector2.Normalize(direction);

        Transform.Position += direction * MoveSpeed * Time.DeltaTime;
    }
}
