using Microsoft.Xna.Framework;

namespace Engine.Core;

/// <summary>
/// Static helper that exposes frame timing info.
/// Updated once per frame by EngineGame.
/// </summary>
public static class Time
{
    /// <summary>Seconds elapsed since the last frame.</summary>
    public static float DeltaTime { get; private set; }

    /// <summary>Total seconds elapsed since the game started.</summary>
    public static float TotalTime { get; private set; }

    /// <summary>Multiplier for DeltaTime. Set to 0 to pause, 0.5 for slow-mo, etc.</summary>
    public static float TimeScale { get; set; } = 1f;

    /// <summary>Raw (unscaled) delta time — useful for UI/debug that shouldn't slow-mo.</summary>
    public static float UnscaledDeltaTime { get; private set; }

    internal static void Update(GameTime gameTime)
    {
        UnscaledDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        DeltaTime = UnscaledDeltaTime * TimeScale;
        TotalTime = (float)gameTime.TotalGameTime.TotalSeconds;
    }
}
