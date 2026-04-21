using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SampleGame.Scenes;

/// <summary>
/// Simple pause screen. Overlays on top of the GameScene.
/// Press Escape or Enter to unpause.
/// </summary>
public class PauseScene : Scene
{
    public override void Initialize()
    {
        UseCamera = false; // UI overlay, draw in screen space

        // Pause the game time
        Time.TimeScale = 0f;
    }

    public override void Update(GameTime gameTime)
    {
        // Resume on Escape or Enter
        if (Input.IsKeyPressed(Keys.Escape) || Input.IsKeyPressed(Keys.Enter))
        {
            Time.TimeScale = 1f;
            Game!.Scenes.Pop();
        }
    }

    public override void Draw(Renderer renderer)
    {
        // Semi-transparent overlay
        renderer.DrawRect(
            new Rectangle(0, 0, Game!.ScreenWidth, Game.ScreenHeight),
            new Color(0, 0, 0, 150)
        );

        // "PAUSED" text — we'd use a font here, but for now draw a white rectangle
        // as a placeholder indicator
        var center = new Vector2(Game.ScreenWidth / 2f, Game.ScreenHeight / 2f);
        renderer.DrawRect(
            center,
            new Vector2(200, 60),
            new Color(50, 50, 50, 200)
        );
        renderer.DrawRect(
            center,
            new Vector2(196, 56),
            new Color(80, 80, 80, 220)
        );
    }
}
