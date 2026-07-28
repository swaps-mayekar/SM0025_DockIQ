using DockIQ.Core;
using DockIQ.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private LevelButtonView[] _levelButtons;

        private void Awake()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlay);

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                var view = _levelButtons[i];
                if (view == null || view.Button == null)
                    continue;

                int id = view.LevelId;
                view.Button.onClick.AddListener(() => OnLevelPressed(id));
            }

            RefreshLevels();
            ProgressStore.Changed += RefreshLevels;
        }

        private void OnDestroy()
        {
            ProgressStore.Changed -= RefreshLevels;
        }

        private void RefreshLevels()
        {
            if (_levelButtons == null || _levelButtons.Length == 0)
                return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                var view = _levelButtons[i];
                if (view == null || view.Button == null)
                    continue;

                int id = view.LevelId;
                bool validLevel = id >= 1 && id <= LevelCatalog.Count;
                bool unlocked = validLevel && ProgressStore.IsUnlocked(id);

                if (view.Label != null)
                    view.Label.text = unlocked ? id.ToString() : "Locked";

                var image = view.Button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = unlocked
                        ? new Color(0.18f, 0.35f, 0.55f, 1f)
                        : new Color(0.2f, 0.22f, 0.25f, 1f);
                }

                view.Button.interactable = unlocked;
            }
        }

        private void OnPlay()
        {
            int level = ProgressStore.GetSelectedLevel();
            if (!ProgressStore.IsUnlocked(level))
                level = ProgressStore.Current.highestUnlocked;
            ProgressStore.SetSelectedLevel(level);
            SceneRouter.LoadGame();
        }

        private void OnLevelPressed(int id)
        {
            if (!ProgressStore.IsUnlocked(id))
                return;

            ProgressStore.SetSelectedLevel(id);
            SceneRouter.LoadGame();
        }
    }
}
