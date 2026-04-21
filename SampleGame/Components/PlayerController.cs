using Engine.Core;
using Engine.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SampleGame.Components;

/// <summary>
/// Moves a GameObject with WASD or arrow keys.
/// Demonstrates how a game-specific component uses the engine.
/// </summary>
public class PlayerController : Component
{
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

        // Normalize so diagonal movement isn't faster
        if (direction != Vector2.Zero)
            direction = Vector2.Normalize(direction);

        Transform.Position += direction * MoveSpeed * Time.DeltaTime;
    }
}
