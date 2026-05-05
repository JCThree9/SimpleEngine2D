using Engine.Rendering;
using Engine.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Engine.Core;

public abstract class Scene
{
    private readonly List<GameObjects.GameObject> _gameObjects = new();
    private readonly List<GameObjects.GameObject> _pendingAdd = new();
    private readonly List<GameObjects.GameObject> _pendingRemove = new();
    private bool _initialized = false;

    public EngineGame? Game { get; internal set; }

    protected InputManager Input => Game!.Input;

    protected ContentManager Content => Game!.Content;

    public IReadOnlyList<GameObjects.GameObject> GameObjects =>
        _gameObjects
            .Where(obj => !_pendingRemove.Contains(obj))
            .Concat(_pendingAdd)
            .ToList();

    public int GameObjectCount => GameObjects.Count;

    public bool UseCamera { get; protected set; } = true;

    public void AddGameObject(GameObjects.GameObject obj)
    {
        if (_gameObjects.Contains(obj) || _pendingAdd.Contains(obj))
            return;

        _pendingRemove.Remove(obj);
        obj.Scene = this;
        _pendingAdd.Add(obj);
    }

    public void RemoveGameObject(GameObjects.GameObject obj)
    {
        if (_pendingAdd.Remove(obj))
        {
            obj.Scene = null;
            return;
        }

        if (!_gameObjects.Contains(obj) || _pendingRemove.Contains(obj))
            return;

        _pendingRemove.Add(obj);
    }

    public abstract void Initialize();

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(Renderer renderer);
    // These methods are used internally to maintain and use a buffer. This is required so data is not altered mid frame thus the "internal" names
    internal void InternalInitialize(EngineGame game)
    {
        if (_initialized) return;
        Game = game;

        var saveFile = Path.Combine("SceneData", $"{GetType().Name}.json");
        if (File.Exists(saveFile))
            SceneSerializer.Load(saveFile, this);
        else
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
        FlushPending();

        foreach (var obj in _gameObjects)
            obj.Draw(renderer);

        Draw(renderer);
    }

    public void ClearAllGameObjects()
    {
        foreach (var obj in _gameObjects)
            obj.Scene = null;

        foreach (var obj in _pendingAdd)
            obj.Scene = null;

        _gameObjects.Clear();
        _pendingAdd.Clear();
        _pendingRemove.Clear();
    }

    private void FlushPending()
    {
        if (_pendingAdd.Count > 0)
        {
            foreach (var obj in _pendingAdd)
            {
                obj.Scene = this;

                if (!_gameObjects.Contains(obj))
                    _gameObjects.Add(obj);
            }

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
