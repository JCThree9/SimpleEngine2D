using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

/// <summary>
/// Base class for all behaviors that can be attached to a GameObject.
/// Override Update and/or Draw to add functionality.
/// </summary>
public abstract class Component
{
    /// <summary>The GameObject this component is attached to.</summary>
    public GameObject Owner { get; internal set; } = null!;

    /// <summary>Whether this component is active. Disabled components skip Update/Draw.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Shortcut to Owner.Transform.</summary>
    protected Transform Transform => Owner.Transform;

    /// <summary>Shortcut to the scene this component's owner belongs to.</summary>
    protected Scene Scene => Owner.Scene!;

    /// <summary>Shortcut to the EngineGame instance.</summary>
    protected EngineGame Game => Scene.Game!;

    /// <summary>Shortcut to the engine's InputManager.</summary>
    protected InputManager Input => Game.Input;

    /// <summary>Called once when the component is first added to a scene.</summary>
    public virtual void Initialize() { }

    /// <summary>Called every frame. Override to add behavior.</summary>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>Called every frame during the draw phase. Override to render things.</summary>
    public virtual void Draw(Renderer renderer) { }
}
