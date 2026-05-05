using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

public class GameObject
{
    private readonly List<Component> _components = new();
    private readonly List<Component> _pendingAdd = new();
    private readonly List<Component> _pendingRemove = new();

    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public Scene? Scene { get; internal set; }

    public Transform Transform { get; } = new();

    public GameObject(string name = "GameObject")
    {
        Name = name;
    }

    public T AddComponent<T>(T component) where T : Component
    {
        component.Owner = this;

        if (_components.Contains(component) || _pendingAdd.Contains(component))
            return component;

        _pendingRemove.Remove(component);
        _pendingAdd.Add(component);
        return component;
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var c in Components)
            if (c is T match) return match;

        return null;
    }

    public void RemoveComponent(Component component)
    {
        if (_pendingAdd.Remove(component))
            return;

        if (!_components.Contains(component) || _pendingRemove.Contains(component))
            return;

        _pendingRemove.Add(component);
    }

    public IReadOnlyList<Component> Components =>
        _components
            .Where(component => !_pendingRemove.Contains(component))
            .Concat(_pendingAdd)
            .ToList();

    internal void Update(GameTime gameTime)
    {
        FlushPendingComponents();

        if (!IsActive) return;

        foreach (var component in _components)
        {
            if (component.Enabled)
                component.Update(gameTime);
        }
    }

    internal void Draw(Renderer renderer)
    {
        FlushPendingComponents();

        if (!IsActive) return;

        foreach (var component in _components)
        {
            if (component.Enabled)
                component.Draw(renderer);
        }
    }

    private void FlushPendingComponents()
    {
        if (_pendingAdd.Count > 0)
        {
            foreach (var component in _pendingAdd)
            {
                if (_components.Contains(component))
                    continue;

                _components.Add(component);
                component.Initialize();
            }

            _pendingAdd.Clear();
        }

        if (_pendingRemove.Count > 0)
        {
            foreach (var component in _pendingRemove)
                _components.Remove(component);

            _pendingRemove.Clear();
        }
    }
}
