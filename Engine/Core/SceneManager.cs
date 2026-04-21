using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.Core;

/// <summary>
/// Manages a stack of Scenes. The topmost scene is the active one
/// that receives Update and Draw calls.
/// </summary>
public class SceneManager
{
    private readonly Stack<Scene> _sceneStack = new();
    private Scene? _pendingScene;
    private enum PendingAction { None, Push, Switch }
    private PendingAction _pendingAction = PendingAction.None;

    /// <summary>Reference to the game — set by EngineGame.</summary>
    internal EngineGame? Game { get; set; }

    /// <summary>The currently active scene (top of the stack).</summary>
    public Scene? ActiveScene => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    /// <summary>Number of scenes on the stack.</summary>
    public int Count => _sceneStack.Count;

    /// <summary>
    /// Push a scene on top of the stack. The previous scene stays underneath
    /// (useful for pause menus layered over gameplay).
    /// </summary>
    public void Push(Scene scene)
    {
        _pendingScene = scene;
        _pendingAction = PendingAction.Push;
    }

    /// <summary>
    /// Remove the top scene and return to the one below it.
    /// </summary>
    public void Pop()
    {
        if (_sceneStack.Count > 0)
        {
            _sceneStack.Pop();
        }
    }

    /// <summary>
    /// Clear the entire stack and push a new scene.
    /// Use this for full scene transitions (e.g. menu → gameplay).
    /// </summary>
    public void Switch(Scene scene)
    {
        _pendingScene = scene;
        _pendingAction = PendingAction.Switch;
    }

    internal void Update(GameTime gameTime)
    {
        ProcessPending();
        ActiveScene?.InternalUpdate(gameTime);
    }

    internal void Draw(Renderer renderer)
    {
        ActiveScene?.InternalDraw(renderer);
    }

    internal void ProcessPending()
    {
        if (_pendingAction == PendingAction.None || _pendingScene == null)
            return;

        if (_pendingAction == PendingAction.Switch)
        {
            _sceneStack.Clear();
        }

        _sceneStack.Push(_pendingScene);

        // Initialize the new scene if the game is available
        if (Game != null)
        {
            _pendingScene.Game = Game;
            _pendingScene.InternalInitialize(Game);
        }

        _pendingScene = null;
        _pendingAction = PendingAction.None;
    }
}
