using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    /// <summary>
    /// Scene-authored achievement row. Layout, copy, and chrome live in the scene;
    /// runtime only toggles locked/unlocked presentation.
    /// </summary>
    public sealed class AchievementRowView : MonoBehaviour
    {
        [SerializeField] private string _achievementId;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _body;
        [SerializeField] [TextArea(2, 4)] private string _unlockedBody;
        [SerializeField] [TextArea(2, 4)] private string _lockedBody;

        public string AchievementId => _achievementId;

        public void Configure(string id, string title, string unlockedBody, string lockedBody, Sprite icon)
        {
            _achievementId = id;
            _unlockedBody = unlockedBody;
            _lockedBody = lockedBody;

            if (_title != null)
                _title.text = title;
            if (_icon != null && icon != null)
                _icon.sprite = icon;
        }

        public void SetUnlocked(bool unlocked)
        {
            if (_body != null)
                _body.text = unlocked ? _unlockedBody : _lockedBody;

            if (_title != null)
                _title.color = unlocked ? PlaceholderArt.Hazard : new Color(0.65f, 0.68f, 0.72f, 1f);

            if (_body != null)
                _body.color = unlocked ? PlaceholderArt.Text : new Color(0.55f, 0.58f, 0.62f, 1f);

            if (_background != null)
                _background.color = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.58f, 0.9f);

            if (_icon != null)
                _icon.color = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.38f, 0.85f);
        }
    }
}
