using Microsoft.Xna.Framework;

namespace Engine.Core;

public static class Time
{
    public static float DeltaTime { get; private set; }

    public static float TotalTime { get; private set; }

    public static float TimeScale { get; set; } = 1f;

    public static float UnscaledDeltaTime { get; private set; }

    internal static void Update(GameTime gameTime)
    {
        UnscaledDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        DeltaTime = UnscaledDeltaTime * TimeScale;
        TotalTime = (float)gameTime.TotalGameTime.TotalSeconds;
    }
}
