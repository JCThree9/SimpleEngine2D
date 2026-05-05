using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.Core;

public class SceneManager
{
    private readonly Stack<Scene> _sceneStack = new();
    private Scene? _pendingScene;
    private enum PendingAction { None, Push, Switch }
    private PendingAction _pendingAction = PendingAction.None;

    internal EngineGame? Game { get; set; }

    public Scene? ActiveScene => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public int Count => _sceneStack.Count;

    public void Push(Scene scene)
    {
        _pendingScene = scene;
        _pendingAction = PendingAction.Push;
    }

    public void Pop()
    {
        if (_sceneStack.Count > 0)
        {
            _sceneStack.Pop();
        }
    }

    public void Switch(Scene scene)
    {
        _pendingScene = scene;
        _pendingAction = PendingAction.Switch;
    }
    //These classes use the internal methods from Scene to maintain updates to data only happening once every frame
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
