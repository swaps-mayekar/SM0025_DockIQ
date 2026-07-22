using DockIQ.Core;
using DockIQ.Levels;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    public sealed class GameSceneController : MonoBehaviour
    {
        private void Awake()
        {
            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = PlaceholderArt.Navy;

            var hud = gameObject.AddComponent<GameHud>();
            hud.Build();

            var level = LevelCatalog.Get(ProgressStore.GetSelectedLevel());
            var controller = gameObject.AddComponent<LevelController>();
            controller.Begin(level, hud);
        }
    }
}
