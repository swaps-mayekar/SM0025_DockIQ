using System.Text;
using DockIQ.Core;
using DockIQ.Gameplay;
using DockIQ.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
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

        [Header("How To Play")]
        [SerializeField] private GameObject _howToPlayPanel;
        [SerializeField] private Button _howToPlayBackButton;
        [SerializeField] private TextMeshProUGUI _howToPlayBody;

        // Legacy builder wiring (kept so older scenes still compile/wire).
        [SerializeField] private Button _playButton;

        private void Awake()
        {
            EnsurePanels();
            WireButtons();
            PopulateHowToPlay();
            RefreshLevels();
            RefreshStoryLabels();
            ShowHome();
            ProgressStore.Changed += OnProgressChanged;
        }

        private void OnDestroy()
        {
            ProgressStore.Changed -= OnProgressChanged;
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

            // Legacy Play button still resumes Story if present.
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

        /// <summary>
        /// Builds the mode hub when the scene still only has Panel_Play (or wiring is incomplete).
        /// Scene-authored refs from DockIQ/Ensure Main Menu Modes take priority.
        /// </summary>
        private void EnsurePanels()
        {
            var canvas = GameObject.Find("MenuCanvas");
            var canvasRt = canvas != null ? (RectTransform)canvas.transform : null;

            if (_playPanel == null)
            {
                var found = GameObject.Find("Panel_Play");
                if (found != null)
                    _playPanel = found;
            }

            if (_playPanel != null)
            {
                var legacyPlay = _playPanel.transform.Find("PlayButton");
                if (legacyPlay != null)
                {
                    if (_playButton == null)
                        _playButton = legacyPlay.GetComponent<Button>();
                    legacyPlay.gameObject.SetActive(false);
                }

                var levelsLabel = _playPanel.transform.Find("LevelsLabel");
                if (levelsLabel != null)
                {
                    var tmp = levelsLabel.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                        tmp.text = "Free Play";
                }

                if (_playBackButton == null)
                {
                    var existingBack = _playPanel.transform.Find("BackButton");
                    if (existingBack != null)
                        _playBackButton = existingBack.GetComponent<Button>();
                    else
                        _playBackButton = CreateRuntimeButton(_playPanel.transform, "BackButton", "Back",
                            new Vector2(0f, 820f), new Vector2(220f, 72f),
                            new Color(0.15f, 0.35f, 0.55f, 1f));
                }
            }

            if (_homePanel == null && canvasRt != null)
            {
                _homePanel = CreateRuntimePanel(canvasRt, "Panel_Home");
                if (_playPanel != null)
                    _homePanel.transform.SetSiblingIndex(_playPanel.transform.GetSiblingIndex());

                CreateRuntimeText(_homePanel.transform, "Tagline", "WAREHOUSE RESCUE", 34, FontStyles.Bold,
                    new Vector2(0f, 760f), new Vector2(900f, 50f), PlaceholderArt.Hazard);
                _storyProgressText = CreateRuntimeText(_homePanel.transform, "StoryProgress",
                    "Story Progress  0/48", 26, FontStyles.Normal,
                    new Vector2(0f, 680f), new Vector2(900f, 40f), PlaceholderArt.Text);
                _storyMissionText = CreateRuntimeText(_homePanel.transform, "StoryMission",
                    "Continue: Level 1", 24, FontStyles.Normal,
                    new Vector2(0f, 620f), new Vector2(960f, 50f), PlaceholderArt.Text);

                _storyButton = CreateRuntimeButton(_homePanel.transform, "StoryButton", "Story Mode",
                    new Vector2(0f, 480f), new Vector2(420f, 90f), new Color(0.12f, 0.55f, 0.35f, 1f));
                _freePlayButton = CreateRuntimeButton(_homePanel.transform, "FreePlayButton", "Free Play",
                    new Vector2(0f, 360f), new Vector2(420f, 90f), new Color(0.15f, 0.35f, 0.55f, 1f));
                _achievementsButton = CreateRuntimeButton(_homePanel.transform, "AchievementsButton", "Achievements",
                    new Vector2(0f, 240f), new Vector2(420f, 90f), new Color(0.15f, 0.35f, 0.55f, 1f));
                _howToPlayButton = CreateRuntimeButton(_homePanel.transform, "HowToPlayButton", "How to Play",
                    new Vector2(0f, 120f), new Vector2(420f, 90f), new Color(0.15f, 0.35f, 0.55f, 1f));

                CreateRuntimeText(_homePanel.transform, "HomeHint",
                    "Story advances the rescue campaign. Free Play replays unlocked levels.",
                    20, FontStyles.Normal, new Vector2(0f, -40f), new Vector2(920f, 80f), PlaceholderArt.Text);
            }

            if (_achievementsPanel == null && canvasRt != null)
            {
                _achievementsPanel = CreateRuntimeModal(canvasRt, "Panel_Achievements", "Achievements",
                    "Achievements coming soon.\n\nComplete Story rescues to unlock future badges for perfect runs, decoy-free clears, and full-yard mastery.",
                    out _achievementsBackButton, out _);
            }

            if (_howToPlayPanel == null && canvasRt != null)
            {
                _howToPlayPanel = CreateRuntimeModal(canvasRt, "Panel_HowToPlay", "How to Play",
                    "",
                    out _howToPlayBackButton, out _howToPlayBody);
            }
        }

        private void OnProgressChanged()
        {
            RefreshLevels();
            RefreshStoryLabels();
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
        }

        private void ShowHowToPlay()
        {
            SetPanel(_homePanel, false);
            SetPanel(_playPanel, false);
            SetPanel(_achievementsPanel, false);
            SetPanel(_howToPlayPanel, true);
            PopulateHowToPlay();
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
                    image.color = unlocked
                        ? new Color(0.18f, 0.35f, 0.55f, 1f)
                        : new Color(0.2f, 0.22f, 0.25f, 1f);
                }

                view.Button.interactable = unlocked;
            }
        }

        private void PopulateHowToPlay()
        {
            if (_howToPlayBody == null)
                return;

            var sb = new StringBuilder(1024);
            sb.AppendLine("DockIQ is a warehouse rescue puzzle. Robots drive the rails automatically — you tap devices to reroute the highlighted rescue robot to the named dock before time runs out.");
            sb.AppendLine();
            sb.AppendLine("Story Mode continues your campaign. Free Play lets you replay any unlocked level without advancing Story.");
            sb.AppendLine();

            for (int i = 0; i < TutorialTipCatalog.AllTips.Count; i++)
            {
                var tip = TutorialTipCatalog.AllTips[i];
                sb.AppendLine($"• {tip.Title}");
                sb.AppendLine(tip.Body);
                sb.AppendLine();
            }

            _howToPlayBody.text = sb.ToString().TrimEnd();
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

        private static GameObject CreateRuntimePanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            StretchFull((RectTransform)go.transform);
            return go;
        }

        private static GameObject CreateRuntimeModal(Transform parent, string name, string title, string body,
            out Button backButton, out TextMeshProUGUI bodyText)
        {
            var panel = CreateRuntimePanel(parent, name);
            var cardGo = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGo.transform.SetParent(panel.transform, false);
            var cardRt = (RectTransform)cardGo.transform;
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(900f, 1200f);
            cardRt.anchoredPosition = Vector2.zero;
            var cardImg = cardGo.GetComponent<Image>();
            cardImg.sprite = PlaceholderArt.WhiteSquare();
            cardImg.color = PlaceholderArt.Panel;

            CreateRuntimeText(cardGo.transform, "Title", title, 40, FontStyles.Bold,
                new Vector2(0f, 480f), new Vector2(820f, 60f), PlaceholderArt.Hazard);
            bodyText = CreateRuntimeText(cardGo.transform, "Body", body, 22, FontStyles.Normal,
                new Vector2(0f, 20f), new Vector2(800f, 820f), PlaceholderArt.Text);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            backButton = CreateRuntimeButton(cardGo.transform, "BackButton", "Back",
                new Vector2(0f, -500f), new Vector2(240f, 72f), new Color(0.15f, 0.35f, 0.55f, 1f));
            panel.SetActive(false);
            return panel;
        }

        private static TextMeshProUGUI CreateRuntimeText(Transform parent, string name, string text, float size,
            FontStyles style, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateRuntimeButton(Transform parent, string name, string label, Vector2 anchoredPos,
            Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = PlaceholderArt.WhiteSquare();
            image.color = color;

            CreateRuntimeText(go.transform, "Label", label, 28, FontStyles.Bold, Vector2.zero,
                new Vector2(size.x - 20f, 60f), PlaceholderArt.Text);
            return go.GetComponent<Button>();
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
