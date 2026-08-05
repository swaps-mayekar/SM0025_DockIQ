using DockIQ.Core;
using DockIQ.Levels;
using DockIQ.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.Gameplay
{
    public sealed class GameSceneController : MonoBehaviour
    {
        [SerializeField] private GameHud _hud;
        [SerializeField] private LevelController _controller;

        private void Start()
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

            PromoteUiBackgroundToWorld();

            var level = LevelCatalog.Get(ProgressStore.GetSelectedLevel());
            _controller.Begin(level, _hud);
        }

        /// <summary>
        /// MenuBG/GameBG on GameHUD (Screen Space Overlay) covers all world sprites.
        /// Move the assigned sprite into a world-space renderer behind the board.
        /// </summary>
        private void PromoteUiBackgroundToWorld()
        {
            Transform uiBg = null;
            var canvas = GameObject.Find("GameHUD");
            if (canvas != null)
            {
                uiBg = canvas.transform.Find("MenuBG");
                if (uiBg == null)
                    uiBg = canvas.transform.Find("GameBG");
            }

            Sprite sprite = null;
            if (uiBg != null)
            {
                var image = uiBg.GetComponent<Image>();
                if (image != null)
                    sprite = image.sprite;
                uiBg.gameObject.SetActive(false);
            }

            if (sprite == null)
                return;

            var bg = GetComponent<GameBackground>();
            if (bg == null)
                bg = gameObject.AddComponent<GameBackground>();
            bg.Setup(sprite);
        }
    }
}
