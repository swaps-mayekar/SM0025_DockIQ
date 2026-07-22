using DockIQ.Core;
using DockIQ.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private Transform _levelGrid;

        private void Awake()
        {
            BuildUi();
            RefreshLevels();
            ProgressStore.Changed += RefreshLevels;
        }

        private void OnDestroy()
        {
            ProgressStore.Changed -= RefreshLevels;
        }

        private void BuildUi()
        {
            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = PlaceholderArt.Navy;

            var canvas = UiFactory.CreateCanvas(transform, "MenuCanvas");
            var safe = UiFactory.CreateSafeArea(canvas.transform);

            UiFactory.CreateText(safe, "Title", "DockIQ", 72, FontStyles.Bold,
                new Vector2(0f, 900f), new Vector2(900f, 100f)).color = Color.white;
            UiFactory.CreateText(safe, "Subtitle", "Warehouse Rescue", 34, FontStyles.Bold,
                new Vector2(0f, 820f), new Vector2(900f, 50f)).color = PlaceholderArt.Hazard;

            UiFactory.CreateButton(safe, "Play", "Play", new Vector2(0f, 680f), OnPlay)
                .GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.35f, 1f);

            UiFactory.CreateText(safe, "LevelsLabel", "Levels", 30, FontStyles.Bold,
                new Vector2(0f, 520f), new Vector2(400f, 40f));

            var gridGo = new GameObject("LevelGrid", typeof(RectTransform));
            gridGo.transform.SetParent(safe, false);
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0f, 80f);
            gridRt.sizeDelta = new Vector2(900f, 700f);

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 150f);
            grid.spacing = new Vector2(24f, 24f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;
            _levelGrid = gridGo.transform;

            UiFactory.CreateText(safe, "Hint", "Tap switches to rotate. Guide the gold VIP parcel.", 22,
                FontStyles.Normal, new Vector2(0f, -920f), new Vector2(900f, 80f));
        }

        private void RefreshLevels()
        {
            if (_levelGrid == null)
                return;

            for (int i = _levelGrid.childCount - 1; i >= 0; i--)
                Destroy(_levelGrid.GetChild(i).gameObject);

            foreach (var level in LevelCatalog.GetAll())
            {
                bool unlocked = ProgressStore.IsUnlocked(level.Id);
                int id = level.Id;
                var btn = UiFactory.CreateButton(_levelGrid, $"Level{id}",
                    unlocked ? id.ToString() : "Locked",
                    Vector2.zero,
                    () =>
                    {
                        if (!ProgressStore.IsUnlocked(id))
                            return;
                        ProgressStore.SetSelectedLevel(id);
                        SceneRouter.LoadGame();
                    });

                var rt = btn.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(150f, 150f);
                btn.GetComponent<Image>().color = unlocked
                    ? new Color(0.18f, 0.35f, 0.55f, 1f)
                    : new Color(0.2f, 0.22f, 0.25f, 1f);
                btn.interactable = unlocked;
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
    }
}
