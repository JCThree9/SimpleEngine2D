using Engine.Debug;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

/// <summary>
/// The main game class. Extends MonoGame's Game and wires together
/// all engine systems (input, scenes, rendering, debug).
///
/// Game projects should NOT extend this class — instead, create Scenes
/// and push them via Scenes.Push().
/// </summary>
public class EngineGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private Renderer _renderer = null!;
    private Camera _camera = null!;
    private DebugOverlay _debug = null!;

    /// <summary>The input manager. Use to check keyboard/mouse state.</summary>
    public InputManager Input { get; } = new();

    /// <summary>The scene manager. Use to push/pop/switch scenes.</summary>
    public SceneManager Scenes { get; } = new();

    /// <summary>The renderer. Passed to Scene.Draw() automatically.</summary>
    public Renderer Renderer => _renderer;

    /// <summary>The camera. Adjust Position and Zoom to move the view.</summary>
    public Camera Camera => _camera;

    /// <summary>The debug overlay. Toggle with F1 in sample game .</summary>
    public DebugOverlay Debug => _debug;

    /// <summary>Window width in pixels.</summary>
    public int ScreenWidth
    {
        get => _graphics.PreferredBackBufferWidth;
        set { _graphics.PreferredBackBufferWidth = value; _graphics.ApplyChanges(); }
    }

    /// <summary>Window height in pixels.</summary>
    public int ScreenHeight
    {
        get => _graphics.PreferredBackBufferHeight;
        set { _graphics.PreferredBackBufferHeight = value; _graphics.ApplyChanges(); }
    }

    public EngineGame(int width = 1280, int height = 720, string title = "SimpleEngine2D")
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;

        Window.Title = title;
        Window.AllowUserResizing = true;

        // Wires up scene manager's reference to us
        Scenes.Game = this;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _camera = new Camera(GraphicsDevice.Viewport);
        _renderer = new Renderer(GraphicsDevice, _camera);
        _debug = new DebugOverlay(this);

        // Try to load the debug font — it's optional
        try
        {
            var debugFont = Content.Load<SpriteFont>("DebugFont");
            _debug.LoadContent(debugFont);
        }
        catch
        {
            // No debug font available — overlay text won't render.
            // Games need to add a DebugFont.spritefont to their Content pipeline.
        }

        // Process any scenes that were pushed before Run() was called
        Scenes.ProcessPending();
    }

    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime);
        Input.Update();
        _debug.Update(gameTime);

        // Let SceneManager handle scene lifecycle + update
        Scenes.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Update camera viewport on resize
        _camera.UpdateViewport(GraphicsDevice.Viewport);

        // Draw the active scene
        if (Scenes.ActiveScene != null)
        {
            if (Scenes.ActiveScene.UseCamera)
                _renderer.Begin();      // world space with camera
            else
                _renderer.BeginUI();    // screen space, no camera

            Scenes.Draw(_renderer);
            _renderer.End();
        }

        // Draw debug overlay (screen-space, no camera)
        _debug.Draw(_renderer.SpriteBatch);

        base.Draw(gameTime);
    }
}
