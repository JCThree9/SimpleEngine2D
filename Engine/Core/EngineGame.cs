using Engine.Debug;
using Engine.Editor;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

public class EngineGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private Renderer _renderer = null!;
    private Camera _camera = null!;
    private DebugOverlay _debug = null!;
    private EditorOverlay _editor = null!;

    public InputManager Input { get; } = new();

    public SceneManager Scenes { get; } = new();

    public Renderer Renderer => _renderer;

    public Camera Camera => _camera;

    public DebugOverlay Debug => _debug;

    public EditorOverlay Editor => _editor;

    public int ScreenWidth
    {
        get => _graphics.PreferredBackBufferWidth;
        set { _graphics.PreferredBackBufferWidth = value; _graphics.ApplyChanges(); }
    }

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
        _editor = new EditorOverlay(this);

        
        try
        {
            var debugFont = Content.Load<SpriteFont>("DebugFont");
            _debug.LoadContent(debugFont);
            _editor.LoadContent(debugFont);
        }
        catch
        {
            
        }

        ComponentRegistry.Initialize();

        // Process any scenes that were pushed before Run() was called
        Scenes.ProcessPending();
    }

    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime);
        Input.Update();
        _debug.Update(gameTime);
        _editor.Update(gameTime);

        if (!_editor.IsVisible || !_editor.HasFocus)
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
        _editor.Draw(_renderer.SpriteBatch);

        base.Draw(gameTime);
    }
}
