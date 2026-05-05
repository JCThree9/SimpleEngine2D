using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;

namespace Engine.GameObjects;

public abstract class Component
{
    public GameObject Owner { get; internal set; } = null!;

    public bool Enabled { get; set; } = true;

    protected Transform Transform => Owner.Transform;

    protected Scene Scene => Owner.Scene!;

    protected EngineGame Game => Scene.Game!;

    protected InputManager Input => Game.Input;

    public virtual void Initialize() { }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(Renderer renderer) { }
}
