using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Engine.Core;

/// <summary>
/// A scene is a self-contained game state (main menu, gameplay, pause screen, etc.).
/// Each scene owns a list of GameObjects.
/// Extend this class and override Initialize, Update, and Draw.
/// </summary>
public abstract class Scene
{
    private readonly List<GameObjects.GameObject> _gameObjects = new();
    private readonly List<GameObjects.GameObject> _pendingAdd = new();
    private readonly List<GameObjects.GameObject> _pendingRemove = new();
    private bool _initialized = false;

    /// <summary>Reference to the game instance (available after Initialize).</summary>
    public EngineGame? Game { get; internal set; }

    /// <summary>Shortcut to the game's InputManager.</summary>
    protected InputManager Input => Game!.Input;

    /// <summary>Shortcut to the game's ContentManager.</summary>
    protected ContentManager Content => Game!.Content;

    /// <summary>All GameObjects in this scene (read-only).</summary>
    public IReadOnlyList<GameObjects.GameObject> GameObjects => _gameObjects;

    /// <summary>Number of active game objects.</summary>
    public int GameObjectCount => _gameObjects.Count;

    /// <summary>
    /// If true (default), the scene draws in world space with the camera transform.
    /// Set to false for UI-only scenes (menus, hubs) that draw in screen space.
    /// </summary>
    public bool UseCamera { get; protected set; } = true;

    /// <summary>
    /// Add a GameObject to this scene. It will be initialized on the next frame.
    /// </summary>
    public void AddGameObject(GameObjects.GameObject obj)
    {
        _pendingAdd.Add(obj);
    }

    /// <summary>Remove a GameObject from this scene.</summary>
    public void RemoveGameObject(GameObjects.GameObject obj)
    {
        _pendingRemove.Add(obj);
    }

    /// <summary>Override to set up your scene (create game objects, load content, etc.).</summary>
    public abstract void Initialize();

    /// <summary>Override to add per-frame logic.</summary>
    public abstract void Update(GameTime gameTime);

    /// <summary>Override to draw scene-level visuals (backgrounds, UI, etc.).</summary>
    public abstract void Draw(Renderer renderer);

    internal void InternalInitialize(EngineGame game)
    {
        if (_initialized) return;
        Game = game;
        Initialize();
        FlushPending();
        _initialized = true;
    }

    internal void InternalUpdate(GameTime gameTime)
    {
        FlushPending();

        foreach (var obj in _gameObjects)
            obj.Update(gameTime);

        Update(gameTime);
    }

    internal void InternalDraw(Renderer renderer)
    {
        foreach (var obj in _gameObjects)
            obj.Draw(renderer);

        Draw(renderer);
    }

    private void FlushPending()
    {
        if (_pendingAdd.Count > 0)
        {
            foreach (var obj in _pendingAdd)
                obj.Scene = this;
            _gameObjects.AddRange(_pendingAdd);
            _pendingAdd.Clear();
        }
        if (_pendingRemove.Count > 0)
        {
            foreach (var obj in _pendingRemove)
            {
                obj.Scene = null;
                _gameObjects.Remove(obj);
            }
            _pendingRemove.Clear();
        }
    }
}
