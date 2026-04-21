using Engine.Core;
using Engine.GameObjects;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using SampleGame.Components;

namespace SampleGame.Scenes;

/// <summary>
/// The main gameplay scene.
/// Creates a player square that moves with WASD. Press Escape for pause menu.
/// </summary>
public class GameScene : Scene
{
    private GameObject _player = null!;

    public override void Initialize()
    {
        // Create the player at the center of the screen
        _player = new GameObject("Player");
        _player.Transform.Position = new Vector2(Game!.ScreenWidth / 2f, Game.ScreenHeight / 2f);
        _player.AddComponent(new PlayerController { MoveSpeed = 250f });
        AddGameObject(_player);
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
        // Draw the player as a colored rectangle (no sprite needed yet)
        renderer.DrawRect(
            _player.Transform.Position,
            new Vector2(40, 40),
            Color.Cyan
        );

        // Draw some ground reference
        renderer.DrawRect(
            new Rectangle(0, (int)(Game!.ScreenHeight * 0.7f), Game.ScreenWidth, 20),
            Color.DarkGray
        );
    }
}
