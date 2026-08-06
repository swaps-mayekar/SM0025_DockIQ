using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Direct references to UI chrome sprites under <c>Assets/UI</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "UiChromeCatalog", menuName = "DockIQ/UI Chrome Catalog")]
    public sealed class UiChromeCatalog : ScriptableObject
    {
        [Header("Buttons (Button.png slices)")]
        [SerializeField] private Sprite _primaryNormal;
        [SerializeField] private Sprite _primaryPressed;
        [SerializeField] private Sprite _primaryDisabled;
        [SerializeField] private Sprite _secondaryNormal;
        [SerializeField] private Sprite _secondaryPressed;
        [SerializeField] private Sprite _secondaryDisabled;
        [SerializeField] private Sprite _dangerNormal;
        [SerializeField] private Sprite _dangerPressed;
        [SerializeField] private Sprite _dangerDisabled;

        [Header("Panels")]
        [SerializeField] private Sprite _panel;
        [SerializeField] private Sprite _backdrop;
        [SerializeField] private Sprite _missionPlate;
        [SerializeField] private Sprite _rowBackground;

        [Header("Level tiles")]
        [SerializeField] private Sprite _levelUnlocked;
        [SerializeField] private Sprite _levelLocked;
        [SerializeField] private Sprite _levelSelected;
        [SerializeField] private Sprite _levelCompleted;

        [Header("Brand")]
        [SerializeField] private Sprite _gameLogo;

        public Sprite PrimaryNormal => _primaryNormal;
        public Sprite PrimaryPressed => _primaryPressed;
        public Sprite PrimaryDisabled => _primaryDisabled;
        public Sprite SecondaryNormal => _secondaryNormal;
        public Sprite SecondaryPressed => _secondaryPressed;
        public Sprite SecondaryDisabled => _secondaryDisabled;
        public Sprite DangerNormal => _dangerNormal;
        public Sprite DangerPressed => _dangerPressed;
        public Sprite DangerDisabled => _dangerDisabled;
        public Sprite Panel => _panel;
        public Sprite Backdrop => _backdrop;
        public Sprite MissionPlate => _missionPlate;
        public Sprite RowBackground => _rowBackground;
        public Sprite LevelUnlocked => _levelUnlocked;
        public Sprite LevelLocked => _levelLocked;
        public Sprite LevelSelected => _levelSelected;
        public Sprite LevelCompleted => _levelCompleted;
        public Sprite GameLogo => _gameLogo;

        public Sprite Button(UiChrome.ButtonStyle style, bool pressed = false, bool disabled = false)
        {
            return style switch
            {
                UiChrome.ButtonStyle.Primary => Pick(_primaryNormal, _primaryPressed, _primaryDisabled, pressed, disabled),
                UiChrome.ButtonStyle.Danger => Pick(_dangerNormal, _dangerPressed, _dangerDisabled, pressed, disabled),
                UiChrome.ButtonStyle.Back => Pick(_secondaryNormal, _secondaryPressed, _secondaryDisabled, pressed, disabled),
                UiChrome.ButtonStyle.Pause => Pick(_secondaryNormal, _secondaryPressed, _secondaryDisabled, pressed, disabled),
                _ => Pick(_secondaryNormal, _secondaryPressed, _secondaryDisabled, pressed, disabled)
            };
        }

        private static Sprite Pick(Sprite normal, Sprite pressedSprite, Sprite disabledSprite, bool pressed, bool disabled)
        {
            if (disabled)
                return disabledSprite != null ? disabledSprite : normal;
            if (pressed)
                return pressedSprite != null ? pressedSprite : normal;
            return normal;
        }
    }
}
