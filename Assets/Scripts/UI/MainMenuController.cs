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
                new Vector2(0f, 560f), new Vector2(400f, 40f));

            // Scrollable level grid for 48 levels
            var scrollGo = new GameObject("LevelScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(safe, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -40f);
            scrollRt.sizeDelta = new Vector2(980f, 980f);
            var scrollImg = scrollGo.GetComponent<Image>();
            scrollImg.color = new Color(0.06f, 0.10f, 0.16f, 0.55f);
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = new Vector2(12f, 12f);
            viewportRt.offsetMax = new Vector2(-12f, -12f);
            viewportGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            var gridGo = new GameObject("LevelGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            gridGo.transform.SetParent(viewportGo.transform, false);
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 1f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.anchoredPosition = Vector2.zero;
            gridRt.sizeDelta = new Vector2(0f, 0f);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140f, 140f);
            grid.spacing = new Vector2(18f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(8, 8, 8, 8);

            var fitter = gridGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.content = gridRt;
            scroll.viewport = viewportRt;
            _levelGrid = gridGo.transform;

            UiFactory.CreateText(safe, "Hint",
                "Tap switches, turntables, bridges & liftables. Slide path pieces. Avoid scrap — collisions fail!",
                20, FontStyles.Normal, new Vector2(0f, -920f), new Vector2(960f, 110f));
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
                rt.sizeDelta = new Vector2(140f, 140f);
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
