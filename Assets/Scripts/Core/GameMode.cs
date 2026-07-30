namespace DockIQ.Core
{
    public enum GameMode
    {
        Story = 0,
        FreePlay = 1
    }

    /// <summary>
    /// Session-level launch context. Survives scene reloads within a play session
    /// so Story vs Free Play progression rules stay consistent after Retry/Next.
    /// </summary>
    public static class GameSession
    {
        public static GameMode Mode { get; private set; } = GameMode.Story;

        public static bool IsStory => Mode == GameMode.Story;

        public static void SetMode(GameMode mode) => Mode = mode;
    }
}
