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

        [Header("Result Modal")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _retryResultButton;
        [SerializeField] private Button _menuResultButton;

        [Header("Pause Modal")]
        [SerializeField] private GameObject _pauseBackdrop;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseBackdropButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitToMenuButton;

        private Action _onPause;
        private Action _onResume;
        private Action _onRestart;
        private Action _onQuitToMenu;

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

            HidePause();
            if (_resultPanel != null)
                _resultPanel.SetActive(false);
        }

        public void ConfigurePause(Action onPause, Action onResume, Action onRestart, Action onQuitToMenu)
        {
            _onPause = onPause;
            _onResume = onResume;
            _onRestart = onRestart;
            _onQuitToMenu = onQuitToMenu;
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
            _timerText.color = seconds <= 8f ? PlaceholderArt.DockWrong : PlaceholderArt.Hazard;
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
            if (_pauseButton != null)
                _pauseButton.gameObject.SetActive(false);

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
            ProgressStore.SetSelectedLevel(next);
            SceneRouter.ReloadGame();
        }

        private void OnPausePressed()
        {
            if (_pausePanel == null || _pausePanel.activeSelf)
                return;

            if (_pauseBackdrop != null)
                _pauseBackdrop.SetActive(true);
            _pausePanel.SetActive(true);
            _onPause?.Invoke();
        }

        private void OnResumePressed()
        {
            HidePause();
            _onResume?.Invoke();
        }

        private void OnRestartPressed() => _onRestart?.Invoke();

        private void OnQuitPressed() => _onQuitToMenu?.Invoke();

        private void HidePause()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
            if (_pauseBackdrop != null)
                _pauseBackdrop.SetActive(false);
        }
    }
}
