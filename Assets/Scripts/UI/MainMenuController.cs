using DockIQ.Core;
using DockIQ.Gameplay;
using DockIQ.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    /// <summary>
    /// Drives the main menu. All panels, buttons, and achievement rows are authored
    /// in <c>1_MainMenu.unity</c> — this controller only wires clicks and refreshes
    /// progress / unlock state. Use DockIQ/Ensure Menu UI In Scene to bake missing pieces.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject _homePanel;
        [SerializeField] private Button _storyButton;
        [SerializeField] private Button _freePlayButton;
        [SerializeField] private Button _achievementsButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private TextMeshProUGUI _storyProgressText;
        [SerializeField] private TextMeshProUGUI _storyMissionText;

        [Header("Free Play")]
        [SerializeField] private GameObject _playPanel;
        [SerializeField] private Button _playBackButton;
        [SerializeField] private LevelButtonView[] _levelButtons;

        [Header("Achievements")]
        [SerializeField] private GameObject _achievementsPanel;
        [SerializeField] private Button _achievementsBackButton;
        [SerializeField] private RectTransform _achievementsContent;
        [SerializeField] private TextMeshProUGUI _achievementsSummary;
        [SerializeField] private AchievementRowView[] _achievementRows;
        [SerializeField] private Sprite[] _achievementIcons;
        [SerializeField] private BoardArtCatalog _boardArt;

        [Header("How To Play")]
        [SerializeField] private GameObject _howToPlayPanel;
        [SerializeField] private Button _howToPlayBackButton;
        [SerializeField] private TextMeshProUGUI _howToPlayBody;

        // Legacy builder wiring (kept so older scenes still compile/wire).
        [SerializeField] private Button _playButton;

        private void Awake()
        {
            BindSceneFallbacks();
            WireButtons();
            EnsureAchievementIcons();
            AchievementStore.EvaluateFromProgress();
            RefreshAchievements();
            RefreshLevels();
            RefreshStoryLabels();
            FitHowToPlayScroll();
            ShowHome();
            ProgressStore.Changed += OnProgressChanged;
        }

        private void OnDestroy()
        {
            ProgressStore.Changed -= OnProgressChanged;
        }

        /// <summary>
        /// Resolves missing serialized refs from existing scene objects only.
        /// Never creates hierarchy — edit UI in the scene (or run Ensure Menu UI In Scene).
        /// </summary>
        private void BindSceneFallbacks()
        {
            if (_homePanel == null)
                _homePanel = FindNamed("Panel_Home");
            if (_playPanel == null)
                _playPanel = FindNamed("Panel_Play");
            if (_achievementsPanel == null)
                _achievementsPanel = FindNamed("Panel_Achievements");
            if (_howToPlayPanel == null)
                _howToPlayPanel = FindNamed("Panel_HowToPlay");

            if (_playPanel != null)
            {
                var legacyPlay = _playPanel.transform.Find("PlayButton");
                if (legacyPlay != null)
                {
                    if (_playButton == null)
                        _playButton = legacyPlay.GetComponent<Button>();
                    legacyPlay.gameObject.SetActive(false);
                }

                if (_playBackButton == null)
                {
                    var back = _playPanel.transform.Find("BackButton");
                    if (back != null)
                        _playBackButton = back.GetComponent<Button>();
                }
            }

            if (_achievementsPanel != null)
            {
                if (_achievementsBackButton == null)
                {
                    var back = _achievementsPanel.transform.Find("Card/BackButton");
                    if (back != null)
                        _achievementsBackButton = back.GetComponent<Button>();
                }

                if (_achievementsContent == null)
                {
                    var content = _achievementsPanel.transform.Find("Card/BodyScroll/Viewport/Content");
                    if (content != null)
                        _achievementsContent = (RectTransform)content;
                }

                if (_achievementsSummary == null && _achievementsContent != null)
                {
                    var summary = _achievementsContent.Find("Summary");
                    if (summary != null)
                        _achievementsSummary = summary.GetComponent<TextMeshProUGUI>();
                }

            if (_achievementRows == null || _achievementRows.Length == 0)
            {
                if (_achievementsContent != null)
                    _achievementRows = _achievementsContent.GetComponentsInChildren<AchievementRowView>(true);

#if UNITY_EDITOR
                if (_achievementRows == null || _achievementRows.Length == 0)
                {
                    Debug.LogWarning(
                        "DockIQ: Achievement rows missing from 1_MainMenu. Run menu item DockIQ/Ensure Menu UI In Scene so rows are editable in the scene.");
                }
#endif
            }
            }

            if (_howToPlayPanel != null)
            {
                if (_howToPlayBackButton == null)
                {
                    var back = _howToPlayPanel.transform.Find("Card/BackButton");
                    if (back != null)
                        _howToPlayBackButton = back.GetComponent<Button>();
                }

                if (_howToPlayBody == null)
                    _howToPlayBody = FindBodyText(_howToPlayPanel.transform);
            }
        }

        private static GameObject FindNamed(string name)
        {
            var found = GameObject.Find(name);
            if (found != null)
                return found;

            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == name)
                    return transforms[i].gameObject;
            }

            return null;
        }

        private void WireButtons()
        {
            if (_storyButton != null)
                _storyButton.onClick.AddListener(OnStory);
            if (_freePlayButton != null)
                _freePlayButton.onClick.AddListener(ShowFreePlay);
            if (_achievementsButton != null)
                _achievementsButton.onClick.AddListener(ShowAchievements);
            if (_howToPlayButton != null)
                _howToPlayButton.onClick.AddListener(ShowHowToPlay);

            if (_playBackButton != null)
                _playBackButton.onClick.AddListener(ShowHome);
            if (_achievementsBackButton != null)
                _achievementsBackButton.onClick.AddListener(ShowHome);
            if (_howToPlayBackButton != null)
                _howToPlayBackButton.onClick.AddListener(ShowHome);

            if (_playButton != null)
                _playButton.onClick.AddListener(OnStory);

            for (int i = 0; _levelButtons != null && i < _levelButtons.Length; i++)
            {
                var view = _levelButtons[i];
                if (view == null || view.Button == null)
                    continue;

                int id = view.LevelId;
                view.Button.onClick.AddListener(() => OnLevelPressed(id));
            }
        }

        private void EnsureAchievementIcons()
        {
            if (_achievementIcons != null && _achievementIcons.Length >= AchievementCatalog.All.Count)
            {
                bool complete = true;
                for (int i = 0; i < AchievementCatalog.All.Count; i++)
                {
                    if (_achievementIcons[i] == null)
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                    return;
            }

            if (_boardArt == null)
            {
#if UNITY_EDITOR
                _boardArt = UnityEditor.AssetDatabase.LoadAssetAtPath<BoardArtCatalog>(
                    "Assets/UI/BoardArtCatalog.asset");
#endif
            }

            int count = AchievementCatalog.All.Count;
            var icons = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                Sprite sprite = null;
                if (_achievementIcons != null && i < _achievementIcons.Length)
                    sprite = _achievementIcons[i];
                if (sprite == null && _boardArt != null)
                    sprite = _boardArt.AchievementIcon(i);
                if (sprite == null)
                    sprite = SpriteCatalog.AchievementIcon(i);
                icons[i] = sprite;
            }

            _achievementIcons = icons;
            if (_boardArt != null)
                SpriteCatalog.Bind(_boardArt);
        }

        private void OnProgressChanged()
        {
            RefreshLevels();
            RefreshStoryLabels();
            RefreshAchievements();
        }

        private void ShowHome()
        {
            SetPanel(_homePanel, true);
            SetPanel(_playPanel, false);
            SetPanel(_achievementsPanel, false);
            SetPanel(_howToPlayPanel, false);
            RefreshStoryLabels();
        }

        private void ShowFreePlay()
        {
            SetPanel(_homePanel, false);
            SetPanel(_playPanel, true);
            SetPanel(_achievementsPanel, false);
            SetPanel(_howToPlayPanel, false);
            RefreshLevels();
        }

        private void ShowAchievements()
        {
            SetPanel(_homePanel, false);
            SetPanel(_playPanel, false);
            SetPanel(_achievementsPanel, true);
            SetPanel(_howToPlayPanel, false);
            RefreshAchievements();
        }

        private void ShowHowToPlay()
        {
            SetPanel(_homePanel, false);
            SetPanel(_playPanel, false);
            SetPanel(_achievementsPanel, false);
            SetPanel(_howToPlayPanel, true);
            // Body copy is scene-authored — do not overwrite at runtime.
            FitHowToPlayScroll();
        }

        /// <summary>
        /// ContentSizeFitter alone collapses How To Play content to height 0 (no layout group),
        /// so ScrollRect elastically snaps back. Drive height from the body text like Achievements.
        /// </summary>
        private void FitHowToPlayScroll()
        {
            if (_howToPlayBody == null)
                return;

            var content = _howToPlayBody.transform.parent as RectTransform;
            if (content == null)
                return;

            var layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.spacing = 0f;
                layout.padding = new RectOffset(8, 8, 8, 8);
            }

            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var le = _howToPlayBody.GetComponent<LayoutElement>();
            if (le == null)
                le = _howToPlayBody.gameObject.AddComponent<LayoutElement>();

            _howToPlayBody.ForceMeshUpdate();
            float height = Mathf.Max(200f, _howToPlayBody.preferredHeight + 24f);
            le.minHeight = height;
            le.preferredHeight = height;

            var bodyRt = (RectTransform)_howToPlayBody.transform;
            bodyRt.sizeDelta = new Vector2(bodyRt.sizeDelta.x, height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        private void RefreshStoryLabels()
        {
            int mission = ProgressStore.Current.highestUnlocked;
            mission = Mathf.Clamp(mission, 1, LevelCatalog.Count);
            var level = LevelCatalog.Get(mission);
            int cleared = Mathf.Clamp(ProgressStore.Current.lastCompleted, 0, LevelCatalog.Count);

            if (_storyProgressText != null)
                _storyProgressText.text = $"Story Progress  {cleared}/{LevelCatalog.Count}";

            if (_storyMissionText != null)
            {
                _storyMissionText.text = cleared >= LevelCatalog.Count
                    ? "All rescues complete — replay any mission in Free Play"
                    : $"Continue: Level {mission} · {level.Title}";
            }
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
                    bool completed = unlocked && ProgressStore.Current.lastCompleted >= id;
                    int mission = ProgressStore.Current.highestUnlocked;
                    bool selected = unlocked && id == mission;
                    UiChrome.ApplyLevelTile(image, unlocked, selected, completed);
                }

                view.Button.interactable = unlocked;
            }
        }

        private void RefreshAchievements()
        {
            EnsureAchievementIcons();

            var all = AchievementCatalog.All;
            int unlockedCount = 0;
            for (int i = 0; i < all.Count; i++)
            {
                if (AchievementStore.IsUnlocked(all[i].Id))
                    unlockedCount++;
            }

            if (_achievementsSummary != null)
                _achievementsSummary.text = $"Unlocked  {unlockedCount}/{all.Count}";

            if (_achievementRows == null)
                return;

            for (int i = 0; i < _achievementRows.Length; i++)
            {
                var row = _achievementRows[i];
                if (row == null || string.IsNullOrEmpty(row.AchievementId))
                    continue;

                row.SetUnlocked(AchievementStore.IsUnlocked(row.AchievementId));
            }
        }

        private static TextMeshProUGUI FindBodyText(Transform panel)
        {
            var scrollBody = panel.Find("Card/BodyScroll/Viewport/Content/Body");
            if (scrollBody != null)
                return scrollBody.GetComponent<TextMeshProUGUI>();

            var body = panel.Find("Card/Body");
            return body != null ? body.GetComponent<TextMeshProUGUI>() : null;
        }

        private void OnStory()
        {
            int level = ProgressStore.Current.highestUnlocked;
            if (!ProgressStore.IsUnlocked(level))
                level = 1;

            GameSession.SetMode(GameMode.Story);
            ProgressStore.SetSelectedLevel(level);
            SceneRouter.LoadGame();
        }

        private void OnLevelPressed(int id)
        {
            if (!ProgressStore.IsUnlocked(id))
                return;

            GameSession.SetMode(GameMode.FreePlay);
            ProgressStore.SetSelectedLevel(id);
            SceneRouter.LoadGame();
        }
    }
}
