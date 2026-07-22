namespace DockIQ.Core
{
    public static class GameConstants
    {
        public const string SceneSplash = "0_SplashScene";
        public const string SceneMainMenu = "1_MainMenu";
        public const string SceneGame = "2_Game";

        public const string PrefProfile = "DockIQ.Profile.v1";
        public const string PrefSelectedLevel = "DockIQ.SelectedLevel";

        public const float SplashSeconds = 1.6f;
        public const float DefaultCellSize = 1f;
        public const float DefaultTickSeconds = 0.45f;

        public const int TotalLevels = 5;

        /// <summary>Board is rendered in classic 2:1 isometric 2D (see IsoMath).</summary>
        public const bool UseIsometricView = true;
    }
}
