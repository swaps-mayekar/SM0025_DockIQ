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
    /// Sources sprites exclusively from <c>Assets/UI</c> via <see cref="UiChromeCatalog"/>.
    /// </summary>
    public static class UiChromeSceneApplier
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string CatalogPath = "Assets/UI/UiChromeCatalog.asset";

        [MenuItem("DockIQ/Apply UI Chrome To Scenes")]
        public static void ApplyToScenes()
        {
            AssetDatabase.Refresh();
            if (!HasChromeAssets())
            {
                Debug.LogError("DockIQ: UI chrome missing under Assets/UI/. Run DockIQ/Import UI Chrome Catalog first.");
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
            EnsureCatalogBound();
            var canvas = GameObject.Find("MenuCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("DockIQ: MenuCanvas not found — skipped chrome bake.");
                return;
            }

            ApplyUnder(canvas.transform);
            ApplyLevelDefaults(canvas.transform);
            ClearLegacyModeIcons(canvas.transform);
        }

        /// <summary>Bake chrome into the currently open Game scene (does not save).</summary>
        public static void ApplyToOpenGame()
        {
            EnsureCatalogBound();
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

        private static bool HasChromeAssets()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<UiChromeCatalog>(CatalogPath);
            return catalog != null && catalog.PrimaryNormal != null;
        }

        private static void EnsureCatalogBound()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<UiChromeCatalog>(CatalogPath);
            if (catalog != null)
                UiChrome.Bind(catalog);
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
                    img.sprite = UiChrome.MissionPlate;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }

                EditorUtility.SetDirty(img);
            }
        }

        private static void ApplyButton(Image image, Button button, UiChrome.ButtonStyle style)
        {
            Sprite normal = UiChrome.Button(style);
            if (normal == null || normal.name == "PlaceholderWhite")
                return;

            image.sprite = normal;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var state = button.spriteState;
            state.pressedSprite = UiChrome.Button(style, pressed: true);
            state.disabledSprite = UiChrome.Button(style, disabled: true);
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
            Sprite sprite = large ? UiChrome.PanelLarge : UiChrome.PanelSmall;
            if (sprite == null || sprite.name == "PlaceholderWhite")
                return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            EditorUtility.SetDirty(image);
        }

        private static void ApplyBackdrop(Image image)
        {
            Sprite sprite = UiChrome.Backdrop;
            if (sprite == null || sprite.name == "PlaceholderWhite")
                return;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = new Color(0f, 0f, 0f, 0.65f);
            image.preserveAspect = false;
            EditorUtility.SetDirty(image);
        }

        private static void ApplyLevelDefaults(Transform root)
        {
            Sprite unlocked = UiChrome.LevelUnlocked;
            if (unlocked == null || unlocked.name == "PlaceholderWhite")
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

        private static void ClearLegacyModeIcons(Transform root)
        {
            // Old Resources/UI/Icons chrome — remove baked ModeIcon children so menus rely on Button.png only.
            string[] buttons = { "StoryButton", "FreePlayButton", "AchievementsButton", "HowToPlayButton" };
            for (int i = 0; i < buttons.Length; i++)
            {
                var t = FindDeep(root, buttons[i]);
                if (t == null)
                    continue;
                var icon = t.Find("ModeIcon");
                if (icon != null)
                    Object.DestroyImmediate(icon.gameObject);
            }
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
                return;

            // No dedicated result sprites under Assets/UI yet — clear Resources refs.
            emblem.sprite = null;
            emblem.enabled = false;
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

            if (art == null)
                return;

            art.sprite = null;
            art.enabled = false;
            so.FindProperty("_tutorialArt").objectReferenceValue = art;
            EditorUtility.SetDirty(art);
        }

        private static void EnsureTimerBezel(GameHud hud, SerializedObject so)
        {
            Image bezel = so.FindProperty("_timerBezel").objectReferenceValue as Image;
            if (bezel == null)
                return;

            // No dedicated timer chrome under Assets/UI yet — clear Resources refs.
            bezel.sprite = null;
            bezel.enabled = false;
            so.FindProperty("_timerBezel").objectReferenceValue = bezel;
            EditorUtility.SetDirty(bezel);
        }
    }
}
