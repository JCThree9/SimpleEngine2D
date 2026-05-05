using Engine.Core;
using Engine.GameObjects;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using SampleGame.Components;

namespace SampleGame.Scenes;

public class GameScene : Scene
{
    public override void Initialize()
    {
        // Create the player at world origin (0,0)
        // The camera is centered on (0,0), so this appears at the center of the screen
        var player = new GameObject("Player");
        player.Transform.Position = Vector2.Zero;
        player.Transform.Size = new Vector2(40, 40);
        player.AddComponent(new PlayerController { MoveSpeed = 250f });
        AddGameObject(player);
    }

    public override void Update(GameTime gameTime)
    {
        // Press Escape to push the pause scene on top
        if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            Game!.Scenes.Push(new PauseScene());
        }
    }

    public override void Draw(Renderer renderer)
    {
        foreach (var gameObject in GameObjects)
        {
            renderer.DrawRect(
                gameObject.Transform.Position,
                gameObject.Transform.Size,
                Color.Cyan
            );
        }

        // Draw a ground bar across the world, below center
        renderer.DrawRect(
            new Rectangle(-1000, 200, 2000, 20),
            Color.DarkGray
        );
    }
}
