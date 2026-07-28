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
        private TextMeshProUGUI _requestText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _titleText;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;
        private Button _nextButton;
        private Button _pauseButton;
        private GameObject _pauseBackdrop;
        private GameObject _pausePanel;

        private Action _onPause;
        private Action _onResume;
        private Action _onRestart;
        private Action _onQuitToMenu;

        public void Build()
        {
            var canvas = UiFactory.CreateCanvas(transform, "GameHUD");
            var safe = UiFactory.CreateSafeArea(canvas.transform);

            var top = UiFactory.CreatePanel(safe, "TopBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(680f, 160f), PlaceholderArt.Panel);
            _titleText = UiFactory.CreateText(top.transform, "Title", "DockIQ", 28, FontStyles.Bold,
                new Vector2(0f, 50f), new Vector2(640f, 40f));
            _requestText = UiFactory.CreateText(top.transform, "Request", "", 22, FontStyles.Normal,
                new Vector2(0f, 8f), new Vector2(640f, 50f));
            _timerText = UiFactory.CreateText(top.transform, "Timer", "0:00", 32, FontStyles.Bold,
                new Vector2(0f, -48f), new Vector2(200f, 40f));
            _timerText.color = PlaceholderArt.Hazard;

            var bottom = UiFactory.CreatePanel(safe, "BottomBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(680f, 80f), Color.clear);
            _pauseButton = UiFactory.CreateButton(bottom.transform, "Pause", "Pause", new Vector2(0f, 0f),
                OnPausePressed);

            _resultPanel = UiFactory.CreatePanel(safe, "Result", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(560f, 320f), PlaceholderArt.Panel).gameObject;
            _resultText = UiFactory.CreateText(_resultPanel.transform, "ResultText", "", 34, FontStyles.Bold,
                new Vector2(0f, 60f), new Vector2(500f, 80f));
            _nextButton = UiFactory.CreateButton(_resultPanel.transform, "Next", "Next Level", new Vector2(0f, -20f),
                OnNext);
            UiFactory.CreateButton(_resultPanel.transform, "RetryResult", "Retry", new Vector2(0f, -100f),
                () => SceneRouter.ReloadGame());
            UiFactory.CreateButton(_resultPanel.transform, "MenuResult", "Menu", new Vector2(0f, -180f),
                () => SceneRouter.LoadMenu());

            _resultPanel.SetActive(false);

            _pauseBackdrop = UiFactory.CreatePanel(safe, "PauseBackdrop", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.35f)).gameObject;
            var backdropRt = (RectTransform)_pauseBackdrop.transform;
            UiFactory.StretchFull(backdropRt);
            var backdropButton = _pauseBackdrop.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.onClick.AddListener(OnResumePressed);
            _pauseBackdrop.SetActive(false);

            _pausePanel = UiFactory.CreatePanel(safe, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(560f, 380f), PlaceholderArt.Panel).gameObject;
            UiFactory.CreateText(_pausePanel.transform, "PauseTitle", "Paused", 34, FontStyles.Bold,
                new Vector2(0f, 120f), new Vector2(500f, 80f));
            UiFactory.CreateButton(_pausePanel.transform, "Resume", "Resume", new Vector2(0f, 45f),
                OnResumePressed);
            UiFactory.CreateButton(_pausePanel.transform, "Restart", "Restart", new Vector2(0f, -45f),
                OnRestartPressed);
            UiFactory.CreateButton(_pausePanel.transform, "QuitToMenu", "Quit to Menu", new Vector2(0f, -135f),
                OnQuitPressed);
            _pausePanel.SetActive(false);
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
            _resultText.text = success ? $"Success\n{message}" : $"Failed\n{message}";
            _resultText.color = success ? PlaceholderArt.DockGreen : PlaceholderArt.DockWrong;
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
