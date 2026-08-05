using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    /// <summary>
    /// Production UI chrome loaded from <c>Resources/UI/</c>.
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

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>(64);
        private static bool _loggedMissing;

        public static Sprite Button(ButtonStyle style, bool pressed = false, bool disabled = false)
        {
            string state = disabled ? "disabled" : pressed ? "pressed" : "normal";
            string key = style switch
            {
                ButtonStyle.Primary => $"UI/Chrome/ui_btn_primary_{state}",
                ButtonStyle.Danger => $"UI/Chrome/ui_btn_danger_{state}",
                ButtonStyle.Back => $"UI/Chrome/ui_btn_back_{state}",
                ButtonStyle.Pause => $"UI/Chrome/ui_btn_pause_{state}",
                _ => $"UI/Chrome/ui_btn_secondary_{state}"
            };
            return Load(key) ?? PlaceholderArt.WhiteSquare();
        }

        public static Sprite PanelLarge => Load("UI/Chrome/ui_panel_modal_large") ?? PlaceholderArt.WhiteSquare();
        public static Sprite PanelSmall => Load("UI/Chrome/ui_panel_modal_small") ?? PlaceholderArt.WhiteSquare();
        public static Sprite Backdrop => Load("UI/Chrome/ui_panel_backdrop") ?? PlaceholderArt.WhiteSquare();
        public static Sprite MissionPlate => Load("UI/Chrome/ui_hud_mission_plate") ?? PlaceholderArt.WhiteSquare();
        public static Sprite TimerOk => Load("UI/Chrome/ui_hud_timer_ok") ?? PlaceholderArt.Circle();
        public static Sprite TimerUrgent => Load("UI/Chrome/ui_hud_timer_urgent") ?? PlaceholderArt.Circle();
        public static Sprite RowBackground => Load("UI/Chrome/ui_row_bg") ?? PlaceholderArt.WhiteSquare();
        public static Sprite ProgressTrack => Load("UI/Chrome/ui_progress_track") ?? PlaceholderArt.WhiteSquare();
        public static Sprite ProgressFill => Load("UI/Chrome/ui_progress_fill") ?? PlaceholderArt.WhiteSquare();

        public static Sprite LevelUnlocked => Load("UI/Chrome/ui_level_unlocked") ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelLocked => Load("UI/Chrome/ui_level_locked") ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelSelected => Load("UI/Chrome/ui_level_selected") ?? PlaceholderArt.WhiteSquare();
        public static Sprite LevelCompleted => Load("UI/Chrome/ui_level_completed") ?? PlaceholderArt.WhiteSquare();

        public static Sprite ResultSuccess => Load("UI/Results/ui_result_success");
        public static Sprite ResultFail => Load("UI/Results/ui_result_fail");

        public static Sprite Icon(string name) => Load($"UI/Icons/{name}");

        public static Sprite Tutorial(string tipId)
        {
            if (string.IsNullOrEmpty(tipId))
                return null;
            return Load($"UI/Tutorials/tut_{tipId}");
        }

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

        private static Sprite Load(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
                return null;

            if (Cache.TryGetValue(resourcesPath, out var cached) && cached != null)
                return cached;

            var sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite == null && !_loggedMissing)
            {
                // One soft notice — Unity may still be importing on first open.
                Debug.LogWarning($"UiChrome: missing sprite Resources/{resourcesPath} (using placeholder until import).");
                _loggedMissing = true;
            }

            Cache[resourcesPath] = sprite;
            return sprite;
        }
    }
}
