using System.Collections.Generic;
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
        private const float AchievementRowHeight = 150f;
        private const float AchievementIconSize = 120f;

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
        [SerializeField] private TextMeshProUGUI _achievementsBody;
        [SerializeField] private RectTransform _achievementsContent;
        [SerializeField] private TextMeshProUGUI _achievementsSummary;
        [SerializeField] private Sprite[] _achievementIcons;
        [SerializeField] private BoardArtCatalog _boardArt;

        [Header("How To Play")]
        [SerializeField] private GameObject _howToPlayPanel;
        [SerializeField] private Button _howToPlayBackButton;
        [SerializeField] private TextMeshProUGUI _howToPlayBody;

        // Legacy builder wiring (kept so older scenes still compile/wire).
        [SerializeField] private Button _playButton;

        private readonly List<GameObject> _achievementRows = new List<GameObject>(12);

        private void Awake()
        {
            EnsurePanels();
            WireButtons();
            EnsureAchievementIcons();
            AchievementStore.EvaluateFromProgress();
            PopulateHowToPlay();
            PopulateAchievements();
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
                _achievementsPanel = CreateRuntimeScrollModal(canvasRt, "Panel_Achievements", "Achievements",
                    out _achievementsBackButton, out _achievementsBody);
            }
            else if (_achievementsPanel != null)
            {
                if (_achievementsPanel.transform.Find("Card/BodyScroll") == null && canvasRt != null)
                {
                    int sibling = _achievementsPanel.transform.GetSiblingIndex();
                    var stale = _achievementsPanel;
                    stale.name = "Panel_Achievements_Stale";
                    stale.SetActive(false);
                    Destroy(stale);

                    _achievementsPanel = CreateRuntimeScrollModal(canvasRt, "Panel_Achievements", "Achievements",
                        out _achievementsBackButton, out _achievementsBody);
                    _achievementsPanel.transform.SetSiblingIndex(sibling);
                }
                else
                {
                    if (_achievementsBody == null)
                        _achievementsBody = FindBodyText(_achievementsPanel.transform);
                    if (_achievementsBackButton == null)
                    {
                        var back = _achievementsPanel.transform.Find("Card/BackButton");
                        if (back != null)
                            _achievementsBackButton = back.GetComponent<Button>();
                    }
                }
            }

            EnsureAchievementListHost();

            if (_howToPlayPanel == null && canvasRt != null)
            {
                _howToPlayPanel = CreateRuntimeScrollModal(canvasRt, "Panel_HowToPlay", "How to Play",
                    out _howToPlayBackButton, out _howToPlayBody);
            }
            else if (_howToPlayPanel != null && _howToPlayBody == null)
            {
                _howToPlayBody = FindBodyText(_howToPlayPanel.transform);
            }
        }

        private void EnsureAchievementListHost()
        {
            if (_achievementsPanel == null)
                return;

            if (_achievementsContent == null)
            {
                var content = _achievementsPanel.transform.Find("Card/BodyScroll/Viewport/Content");
                if (content != null)
                    _achievementsContent = (RectTransform)content;
            }

            if (_achievementsContent == null)
                return;

            if (_achievementsContent.GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = _achievementsContent.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.spacing = 12f;
                layout.padding = new RectOffset(8, 8, 8, 8);
            }

            if (_achievementsContent.GetComponent<ContentSizeFitter>() == null)
            {
                var fitter = _achievementsContent.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (_achievementsBody != null)
                _achievementsBody.gameObject.SetActive(false);

            if (_achievementsSummary == null)
            {
                var existing = _achievementsContent.Find("Summary");
                if (existing != null)
                    _achievementsSummary = existing.GetComponent<TextMeshProUGUI>();
            }

            if (_achievementsSummary == null)
            {
                _achievementsSummary = CreateRuntimeText(_achievementsContent, "Summary", "Unlocked  0/9", 26,
                    FontStyles.Bold, Vector2.zero, new Vector2(780f, 40f), PlaceholderArt.Hazard);
                _achievementsSummary.alignment = TextAlignmentOptions.Center;
                var summaryRt = (RectTransform)_achievementsSummary.transform;
                summaryRt.anchorMin = new Vector2(0f, 1f);
                summaryRt.anchorMax = new Vector2(1f, 1f);
                summaryRt.pivot = new Vector2(0.5f, 1f);
                summaryRt.sizeDelta = new Vector2(0f, 44f);
                var le = _achievementsSummary.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 44f;
                le.preferredHeight = 44f;
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
            bool any = false;
            for (int i = 0; i < count; i++)
            {
                Sprite sprite = null;
                if (_achievementIcons != null && i < _achievementIcons.Length)
                    sprite = _achievementIcons[i];
                if (sprite == null && _boardArt != null)
                    sprite = _boardArt.AchievementIcon(i);
                icons[i] = sprite;
                if (sprite != null)
                    any = true;
            }

#if UNITY_EDITOR
            if (!any)
            {
                var loaded = LoadAchievementSpritesEditor();
                for (int i = 0; i < count && i < loaded.Length; i++)
                    icons[i] = loaded[i];
            }
#endif

            _achievementIcons = icons;
            if (_boardArt != null)
                SpriteCatalog.Bind(_boardArt);
        }

#if UNITY_EDITOR
        private static Sprite[] LoadAchievementSpritesEditor()
        {
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/UI/Achievements.png");
            var list = new List<Sprite>(assets.Length);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    list.Add(sprite);
            }

            list.Sort((a, b) => SliceIndex(a.name).CompareTo(SliceIndex(b.name)));
            return list.ToArray();
        }

        private static int SliceIndex(string name)
        {
            if (string.IsNullOrEmpty(name))
                return int.MaxValue;
            int underscore = name.LastIndexOf('_');
            if (underscore >= 0 && underscore + 1 < name.Length &&
                int.TryParse(name.Substring(underscore + 1), out int index))
                return index;
            return int.MaxValue;
        }
#endif

        private void OnProgressChanged()
        {
            RefreshLevels();
            RefreshStoryLabels();
            PopulateAchievements();
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
            PopulateAchievements();
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
                    bool completed = unlocked && ProgressStore.Current.lastCompleted >= id;
                    int mission = ProgressStore.Current.highestUnlocked;
                    bool selected = unlocked && id == mission;
                    UiChrome.ApplyLevelTile(image, unlocked, selected, completed);
                }

                view.Button.interactable = unlocked;
            }
        }

        private void PopulateHowToPlay()
        {
            if (_howToPlayBody == null)
                return;

            var sb = new StringBuilder(1024);
            sb.AppendLine("Robots drive the rails on their own. Tap devices to guide the highlighted rescue robot to the named dock before time runs out.");
            sb.AppendLine();
            sb.AppendLine("Story advances the campaign. Free Play replays unlocked levels without advancing Story.");
            sb.AppendLine();

            for (int i = 0; i < TutorialTipCatalog.AllTips.Count; i++)
            {
                var tip = TutorialTipCatalog.AllTips[i];
                // Intro already covers the goal tip.
                if (tip.Id == TutorialTipCatalog.MissionBasics)
                    continue;

                sb.AppendLine($"• {tip.Title} — {tip.Body}");
                sb.AppendLine();
            }

            _howToPlayBody.text = sb.ToString().TrimEnd();
            FitBodyHeight(_howToPlayBody);
        }

        private void PopulateAchievements()
        {
            EnsureAchievementListHost();
            EnsureAchievementIcons();
            if (_achievementsContent == null)
                return;

            for (int i = 0; i < _achievementRows.Count; i++)
            {
                if (_achievementRows[i] != null)
                    Destroy(_achievementRows[i]);
            }

            _achievementRows.Clear();

            int unlockedCount = 0;
            var all = AchievementCatalog.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (AchievementStore.IsUnlocked(all[i].Id))
                    unlockedCount++;
            }

            if (_achievementsSummary != null)
                _achievementsSummary.text = $"Unlocked  {unlockedCount}/{all.Count}";

            for (int i = 0; i < all.Count; i++)
            {
                var def = all[i];
                bool unlocked = AchievementStore.IsUnlocked(def.Id);
                Sprite icon = IconFor(i);
                _achievementRows.Add(CreateAchievementRow(_achievementsContent, def, icon, unlocked));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_achievementsContent);
        }

        private Sprite IconFor(int index)
        {
            if (_achievementIcons != null && index >= 0 && index < _achievementIcons.Length)
                return _achievementIcons[index];
            if (_boardArt != null)
                return _boardArt.AchievementIcon(index);
            return SpriteCatalog.AchievementIcon(index);
        }

        private static GameObject CreateAchievementRow(Transform parent, AchievementDef def, Sprite icon,
            bool unlocked)
        {
            var row = new GameObject($"Achievement_{def.Id}", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowRt = (RectTransform)row.transform;
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(0f, AchievementRowHeight);

            var rowBg = row.GetComponent<Image>();
            rowBg.sprite = UiChrome.RowBackground;
            rowBg.type = Image.Type.Sliced;
            rowBg.color = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.58f, 0.9f);

            var layout = row.GetComponent<LayoutElement>();
            layout.minHeight = AchievementRowHeight;
            layout.preferredHeight = AchievementRowHeight;
            layout.flexibleWidth = 1f;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(row.transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(16f, 0f);
            iconRt.sizeDelta = new Vector2(AchievementIconSize, AchievementIconSize);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = icon != null ? icon : PlaceholderArt.Circle();
            iconImg.preserveAspect = true;
            iconImg.color = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.38f, 0.85f);
            iconImg.raycastTarget = false;

            string body = unlocked ? def.Description : def.LockedHint;
            Color titleColor = unlocked ? PlaceholderArt.Hazard : new Color(0.65f, 0.68f, 0.72f, 1f);
            Color bodyColor = unlocked ? PlaceholderArt.Text : new Color(0.55f, 0.58f, 0.62f, 1f);

            var titleTmp = CreateRuntimeText(row.transform, "Title", def.Title, 26, FontStyles.Bold,
                new Vector2(0f, 28f), new Vector2(520f, 36f), titleColor);
            titleTmp.alignment = TextAlignmentOptions.Left;
            var titleRt = (RectTransform)titleTmp.transform;
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0f, 0.5f);
            titleRt.pivot = new Vector2(0f, 0.5f);
            titleRt.anchoredPosition = new Vector2(152f, 28f);
            titleRt.sizeDelta = new Vector2(620f, 36f);

            var bodyTmp = CreateRuntimeText(row.transform, "Body", body, 20, FontStyles.Normal,
                new Vector2(0f, -22f), new Vector2(520f, 70f), bodyColor);
            bodyTmp.alignment = TextAlignmentOptions.TopLeft;
            var bodyRt = (RectTransform)bodyTmp.transform;
            bodyRt.anchorMin = bodyRt.anchorMax = new Vector2(0f, 0.5f);
            bodyRt.pivot = new Vector2(0f, 0.5f);
            bodyRt.anchoredPosition = new Vector2(152f, -22f);
            bodyRt.sizeDelta = new Vector2(620f, 70f);

            return row;
        }

        private static void FitBodyHeight(TextMeshProUGUI body)
        {
            if (body == null)
                return;

            body.ForceMeshUpdate();
            var rt = (RectTransform)body.transform;
            float height = Mathf.Max(200f, body.preferredHeight + 24f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);

            var content = body.transform.parent as RectTransform;
            if (content != null)
                content.sizeDelta = new Vector2(content.sizeDelta.x, height);
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

        private static GameObject CreateRuntimePanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            StretchFull((RectTransform)go.transform);
            return go;
        }

        private static GameObject CreateRuntimeScrollModal(Transform parent, string name, string title,
            out Button backButton, out TextMeshProUGUI bodyText)
        {
            var panel = CreateRuntimePanel(parent, name);
            var cardGo = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGo.transform.SetParent(panel.transform, false);
            var cardRt = (RectTransform)cardGo.transform;
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(920f, 1400f);
            cardRt.anchoredPosition = Vector2.zero;
            var cardImg = cardGo.GetComponent<Image>();
            UiChrome.ApplyPanel(cardImg, large: true);

            CreateRuntimeText(cardGo.transform, "Title", title, 40, FontStyles.Bold,
                new Vector2(0f, 600f), new Vector2(840f, 60f), PlaceholderArt.Hazard);

            var scrollGo = new GameObject("BodyScroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(ScrollRect));
            scrollGo.transform.SetParent(cardGo.transform, false);
            var scrollRt = (RectTransform)scrollGo.transform;
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, 40f);
            scrollRt.sizeDelta = new Vector2(840f, 1000f);
            scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.09f, 0.14f, 0.65f);
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = (RectTransform)viewportGo.transform;
            StretchFull(viewportRt);
            viewportRt.offsetMin = new Vector2(16f, 16f);
            viewportRt.offsetMax = new Vector2(-16f, -16f);
            viewportGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            bodyText = CreateRuntimeText(contentGo.transform, "Body", "", 22, FontStyles.Normal,
                Vector2.zero, new Vector2(780f, 2000f), PlaceholderArt.Text);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            var bodyRt = (RectTransform)bodyText.transform;
            bodyRt.anchorMin = new Vector2(0f, 1f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 1f);
            bodyRt.anchoredPosition = Vector2.zero;
            bodyRt.sizeDelta = new Vector2(-20f, 2000f);

            scroll.content = contentRt;
            scroll.viewport = viewportRt;

            backButton = CreateRuntimeButton(cardGo.transform, "BackButton", "Back",
                new Vector2(0f, -620f), new Vector2(240f, 72f), new Color(0.15f, 0.35f, 0.55f, 1f));
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

            var button = go.GetComponent<Button>();
            var image = go.GetComponent<Image>();
            UiChrome.ApplyButton(image, button, UiChrome.StyleForButtonName(name));
            // Preserve intentional primary tint override when chrome missing.
            if (image.sprite == null || image.sprite.name == "PlaceholderWhite")
                image.color = color;

            CreateRuntimeText(go.transform, "Label", label, 28, FontStyles.Bold, Vector2.zero,
                new Vector2(size.x - 20f, 60f), PlaceholderArt.Text);
            return button;
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
