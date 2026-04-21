using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

/// <summary>
/// A game entity. Has a Transform and a list of Components.
/// Components provide all the behavior (movement, rendering, etc.).
/// </summary>
public class GameObject
{
    private readonly List<Component> _components = new();
    private readonly List<Component> _pendingAdd = new();
    private readonly List<Component> _pendingRemove = new();

    /// <summary>Display name — useful for debugging.</summary>
    public string Name { get; set; }

    /// <summary>Whether this object is active. Inactive objects skip Update/Draw.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The scene this object belongs to (set automatically when added to a scene).</summary>
    public Scene? Scene { get; internal set; }

    /// <summary>Position, rotation, and scale.</summary>
    public Transform Transform { get; } = new();

    public GameObject(string name = "GameObject")
    {
        Name = name;
    }

    /// <summary>
    /// Attach a component to this GameObject.
    /// Returns the component for chaining.
    /// </summary>
    public T AddComponent<T>(T component) where T : Component
    {
        component.Owner = this;
        _pendingAdd.Add(component);
        return component;
    }

    /// <summary>Find the first component of type T, or null.</summary>
    public T? GetComponent<T>() where T : Component
    {
        foreach (var c in _components)
            if (c is T match) return match;

        // Also check pending adds
        foreach (var c in _pendingAdd)
            if (c is T match) return match;

        return null;
    }

    /// <summary>Remove a component by reference.</summary>
    public void RemoveComponent(Component component)
    {
        _pendingRemove.Add(component);
    }

    /// <summary>All components currently on this object (read-only).</summary>
    public IReadOnlyList<Component> Components => _components;

    internal void Update(GameTime gameTime)
    {
        // Flush pending changes
        if (_pendingAdd.Count > 0)
        {
            foreach (var c in _pendingAdd)
            {
                _components.Add(c);
                c.Initialize();
            }
            _pendingAdd.Clear();
        }
        if (_pendingRemove.Count > 0)
        {
            foreach (var c in _pendingRemove)
                _components.Remove(c);
            _pendingRemove.Clear();
        }

        if (!IsActive) return;

        foreach (var component in _components)
        {
            if (component.Enabled)
                component.Update(gameTime);
        }
    }

    internal void Draw(Renderer renderer)
    {
        if (!IsActive) return;

        foreach (var component in _components)
        {
            if (component.Enabled)
                component.Draw(renderer);
        }
    }
}
