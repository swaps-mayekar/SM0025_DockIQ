using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    /// <summary>
    /// Production UI chrome from <see cref="UiChromeCatalog"/> (<c>Assets/UI</c>).
    /// Falls back to <see cref="PlaceholderArt"/> when a sprite is missing.
    /// </summary>
    public static class UiChrome
    {
        public enum ButtonStyle
        {
            Primary,
            Secondary,
            Danger,
            Back,
            Pause
        }

        private const string CatalogPath = "Assets/UI/UiChromeCatalog.asset";
        private static UiChromeCatalog _catalog;

        public static void Bind(UiChromeCatalog catalog) => _catalog = catalog;

        public static UiChromeCatalog Catalog
        {
            get
            {
                if (_catalog == null)
                    TryAutoBind();
                return _catalog;
            }
        }

        public static Sprite Button(ButtonStyle style, bool pressed = false, bool disabled = false)
        {
            var catalog = Catalog;
            Sprite sprite = catalog != null ? catalog.Button(style, pressed, disabled) : null;
            return sprite != null ? sprite : PlaceholderArt.WhiteSquare();
        }

        public static Sprite PanelLarge => Catalog?.Panel ?? PlaceholderArt.WhiteSquare();
        public static Sprite PanelSmall => Catalog?.Panel ?? PlaceholderArt.WhiteSquare();
        public static Sprite Backdrop => Catalog?.Backdrop ?? Catalog?.Panel ?? PlaceholderArt.WhiteSquare();
        public static Sprite MissionPlate => Catalog?.MissionPlate ?? Catalog?.SecondaryNormal ?? PlaceholderArt.WhiteSquare();
        public static Sprite TimerOk => null;
        public static Sprite TimerUrgent => null;
        public static Sprite RowBackground => Catalog?.RowBackground ?? PlaceholderArt.WhiteSquare();
        public static Sprite ProgressTrack => PlaceholderArt.WhiteSquare();
        public static Sprite ProgressFill => PlaceholderArt.WhiteSquare();

        public static Sprite LevelUnlocked => Catalog?.LevelUnlocked ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelLocked => Catalog?.LevelLocked ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelSelected => Catalog?.LevelSelected ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelCompleted => Catalog?.LevelCompleted ?? PlaceholderArt.WhiteSquare();

        public static Sprite ResultSuccess => null;
        public static Sprite ResultFail => null;

        public static Sprite GameLogo => Catalog?.GameLogo;

        /// <summary>Optional icon lookup — no Resources pack; returns null unless catalog grows icons.</summary>
        public static Sprite Icon(string name) => null;

        /// <summary>Optional tutorial art — not shipped under Assets/UI yet.</summary>
        public static Sprite Tutorial(string tipId) => null;

        public static void ApplyButton(Image image, Button button, ButtonStyle style)
        {
            if (image == null)
                return;

            Sprite normal = Button(style);
            image.sprite = normal;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = 1f;

            if (button == null)
                return;

            var spriteState = button.spriteState;
            spriteState.pressedSprite = Button(style, pressed: true);
            spriteState.disabledSprite = Button(style, disabled: true);
            spriteState.highlightedSprite = normal;
            spriteState.selectedSprite = normal;
            button.spriteState = spriteState;
            button.transition = Selectable.Transition.SpriteSwap;
            button.targetGraphic = image;
        }

        public static void ApplyPanel(Image image, bool large = true)
        {
            if (image == null)
                return;
            image.sprite = large ? PanelLarge : PanelSmall;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        public static void ApplyBackdrop(Image image)
        {
            if (image == null)
                return;
            image.sprite = Backdrop;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
        }

        public static void ApplyLevelTile(Image image, bool unlocked, bool selected = false, bool completed = false)
        {
            if (image == null)
                return;

            if (!unlocked)
                image.sprite = LevelLocked;
            else if (selected)
                image.sprite = LevelSelected;
            else if (completed)
                image.sprite = LevelCompleted;
            else
                image.sprite = LevelUnlocked;

            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        public static ButtonStyle StyleForButtonName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return ButtonStyle.Secondary;

            string n = name.ToLowerInvariant();
            if (n.Contains("story") || n.Contains("gotit") || n.Contains("got_it") || n.Contains("next"))
                return ButtonStyle.Primary;
            if (n.Contains("quit") || n.Contains("fail"))
                return ButtonStyle.Danger;
            if (n.Contains("back") || n.Contains("menu"))
                return ButtonStyle.Back;
            if (n.Contains("pause"))
                return ButtonStyle.Pause;
            if (n.Contains("retry") || n.Contains("restart"))
                return ButtonStyle.Danger;
            return ButtonStyle.Secondary;
        }

        /// <summary>Skins common named UI under a root (menu or HUD canvases).</summary>
        public static void ApplyUnder(Transform root)
        {
            if (root == null)
                return;

            var images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i];
                if (img == null)
                    continue;

                string name = img.gameObject.name;
                var btn = img.GetComponent<Button>();
                if (btn != null)
                {
                    // Level tiles keep number labels — use level chrome instead of text buttons.
                    if (name.StartsWith("Level_"))
                        continue;
                    ApplyButton(img, btn, StyleForButtonName(name));
                    continue;
                }

                if (name == "Card" || name == "ResultPanel" || name == "PausePanel" || name == "TutorialPanel")
                {
                    bool large = name == "Card" || name == "TutorialPanel";
                    ApplyPanel(img, large);
                }
                else if (name == "PauseBackdrop" || name == "TutorialBackdrop")
                {
                    ApplyBackdrop(img);
                }
                else if (name == "TopBar")
                {
                    img.sprite = MissionPlate;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
            }
        }

        private static void TryAutoBind()
        {
#if UNITY_EDITOR
            _catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<UiChromeCatalog>(CatalogPath);
#endif
        }
    }
}
