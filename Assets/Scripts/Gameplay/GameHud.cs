using System;
using DockIQ.Core;
using DockIQ.Levels;
using DockIQ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.Gameplay
{
    public sealed class GameHud : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _requestText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Image _timerBezel;

        [Header("Result Modal")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _retryResultButton;
        [SerializeField] private Button _menuResultButton;
        [SerializeField] private Image _resultEmblem;

        [Header("Pause Modal")]
        [SerializeField] private GameObject _pauseBackdrop;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseBackdropButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitToMenuButton;

        [Header("Tutorial Modal")]
        [SerializeField] private GameObject _tutorialBackdrop;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TextMeshProUGUI _tutorialTitle;
        [SerializeField] private TextMeshProUGUI _tutorialBody;
        [SerializeField] private Button _tutorialGotItButton;
        [SerializeField] private Image _tutorialArt;

        private Action _onPause;
        private Action _onResume;
        private Action _onRestart;
        private Action _onQuitToMenu;
        private Action _onTutorialDismissed;

        public bool IsTutorialOpen =>
            _tutorialPanel != null && _tutorialPanel.activeSelf;

        private void Awake()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPausePressed);
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNext);
            if (_retryResultButton != null)
                _retryResultButton.onClick.AddListener(SceneRouter.ReloadGame);
            if (_menuResultButton != null)
                _menuResultButton.onClick.AddListener(SceneRouter.LoadMenu);
            if (_pauseBackdropButton != null)
                _pauseBackdropButton.onClick.AddListener(OnResumePressed);
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumePressed);
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartPressed);
            if (_quitToMenuButton != null)
                _quitToMenuButton.onClick.AddListener(OnQuitPressed);
            if (_tutorialGotItButton != null)
                _tutorialGotItButton.onClick.AddListener(OnTutorialGotIt);

            HidePause();
            if (_resultPanel != null)
                _resultPanel.SetActive(false);
            // Tutorial/result panels stay inactive from scene authoring.
            // Do not HideTutorial() here — Begin can race Awake and would get wiped.
        }

        public void ConfigurePause(Action onPause, Action onResume, Action onRestart, Action onQuitToMenu)
        {
            _onPause = onPause;
            _onResume = onResume;
            _onRestart = onRestart;
            _onQuitToMenu = onQuitToMenu;
        }

        public bool ShowTutorial(string title, string body, Action onDismissed)
        {
            return ShowTutorial(title, body, null, onDismissed);
        }

        public bool ShowTutorial(string title, string body, string tipId, Action onDismissed)
        {
            if (_tutorialPanel == null)
            {
                Debug.LogWarning("GameHud: Tutorial panel is not assigned — tip will not be marked seen.");
                return false;
            }

            _onTutorialDismissed = onDismissed;
            if (_tutorialTitle != null)
                _tutorialTitle.text = title;
            if (_tutorialBody != null)
                _tutorialBody.text = body;

            if (_tutorialArt != null)
            {
                Sprite art = UiChrome.Tutorial(tipId);
                _tutorialArt.sprite = art;
                _tutorialArt.enabled = art != null;
            }

            HidePause();
            if (_pauseButton != null)
                _pauseButton.interactable = false;

            if (_tutorialBackdrop != null)
            {
                _tutorialBackdrop.SetActive(true);
                _tutorialBackdrop.transform.SetAsLastSibling();
            }

            _tutorialPanel.SetActive(true);
            _tutorialPanel.transform.SetAsLastSibling();
            return true;
        }

        public void HideTutorial()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);
            if (_tutorialBackdrop != null)
                _tutorialBackdrop.SetActive(false);

            if (_pauseButton != null && (_resultPanel == null || !_resultPanel.activeSelf))
                _pauseButton.interactable = true;
        }

        public void ShowRequest(string request, string title)
        {
            if (_titleText != null)
                _titleText.text = title;
            if (_requestText != null)
                _requestText.text = request;
        }

        public void SetTimer(float seconds)
        {
            if (_timerText == null)
                return;
            seconds = Mathf.Max(0f, seconds);
            int s = Mathf.CeilToInt(seconds);
            _timerText.text = $"{s / 60}:{s % 60:00}";
            bool urgent = seconds <= 8f;
            _timerText.color = urgent ? PlaceholderArt.DockWrong : PlaceholderArt.Hazard;

            if (_timerBezel != null)
            {
                Sprite bezel = urgent ? UiChrome.TimerUrgent : UiChrome.TimerOk;
                if (bezel != null)
                    _timerBezel.sprite = bezel;
                _timerBezel.enabled = _timerBezel.sprite != null;
            }
        }

        public void HideResult()
        {
            if (_resultPanel != null)
                _resultPanel.SetActive(false);
        }

        public void ShowResult(bool success, string message, bool hasNext)
        {
            if (_resultPanel == null)
                return;

            HidePause();
            HideTutorial();
            if (_pauseButton != null)
                _pauseButton.gameObject.SetActive(false);

            if (_resultEmblem != null)
            {
                _resultEmblem.sprite = success ? UiChrome.ResultSuccess : UiChrome.ResultFail;
                _resultEmblem.enabled = _resultEmblem.sprite != null;
                _resultEmblem.color = Color.white;
            }

            _resultPanel.SetActive(true);
            if (_resultText != null)
            {
                _resultText.text = success ? $"Success\n{message}" : $"Failed\n{message}";
                _resultText.color = success ? PlaceholderArt.DockGreen : PlaceholderArt.DockWrong;
            }
            if (_nextButton != null)
                _nextButton.gameObject.SetActive(success && hasNext);
        }

        private void OnNext()
        {
            int current = ProgressStore.GetSelectedLevel();
            int next = Mathf.Min(current + 1, LevelCatalog.Count);

            // Free Play only advances into already-unlocked levels.
            if (!GameSession.IsStory && !ProgressStore.IsUnlocked(next))
                return;

            ProgressStore.SetSelectedLevel(next);
            SceneRouter.ReloadGame();
        }

        private void OnPausePressed()
        {
            if (IsTutorialOpen)
                return;
            if (_pausePanel == null || _pausePanel.activeSelf)
                return;

            if (_pauseBackdrop != null)
                _pauseBackdrop.SetActive(true);
            _pausePanel.SetActive(true);
            _onPause?.Invoke();
        }

        private void OnResumePressed()
        {
            if (IsTutorialOpen)
                return;

            HidePause();
            _onResume?.Invoke();
        }

        private void OnRestartPressed() => _onRestart?.Invoke();

        private void OnQuitPressed() => _onQuitToMenu?.Invoke();

        private void OnTutorialGotIt()
        {
            HideTutorial();
            var dismiss = _onTutorialDismissed;
            _onTutorialDismissed = null;
            dismiss?.Invoke();
        }

        private void HidePause()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
            if (_pauseBackdrop != null)
                _pauseBackdrop.SetActive(false);
        }
    }
}
