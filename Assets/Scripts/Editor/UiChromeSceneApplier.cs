using DockIQ.Gameplay;
using DockIQ.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.Editor
{
    /// <summary>
    /// Bakes <see cref="UiChrome"/> sprites into Main Menu / Game scenes so they can be edited in the Inspector.
    /// </summary>
    public static class UiChromeSceneApplier
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string ChromeRoot = "Assets/Resources/UI";

        [MenuItem("DockIQ/Apply UI Chrome To Scenes")]
        public static void ApplyToScenes()
        {
            AssetDatabase.Refresh();
            if (!HasChromeAssets())
            {
                Debug.LogError("DockIQ: UI chrome missing under Assets/Resources/UI/. Generate chrome assets first.");
                return;
            }

            string activePath = EditorSceneManager.GetActiveScene().path;
            int applied = 0;
            applied += ApplyMainMenu() ? 1 : 0;
            applied += ApplyGame() ? 1 : 0;

            if (!string.IsNullOrEmpty(activePath))
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);

            Debug.Log($"DockIQ: Applied UI chrome to {applied} scene(s). Edit sprites/layout in the Inspector — runtime no longer re-skins.");
        }

        /// <summary>Bake chrome into the currently open Main Menu scene (does not save).</summary>
        public static void ApplyToOpenMainMenu()
        {
            var canvas = GameObject.Find("MenuCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("DockIQ: MenuCanvas not found — skipped chrome bake.");
                return;
            }

            ApplyUnder(canvas.transform);
            AttachModeIcon(FindButton(canvas.transform, "StoryButton"), "ui_icon_story");
            AttachModeIcon(FindButton(canvas.transform, "FreePlayButton"), "ui_icon_freeplay");
            AttachModeIcon(FindButton(canvas.transform, "AchievementsButton"), "ui_icon_achievements");
            AttachModeIcon(FindButton(canvas.transform, "HowToPlayButton"), "ui_icon_howto");
            ApplyLevelDefaults(canvas.transform);
        }

        /// <summary>Bake chrome into the currently open Game scene (does not save).</summary>
        public static void ApplyToOpenGame()
        {
            var hud = Object.FindFirstObjectByType<GameHud>();
            if (hud == null)
            {
                Debug.LogWarning("DockIQ: GameHud not found — skipped chrome bake.");
                return;
            }

            ApplyUnder(hud.transform);
            var so = new SerializedObject(hud);
            EnsureResultEmblem(hud, so);
            EnsureTutorialArt(hud, so);
            EnsureTimerBezel(hud, so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
        }

        public static bool ApplyMainMenu()
        {
            string path = $"{SceneFolder}/1_MainMenu.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                return false;

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ApplyToOpenMainMenu();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DockIQ: UI chrome baked into 1_MainMenu.");
            return true;
        }

        public static bool ApplyGame()
        {
            string path = $"{SceneFolder}/2_Game.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                return false;

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ApplyToOpenGame();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DockIQ: UI chrome baked into 2_Game.");
            return true;
        }

        private static bool HasChromeAssets() =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{ChromeRoot}/Chrome/ui_btn_primary_normal.png") != null;

        private static Sprite Load(string relativeUnderUi)
        {
            string path = $"{ChromeRoot}/{relativeUnderUi}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"DockIQ: Missing sprite {path}");
            return sprite;
        }

        private static void ApplyUnder(Transform root)
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
                    if (name.StartsWith("Level_"))
                        continue;
                    ApplyButton(img, btn, UiChrome.StyleForButtonName(name));
                    continue;
                }

                if (name == "Card" || name == "ResultPanel" || name == "PausePanel" || name == "TutorialPanel")
                    ApplyPanel(img, large: name == "Card" || name == "TutorialPanel");
                else if (name == "PauseBackdrop" || name == "TutorialBackdrop")
                    ApplyBackdrop(img);
                else if (name == "TopBar")
                {
                    img.sprite = Load("Chrome/ui_hud_mission_plate");
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }

                EditorUtility.SetDirty(img);
            }
        }

        private static void ApplyButton(Image image, Button button, UiChrome.ButtonStyle style)
        {
            string prefix = style switch
            {
                UiChrome.ButtonStyle.Primary => "Chrome/ui_btn_primary",
                UiChrome.ButtonStyle.Danger => "Chrome/ui_btn_danger",
                UiChrome.ButtonStyle.Back => "Chrome/ui_btn_back",
                UiChrome.ButtonStyle.Pause => "Chrome/ui_btn_pause",
                _ => "Chrome/ui_btn_secondary"
            };

            Sprite normal = Load($"{prefix}_normal");
            Sprite pressed = Load($"{prefix}_pressed");
            Sprite disabled = Load($"{prefix}_disabled");
            if (normal == null)
                return;

            image.sprite = normal;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var state = button.spriteState;
            state.pressedSprite = pressed;
            state.disabledSprite = disabled;
            state.highlightedSprite = normal;
            state.selectedSprite = normal;
            button.spriteState = state;
            button.transition = Selectable.Transition.SpriteSwap;
            button.targetGraphic = image;

            EditorUtility.SetDirty(image);
            EditorUtility.SetDirty(button);
        }

        private static void ApplyPanel(Image image, bool large)
        {
            Sprite sprite = Load(large ? "Chrome/ui_panel_modal_large" : "Chrome/ui_panel_modal_small");
            if (sprite == null)
                return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            EditorUtility.SetDirty(image);
        }

        private static void ApplyBackdrop(Image image)
        {
            Sprite sprite = Load("Chrome/ui_panel_backdrop");
            if (sprite == null)
                return;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
            EditorUtility.SetDirty(image);
        }

        private static void ApplyLevelDefaults(Transform root)
        {
            Sprite unlocked = Load("Chrome/ui_level_unlocked");
            if (unlocked == null)
                return;

            var buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (!buttons[i].name.StartsWith("Level_"))
                    continue;
                var img = buttons[i].GetComponent<Image>();
                if (img == null)
                    continue;
                img.sprite = unlocked;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                EditorUtility.SetDirty(img);
            }
        }

        private static Button FindButton(Transform root, string name)
        {
            var t = FindDeep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void AttachModeIcon(Button button, string iconFile)
        {
            if (button == null)
                return;

            Transform existing = button.transform.Find("ModeIcon");
            Image img;
            if (existing != null)
            {
                img = existing.GetComponent<Image>();
            }
            else
            {
                var go = new GameObject("ModeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(button.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(18f, 0f);
                rt.sizeDelta = new Vector2(64f, 64f);
                img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;

                var label = button.transform.Find("Label") as RectTransform;
                if (label != null)
                {
                    label.anchoredPosition = new Vector2(28f, 0f);
                    label.sizeDelta = new Vector2(Mathf.Max(200f, label.sizeDelta.x - 40f), label.sizeDelta.y);
                    EditorUtility.SetDirty(label);
                }
            }

            img.sprite = Load($"Icons/{iconFile}");
            img.color = Color.white;
            EditorUtility.SetDirty(img);
            EditorUtility.SetDirty(button);
        }

        private static void EnsureResultEmblem(GameHud hud, SerializedObject so)
        {
            var resultPanel = so.FindProperty("_resultPanel").objectReferenceValue as GameObject;
            if (resultPanel == null)
                return;

            Image emblem = so.FindProperty("_resultEmblem").objectReferenceValue as Image;
            if (emblem == null)
            {
                var existing = resultPanel.transform.Find("ResultEmblem");
                if (existing != null)
                    emblem = existing.GetComponent<Image>();
            }

            if (emblem == null)
            {
                var go = new GameObject("ResultEmblem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(resultPanel.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 85f);
                rt.sizeDelta = new Vector2(120f, 120f);
                emblem = go.GetComponent<Image>();
                emblem.preserveAspect = true;
                emblem.raycastTarget = false;

                var resultText = so.FindProperty("_resultText").objectReferenceValue as TextMeshProUGUI;
                if (resultText != null)
                {
                    resultText.rectTransform.anchoredPosition = new Vector2(0f, -10f);
                    EditorUtility.SetDirty(resultText);
                }
            }

            emblem.sprite = Load("Results/ui_result_success");
            emblem.color = Color.white;
            emblem.enabled = true;
            so.FindProperty("_resultEmblem").objectReferenceValue = emblem;
            EditorUtility.SetDirty(emblem);
        }

        private static void EnsureTutorialArt(GameHud hud, SerializedObject so)
        {
            var tutorialPanel = so.FindProperty("_tutorialPanel").objectReferenceValue as GameObject;
            if (tutorialPanel == null)
                return;

            Image art = so.FindProperty("_tutorialArt").objectReferenceValue as Image;
            if (art == null)
            {
                var existing = tutorialPanel.transform.Find("TutorialArt");
                if (existing != null)
                    art = existing.GetComponent<Image>();
            }

            var panelRt = tutorialPanel.transform as RectTransform;
            if (panelRt != null && panelRt.sizeDelta.y < 520f)
            {
                panelRt.sizeDelta = new Vector2(Mathf.Max(620f, panelRt.sizeDelta.x), 520f);
                EditorUtility.SetDirty(panelRt);
            }

            if (art == null)
            {
                var go = new GameObject("TutorialArt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(tutorialPanel.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 55f);
                rt.sizeDelta = new Vector2(180f, 180f);
                art = go.GetComponent<Image>();
                art.preserveAspect = true;
                art.raycastTarget = false;
            }

            art.sprite = Load("Tutorials/tut_mission_basics");
            art.color = Color.white;
            art.enabled = false;
            so.FindProperty("_tutorialArt").objectReferenceValue = art;

            var title = so.FindProperty("_tutorialTitle").objectReferenceValue as TextMeshProUGUI;
            if (title != null)
            {
                title.rectTransform.anchoredPosition = new Vector2(0f, 200f);
                EditorUtility.SetDirty(title);
            }

            var body = so.FindProperty("_tutorialBody").objectReferenceValue as TextMeshProUGUI;
            if (body != null)
            {
                body.rectTransform.anchoredPosition = new Vector2(0f, -95f);
                EditorUtility.SetDirty(body);
            }

            var gotIt = so.FindProperty("_tutorialGotItButton").objectReferenceValue as Button;
            if (gotIt != null)
            {
                var gotRt = gotIt.transform as RectTransform;
                if (gotRt != null)
                {
                    gotRt.anchoredPosition = new Vector2(0f, -200f);
                    EditorUtility.SetDirty(gotRt);
                }
            }

            EditorUtility.SetDirty(art);
        }

        private static void EnsureTimerBezel(GameHud hud, SerializedObject so)
        {
            var timerText = so.FindProperty("_timerText").objectReferenceValue as TextMeshProUGUI;
            if (timerText == null)
                return;

            Image bezel = so.FindProperty("_timerBezel").objectReferenceValue as Image;
            Transform parent = timerText.transform.parent;
            if (parent == null)
                return;

            if (bezel == null)
            {
                var existing = parent.Find("TimerBezel");
                if (existing != null)
                    bezel = existing.GetComponent<Image>();
            }

            if (bezel == null)
            {
                var go = new GameObject("TimerBezel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(parent, false);
                go.transform.SetSiblingIndex(timerText.transform.GetSiblingIndex());
                var rt = (RectTransform)go.transform;
                var src = timerText.rectTransform;
                rt.anchorMin = src.anchorMin;
                rt.anchorMax = src.anchorMax;
                rt.pivot = src.pivot;
                rt.anchoredPosition = src.anchoredPosition;
                rt.sizeDelta = new Vector2(Mathf.Max(120f, src.sizeDelta.x + 40f), Mathf.Max(72f, src.sizeDelta.y + 24f));
                bezel = go.GetComponent<Image>();
                bezel.raycastTarget = false;
                bezel.preserveAspect = true;
            }

            bezel.sprite = Load("Chrome/ui_hud_timer_ok");
            bezel.color = Color.white;
            so.FindProperty("_timerBezel").objectReferenceValue = bezel;
            EditorUtility.SetDirty(bezel);
        }
    }
}
