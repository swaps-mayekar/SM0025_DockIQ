using System.Collections;
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
        private const float PanelFadeSeconds = 0.22f;
        private const float IntroStaggerSeconds = 0.07f;
        private const float IntroSlideSeconds = 0.32f;
        private const float IntroSlideOffset = -48f;
        private const float LogoPulseAmount = 0.035f;
        private const float LogoPulseSpeed = 2.2f;
        private const float ModalPopScale = 0.94f;

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
        [SerializeField] private UiChromeCatalog _uiChrome;

        [Header("How To Play")]
        [SerializeField] private GameObject _howToPlayPanel;
        [SerializeField] private Button _howToPlayBackButton;
        [SerializeField] private TextMeshProUGUI _howToPlayBody;

        // Legacy builder wiring (kept so older scenes still compile/wire).
        [SerializeField] private Button _playButton;

        private CanvasGroup _homeGroup;
        private CanvasGroup _playGroup;
        private CanvasGroup _achievementsGroup;
        private CanvasGroup _howToPlayGroup;
        private RectTransform _playCard;
        private RectTransform _achievementsCard;
        private RectTransform _howToPlayCard;
        private RectTransform _logo;
        private Vector3 _logoBaseScale = Vector3.one;
        private HomeButtonMotion[] _homeButtons;
        private Coroutine _transition;
        private Coroutine _intro;
        private GameObject _activePanel;
        private bool _introPlaying;

        private struct HomeButtonMotion
        {
            public Button Button;
            public RectTransform Rect;
            public CanvasGroup Group;
            public Vector2 RestPos;
        }

        private void Awake()
        {
            BindSceneFallbacks();
            BindUiChrome();
            BindMotionTargets();
            WireButtons();
            EnsureAchievementIcons();
            AchievementStore.EvaluateFromProgress();
            RefreshAchievements();
            RefreshLevels();
            RefreshStoryLabels();
            FitHowToPlayScroll();
            ShowHomeImmediate();
            ProgressStore.Changed += OnProgressChanged;
        }

        private void Start()
        {
            _intro = StartCoroutine(PlayHomeIntro());
        }

        private void Update()
        {
            if (_logo == null || _introPlaying)
                return;

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * LogoPulseSpeed) * LogoPulseAmount;
            _logo.localScale = _logoBaseScale * pulse;
        }

        private void OnDestroy()
        {
            ProgressStore.Changed -= OnProgressChanged;
        }

        private void BindMotionTargets()
        {
            _homeGroup = UiMotion.EnsureCanvasGroup(_homePanel);
            _playGroup = UiMotion.EnsureCanvasGroup(_playPanel);
            _achievementsGroup = UiMotion.EnsureCanvasGroup(_achievementsPanel);
            _howToPlayGroup = UiMotion.EnsureCanvasGroup(_howToPlayPanel);

            _playCard = FindCard(_playPanel);
            _achievementsCard = FindCard(_achievementsPanel);
            _howToPlayCard = FindCard(_howToPlayPanel);

            var logoGo = FindNamed("GameLogo");
            if (logoGo != null)
            {
                _logo = logoGo.transform as RectTransform;
                if (_logo != null)
                    _logoBaseScale = _logo.localScale;
            }

            _homeButtons = BuildHomeButtons();
        }

        private HomeButtonMotion[] BuildHomeButtons()
        {
            var buttons = new[] { _storyButton, _freePlayButton, _achievementsButton, _howToPlayButton };
            var list = new HomeButtonMotion[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null)
                    continue;

                var rt = button.transform as RectTransform;
                list[i] = new HomeButtonMotion
                {
                    Button = button,
                    Rect = rt,
                    Group = UiMotion.EnsureCanvasGroup(button.gameObject),
                    RestPos = rt != null ? rt.anchoredPosition : Vector2.zero
                };
            }

            return list;
        }

        private static RectTransform FindCard(GameObject panel)
        {
            if (panel == null)
                return null;

            var card = panel.transform.Find("Card");
            return card as RectTransform;
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

        private void BindUiChrome()
        {
            if (_uiChrome == null)
            {
#if UNITY_EDITOR
                _uiChrome = UnityEditor.AssetDatabase.LoadAssetAtPath<UiChromeCatalog>(
                    "Assets/UI/UiChromeCatalog.asset");
#endif
            }

            if (_uiChrome != null)
                UiChrome.Bind(_uiChrome);
        }

        private void OnProgressChanged()
        {
            RefreshLevels();
            RefreshStoryLabels();
            RefreshAchievements();
        }

        private void ShowHome()
        {
            RefreshStoryLabels();
            TransitionTo(_homePanel, _homeGroup, null, false);
        }

        private void ShowFreePlay()
        {
            RefreshLevels();
            TransitionTo(_playPanel, _playGroup, _playCard, true);
        }

        private void ShowAchievements()
        {
            RefreshAchievements();
            TransitionTo(_achievementsPanel, _achievementsGroup, _achievementsCard, true);
        }

        private void ShowHowToPlay()
        {
            // Body copy is scene-authored — do not overwrite at runtime.
            FitHowToPlayScroll();
            TransitionTo(_howToPlayPanel, _howToPlayGroup, _howToPlayCard, true);
        }

        private void ShowHomeImmediate()
        {
            SetPanelVisible(_homePanel, _homeGroup, true, 1f);
            SetPanelVisible(_playPanel, _playGroup, false, 0f);
            SetPanelVisible(_achievementsPanel, _achievementsGroup, false, 0f);
            SetPanelVisible(_howToPlayPanel, _howToPlayGroup, false, 0f);
            ResetCardScale(_playCard);
            ResetCardScale(_achievementsCard);
            ResetCardScale(_howToPlayCard);
            _activePanel = _homePanel;
            PrepareHomeIntroPose();
        }

        private void PrepareHomeIntroPose()
        {
            if (_homeButtons == null)
                return;

            for (int i = 0; i < _homeButtons.Length; i++)
            {
                var item = _homeButtons[i];
                if (item.Group == null || item.Rect == null)
                    continue;

                item.Group.alpha = 0f;
                item.Group.interactable = false;
                item.Group.blocksRaycasts = false;
                item.Rect.anchoredPosition = item.RestPos + new Vector2(0f, IntroSlideOffset);
            }
        }

        private IEnumerator PlayHomeIntro()
        {
            _introPlaying = true;

            if (_logo != null)
            {
                var logoGroup = UiMotion.EnsureCanvasGroup(_logo.gameObject);
                logoGroup.alpha = 0f;
                _logo.localScale = _logoBaseScale * 0.88f;

                float t = 0f;
                const float logoIn = 0.4f;
                while (t < 1f)
                {
                    t += Time.unscaledDeltaTime / logoIn;
                    float s = UiMotion.Smooth01(t);
                    logoGroup.alpha = s;
                    _logo.localScale = Vector3.Lerp(_logoBaseScale * 0.88f, _logoBaseScale, s);
                    yield return null;
                }

                logoGroup.alpha = 1f;
                _logo.localScale = _logoBaseScale;
            }

            if (_homeButtons != null && _homeButtons.Length > 0)
            {
                float elapsed = 0f;
                float total = IntroSlideSeconds + IntroStaggerSeconds * Mathf.Max(0, _homeButtons.Length - 1);

                while (elapsed < total)
                {
                    elapsed += Time.unscaledDeltaTime;
                    for (int i = 0; i < _homeButtons.Length; i++)
                    {
                        var item = _homeButtons[i];
                        if (item.Group == null || item.Rect == null)
                            continue;

                        float localT = (elapsed - i * IntroStaggerSeconds) / IntroSlideSeconds;
                        float s = UiMotion.Smooth01(localT);
                        item.Group.alpha = s;
                        item.Rect.anchoredPosition = Vector2.Lerp(
                            item.RestPos + new Vector2(0f, IntroSlideOffset),
                            item.RestPos,
                            s);
                    }

                    yield return null;
                }

                RestoreHomeButtonsVisible();
            }

            _introPlaying = false;
            _intro = null;
        }

        private void TransitionTo(GameObject panel, CanvasGroup group, RectTransform card, bool popIn)
        {
            if (panel == null || panel == _activePanel)
                return;

            if (_intro != null)
            {
                StopCoroutine(_intro);
                _intro = null;
                FinishHomeIntroImmediate();
            }

            if (_transition != null)
                StopCoroutine(_transition);

            _transition = StartCoroutine(AnimatePanelSwitch(panel, group, card, popIn));
        }

        private IEnumerator AnimatePanelSwitch(
            GameObject panel,
            CanvasGroup group,
            RectTransform card,
            bool popIn)
        {
            CanvasGroup fromGroup = GroupFor(_activePanel);
            if (fromGroup != null && _activePanel != null && _activePanel.activeSelf)
            {
                fromGroup.interactable = false;
                fromGroup.blocksRaycasts = false;
                yield return UiMotion.Fade(fromGroup, fromGroup.alpha, 0f, PanelFadeSeconds * 0.75f);
            }
            else
            {
                SetPanelVisible(_homePanel, _homeGroup, false, 0f);
                SetPanelVisible(_playPanel, _playGroup, false, 0f);
                SetPanelVisible(_achievementsPanel, _achievementsGroup, false, 0f);
                SetPanelVisible(_howToPlayPanel, _howToPlayGroup, false, 0f);
            }

            _activePanel = panel;

            if (popIn && card != null)
            {
                yield return UiMotion.FadeScale(
                    group,
                    card,
                    0f,
                    1f,
                    ModalPopScale,
                    1f,
                    PanelFadeSeconds);
            }
            else
            {
                if (panel == _homePanel)
                    RestoreHomeButtonsVisible();

                yield return UiMotion.Fade(group, 0f, 1f, PanelFadeSeconds);
            }

            _transition = null;
        }

        private void FinishHomeIntroImmediate()
        {
            _introPlaying = false;
            if (_logo != null)
            {
                var logoGroup = UiMotion.EnsureCanvasGroup(_logo.gameObject);
                logoGroup.alpha = 1f;
                _logo.localScale = _logoBaseScale;
            }

            RestoreHomeButtonsVisible();
        }

        private void RestoreHomeButtonsVisible()
        {
            if (_homeButtons == null)
                return;

            for (int i = 0; i < _homeButtons.Length; i++)
            {
                var item = _homeButtons[i];
                if (item.Group == null || item.Rect == null)
                    continue;

                item.Group.alpha = 1f;
                item.Group.interactable = true;
                item.Group.blocksRaycasts = true;
                item.Rect.anchoredPosition = item.RestPos;
            }
        }

        private CanvasGroup GroupFor(GameObject panel)
        {
            if (panel == _homePanel)
                return _homeGroup;
            if (panel == _playPanel)
                return _playGroup;
            if (panel == _achievementsPanel)
                return _achievementsGroup;
            if (panel == _howToPlayPanel)
                return _howToPlayGroup;
            return null;
        }

        private static void SetPanelVisible(GameObject panel, CanvasGroup group, bool active, float alpha)
        {
            if (panel == null)
                return;

            panel.SetActive(active);
            if (group == null)
                return;

            group.alpha = alpha;
            group.interactable = active;
            group.blocksRaycasts = active;
        }

        private static void ResetCardScale(RectTransform card)
        {
            if (card != null)
                card.localScale = Vector3.one;
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
                    view.Label.text = id.ToString();

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
