using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Debug;

public class DebugOverlay
{
    private bool _isVisible = false;
    private SpriteFont? _font;
    private readonly EngineGame _game;

    // FPS tracking
    private float _fpsTimer = 0f;
    private int _frameCount = 0;
    private float _currentFps = 0f;

    public bool IsVisible => _isVisible;

    public DebugOverlay(EngineGame game)
    {
        _game = game;
    }

    public void LoadContent(SpriteFont font)
    {
        _font = font;
    }

    public void Toggle() => _isVisible = !_isVisible;

    public void Update(GameTime gameTime)
    {
        // Toggle with F1
        if (_game.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F1))
            Toggle();

        // FPS counter
        _frameCount++;
        _fpsTimer += Time.UnscaledDeltaTime;
        if (_fpsTimer >= 1f)
        {
            _currentFps = _frameCount / _fpsTimer;
            _frameCount = 0;
            _fpsTimer = 0f;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!_isVisible || _font == null) return;

        spriteBatch.Begin();

        var y = 10f;
        var x = 10f;
        var lineHeight = 20f;
        var bgColor = new Color(0, 0, 0, 180);
        var textColor = Color.LimeGreen;

        // Background panel
        var panelHeight = lineHeight * 5 + 10;
        // We'll just draw text for now 

        DrawLine($"FPS: {_currentFps:F1}", x, y, textColor);
        y += lineHeight;
        DrawLine($"DeltaTime: {Time.DeltaTime:F4}s", x, y, textColor);
        y += lineHeight;
        DrawLine($"TimeScale: {Time.TimeScale:F2}x", x, y, textColor);
        y += lineHeight;

        var scene = _game.Scenes.ActiveScene;
        if (scene != null)
        {
            DrawLine($"Scene: {scene.GetType().Name}", x, y, textColor);
            y += lineHeight;
            DrawLine($"GameObjects: {scene.GameObjectCount}", x, y, textColor);
            y += lineHeight;
        }
        else
        {
            DrawLine("Scene: (none)", x, y, Color.Yellow);
            y += lineHeight;
        }

        DrawLine("[F1] Toggle Debug", x, y, Color.Gray);

        spriteBatch.End();

        void DrawLine(string text, float px, float py, Color color)
        {
            // Shadow
            spriteBatch.DrawString(_font, text, new Vector2(px + 1, py + 1), Color.Black);
            // Text
            spriteBatch.DrawString(_font, text, new Vector2(px, py), color);
        }
    }
}
