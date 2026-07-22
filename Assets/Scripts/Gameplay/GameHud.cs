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
            UiFactory.CreateButton(bottom.transform, "Menu", "Menu", new Vector2(-220f, 0f),
                () => SceneRouter.LoadMenu());
            UiFactory.CreateButton(bottom.transform, "Retry", "Retry", new Vector2(0f, 0f),
                () => SceneRouter.ReloadGame());

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
    }
}
