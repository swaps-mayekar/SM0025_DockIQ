using UnityEngine.SceneManagement;

namespace DockIQ.Core
{
    public static class SceneRouter
    {
        public static void LoadSplash() =>
            SceneManager.LoadScene(GameConstants.SceneSplash);

        public static void LoadMenu() =>
            SceneManager.LoadScene(GameConstants.SceneMainMenu);

        public static void LoadGame() =>
            SceneManager.LoadScene(GameConstants.SceneGame);

        public static void ReloadGame() =>
            SceneManager.LoadScene(GameConstants.SceneGame);
    }
}
