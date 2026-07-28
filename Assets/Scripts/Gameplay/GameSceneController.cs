using DockIQ.Core;
using DockIQ.Levels;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    public sealed class GameSceneController : MonoBehaviour
    {
        [SerializeField] private GameHud _hud;
        [SerializeField] private LevelController _controller;

        private void Awake()
        {
            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = PlaceholderArt.Navy;

            _hud ??= FindFirstObjectByType<GameHud>();
            _controller ??= FindFirstObjectByType<LevelController>();

            if (_hud == null || _controller == null)
            {
                Debug.LogError("Game scene is missing scene-authored HUD/LevelController references.");
                return;
            }

            var level = LevelCatalog.Get(ProgressStore.GetSelectedLevel());
            _controller.Begin(level, _hud);
        }
    }
}
