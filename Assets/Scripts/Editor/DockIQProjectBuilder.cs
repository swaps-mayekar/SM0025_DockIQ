using System;
using System.Collections.Generic;
using System.IO;
using DockIQ.Core;
using DockIQ.Gameplay;
using DockIQ.Levels;
using DockIQ.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DockIQ.Editor
{
    public static class DockIQProjectBuilder
    {
        private const string SceneFolder = "Assets/Scenes";

        [InitializeOnLoadMethod]
        private static void BuildOnceAfterCompile()
        {
            if (Application.isBatchMode || SessionState.GetBool("DockIQ.ContentBuilt", false))
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/1_MainMenu.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/2_Game.unity") != null)
                return;

            SessionState.SetBool("DockIQ.ContentBuilt", true);
            EditorApplication.delayCall += Build;
        }

        [MenuItem("DockIQ/Build Game Content")]
        public static void Build()
        {
            BuildInternal(overwriteExistingScenes: false);
        }

        [MenuItem("DockIQ/Force Rebuild Scenes (Wipes Scene Edits)")]
        public static void ForceRebuildScenes()
        {
            if (!EditorUtility.DisplayDialog(
                    "Force Rebuild Scenes",
                    "This will overwrite Splash, Main Menu, and Game scenes and wipe any manual edits.\n\nContinue?",
                    "Overwrite Scenes",
                    "Cancel"))
                return;

            BuildInternal(overwriteExistingScenes: true);
        }

        [MenuItem("DockIQ/Ensure Tutorial UI In Game Scene")]
        public static void EnsureTutorialUiInGameScene()
        {
            string scenePath = SceneFolder + "/2_Game.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogError($"DockIQ: Missing scene {scenePath}. Run Build Game Content first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHud>();
            if (hud == null)
            {
                Debug.LogError("DockIQ: GameHud not found in game scene.");
                return;
            }

            var so = new SerializedObject(hud);
            var existingPanel = so.FindProperty("_tutorialPanel").objectReferenceValue as GameObject;
            if (existingPanel != null)
            {
                Debug.Log("DockIQ: Tutorial UI already present — left scene edits untouched.");
                return;
            }

            var safe = GameObject.Find("SafeArea");
            if (safe == null)
            {
                Debug.LogError("DockIQ: SafeArea not found in game scene.");
                return;
            }

            CreateAndWireTutorialUi(safe.transform, hud);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DockIQ: Tutorial UI added to existing game scene (other edits preserved).");
        }

        [MenuItem("DockIQ/Clear Tutorial Progress")]
        public static void ClearTutorialProgress()
        {
            ProgressStore.ClearTutorialTips();
            Debug.Log("DockIQ: Tutorial tip progress cleared. Tips will show again on next play.");
        }

        [MenuItem("DockIQ/Ensure Main Menu Modes")]
        public static void EnsureMainMenuModes()
        {
            string scenePath = SceneFolder + "/1_MainMenu.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogError($"DockIQ: Missing scene {scenePath}. Run Build Game Content first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            if (menu == null)
            {
                Debug.LogError("DockIQ: MainMenuController not found in main menu scene.");
                return;
            }

            var canvas = GameObject.Find("MenuCanvas");
            if (canvas == null)
            {
                Debug.LogError("DockIQ: MenuCanvas not found in main menu scene.");
                return;
            }

            var canvasRt = (RectTransform)canvas.transform;
            var playPanel = FindInSceneIncludingInactive(scene, "Panel_Play");
            if (playPanel == null)
            {
                // Fallback: search all loaded transforms (includes inactive).
                var transforms = UnityEngine.Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < transforms.Length; i++)
                {
                    if (transforms[i] != null && transforms[i].name == "Panel_Play" &&
                        transforms[i].gameObject.scene.path == scenePath)
                    {
                        playPanel = transforms[i].gameObject;
                        break;
                    }
                }
            }

            if (playPanel == null)
            {
                var roots = scene.GetRootGameObjects();
                var names = new System.Text.StringBuilder();
                for (int i = 0; i < roots.Length; i++)
                    names.Append(roots[i].name).Append(i + 1 < roots.Length ? ", " : "");
                Debug.LogError("DockIQ: Panel_Play not found after opening main menu. Roots: " + names);
                return;
            }

            var so = new SerializedObject(menu);
            var existingHome = so.FindProperty("_homePanel").objectReferenceValue as GameObject;
            if (existingHome != null)
            {
                Debug.Log("DockIQ: Main menu modes already present — left scene edits untouched.");
                return;
            }

            // Hide the old primary Play button inside Free Play; Story is the home CTA.
            var legacyPlay = playPanel.transform.Find("PlayButton");
            if (legacyPlay != null)
                legacyPlay.gameObject.SetActive(false);

            var levelsLabel = playPanel.transform.Find("LevelsLabel");
            if (levelsLabel != null)
            {
                var labelTmp = levelsLabel.GetComponent<TextMeshProUGUI>();
                if (labelTmp != null)
                    labelTmp.text = "Free Play";
            }

            var playBack = CreateButton("BackButton", playPanel.transform, "Back",
                new Vector2(0f, 820f), new Vector2(220f, 72f));

            var homePanel = CreateFullScreenPanel("Panel_Home", canvasRt);
            homePanel.transform.SetSiblingIndex(playPanel.transform.GetSiblingIndex());

            CreateText("Tagline", homePanel.transform, "WAREHOUSE RESCUE", 34, FontStyles.Bold,
                new Vector2(0f, 760f), new Vector2(900f, 50f), PlaceholderArt.Hazard);
            var storyProgress = CreateText("StoryProgress", homePanel.transform, "Story Progress  0/48", 26,
                FontStyles.Normal, new Vector2(0f, 680f), new Vector2(900f, 40f), PlaceholderArt.Text);
            var storyMission = CreateText("StoryMission", homePanel.transform, "Continue: Level 1", 24,
                FontStyles.Normal, new Vector2(0f, 620f), new Vector2(960f, 50f), PlaceholderArt.Text);

            var storyBtn = CreateButton("StoryButton", homePanel.transform, "Story Mode",
                new Vector2(0f, 480f), new Vector2(420f, 90f));
            storyBtn.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.35f, 1f);

            var freePlayBtn = CreateButton("FreePlayButton", homePanel.transform, "Free Play",
                new Vector2(0f, 360f), new Vector2(420f, 90f));
            var achievementsBtn = CreateButton("AchievementsButton", homePanel.transform, "Achievements",
                new Vector2(0f, 240f), new Vector2(420f, 90f));
            var howToBtn = CreateButton("HowToPlayButton", homePanel.transform, "How to Play",
                new Vector2(0f, 120f), new Vector2(420f, 90f));

            CreateText("HomeHint", homePanel.transform,
                "Story advances the rescue campaign. Free Play replays unlocked levels.",
                20, FontStyles.Normal, new Vector2(0f, -40f), new Vector2(920f, 80f), PlaceholderArt.Text);

            var achievementsPanel = CreateModalPanel("Panel_Achievements", canvasRt, "Achievements",
                "Achievements coming soon.\n\nComplete Story rescues to unlock future badges for perfect runs, decoy-free clears, and full-yard mastery.",
                out Button achievementsBack);

            var howToPanel = CreateScrollModalPanel("Panel_HowToPlay", canvasRt, "How to Play",
                out TextMeshProUGUI howToBody, out Button howToBack);

            playPanel.SetActive(false);
            achievementsPanel.SetActive(false);
            howToPanel.SetActive(false);
            homePanel.SetActive(true);

            so.FindProperty("_homePanel").objectReferenceValue = homePanel;
            so.FindProperty("_storyButton").objectReferenceValue = storyBtn;
            so.FindProperty("_freePlayButton").objectReferenceValue = freePlayBtn;
            so.FindProperty("_achievementsButton").objectReferenceValue = achievementsBtn;
            so.FindProperty("_howToPlayButton").objectReferenceValue = howToBtn;
            so.FindProperty("_storyProgressText").objectReferenceValue = storyProgress;
            so.FindProperty("_storyMissionText").objectReferenceValue = storyMission;
            so.FindProperty("_playPanel").objectReferenceValue = playPanel;
            so.FindProperty("_playBackButton").objectReferenceValue = playBack;
            so.FindProperty("_achievementsPanel").objectReferenceValue = achievementsPanel;
            so.FindProperty("_achievementsBackButton").objectReferenceValue = achievementsBack;
            so.FindProperty("_howToPlayPanel").objectReferenceValue = howToPanel;
            so.FindProperty("_howToPlayBackButton").objectReferenceValue = howToBack;
            so.FindProperty("_howToPlayBody").objectReferenceValue = howToBody;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("DockIQ: Main menu modes added (MenuBG / GameLogo preserved).");
        }

        private static GameObject FindInSceneIncludingInactive(UnityEngine.SceneManagement.Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var found = FindChildRecursive(roots[i].transform, name);
                if (found != null)
                    return found.gameObject;
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        [MenuItem("DockIQ/Ensure Board Art References")]
        public static void EnsureBoardArtInGameScene()
        {
            const string scenePath = SceneFolder + "/2_Game.unity";
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                Debug.LogError($"DockIQ: Missing scene {scenePath}. Run Build Game Content first.");
                return;
            }

            var catalog = EnsureBoardArtCatalog();
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<LevelController>();
            if (controller == null)
            {
                Debug.LogError("DockIQ: LevelController not found in game scene.");
                return;
            }

            var so = new SerializedObject(controller);
            var artProp = so.FindProperty("_boardArt");
            if (artProp == null)
            {
                Debug.LogError("DockIQ: LevelController is missing _boardArt field.");
                return;
            }

            if (artProp.objectReferenceValue == catalog)
            {
                Debug.Log("DockIQ: Board art already assigned — refreshed sprite arrays on catalog.");
            }
            else
            {
                artProp.objectReferenceValue = catalog;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("DockIQ: Board art catalog assigned to LevelController.");
            }

            RemoveLegacyUiResources();
        }

        private static void BuildInternal(bool overwriteExistingScenes)
        {
            try
            {
                EnsureFolders();
                EnsureLogoResource();
                EnsureBoardArtCatalog();
                EnsureScene($"{SceneFolder}/0_SplashScene.unity", BuildSplashScene, overwriteExistingScenes);
                EnsureScene($"{SceneFolder}/1_MainMenu.unity", BuildMenuScene, overwriteExistingScenes);
                EnsureScene($"{SceneFolder}/2_Game.unity", BuildGameScene, overwriteExistingScenes);

                // Non-destructive: if game scene was kept, still add tutorial UI when missing.
                if (!overwriteExistingScenes &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/2_Game.unity") != null)
                    EnsureTutorialUiInGameScene();

                // Non-destructive: upgrade existing main menu to mode hub when missing.
                if (!overwriteExistingScenes &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/1_MainMenu.unity") != null)
                    EnsureMainMenuModes();

                // Non-destructive: wire board art references on existing game scene.
                if (!overwriteExistingScenes &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/2_Game.unity") != null)
                    EnsureBoardArtInGameScene();
                else
                    RemoveLegacyUiResources();

                ConfigureBuildSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log(overwriteExistingScenes
                    ? "DOCKIQ_FORCE_REBUILD_COMPLETE"
                    : "DOCKIQ_BUILD_COMPLETE");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        private static void EnsureScene(string scenePath, Action buildScene, bool overwriteExistingScenes)
        {
            bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null;
            if (exists && !overwriteExistingScenes)
            {
                Debug.Log($"DockIQ: Keeping existing scene edits → {scenePath}");
                return;
            }

            buildScene();
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/Scripts",
                "Assets/Prefabs",
                "Assets/Resources",
                "Assets/Sprites/Belts",
                "Assets/Sprites/Devices",
                "Assets/Sprites/Parcels",
                "Assets/Sprites/Docks",
                "Assets/Sprites/UI",
                SceneFolder
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                    string name = Path.GetFileName(folder);
                    if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
                        AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static void EnsureLogoResource()
        {
            const string src = "Assets/UI/GameLogo.png";
            const string dst = "Assets/Resources/GameLogo.png";
            if (File.Exists(src) && !File.Exists(dst))
                AssetDatabase.CopyAsset(src, dst);

            var importer = AssetImporter.GetAtPath(dst) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }

        private const string BoardArtPath = "Assets/UI/BoardArtCatalog.asset";
        private const string ParcelsTexturePath = "Assets/UI/Parcels.png";
        private const string GatesTexturePath = "Assets/UI/Gates.png";

        private static BoardArtCatalog EnsureBoardArtCatalog()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI"))
                AssetDatabase.CreateFolder("Assets", "UI");

            var catalog = AssetDatabase.LoadAssetAtPath<BoardArtCatalog>(BoardArtPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<BoardArtCatalog>();
                AssetDatabase.CreateAsset(catalog, BoardArtPath);
            }

            var so = new SerializedObject(catalog);
            AssignSortedSprites(so.FindProperty("_parcels"), ParcelsTexturePath);
            AssignSortedSprites(so.FindProperty("_gates"), GatesTexturePath);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static void AssignSortedSprites(SerializedProperty arrayProp, string texturePath)
        {
            if (arrayProp == null)
                return;

            var sprites = LoadSortedSprites(texturePath);
            arrayProp.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static Sprite[] LoadSortedSprites(string texturePath)
        {
            if (!File.Exists(texturePath))
                return Array.Empty<Sprite>();

            var assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            var sprites = new List<Sprite>(assets.Length);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    sprites.Add(sprite);
            }

            sprites.Sort((a, b) => SliceIndex(a.name).CompareTo(SliceIndex(b.name)));
            return sprites.ToArray();
        }

        private static int SliceIndex(string name)
        {
            if (string.IsNullOrEmpty(name))
                return int.MaxValue;

            int underscore = name.LastIndexOf('_');
            if (underscore >= 0 && underscore + 1 < name.Length &&
                int.TryParse(name.Substring(underscore + 1), out int index))
                return index;

            return int.MaxValue;
        }

        private static void RemoveLegacyUiResources()
        {
            string[] legacy =
            {
                "Assets/Resources/UI/Parcels.png",
                "Assets/Resources/UI/Gates.png"
            };

            bool removed = false;
            for (int i = 0; i < legacy.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(legacy[i]) == null && !File.Exists(legacy[i]))
                    continue;

                if (AssetDatabase.DeleteAsset(legacy[i]))
                    removed = true;
            }

            if (AssetDatabase.IsValidFolder("Assets/Resources/UI"))
            {
                string[] remaining = AssetDatabase.FindAssets(string.Empty, new[] { "Assets/Resources/UI" });
                if (remaining == null || remaining.Length == 0)
                {
                    AssetDatabase.DeleteAsset("Assets/Resources/UI");
                    removed = true;
                }
            }

            if (removed)
                Debug.Log("DockIQ: Removed legacy Resources/UI parcel and gate copies.");
        }

        private static void BuildSplashScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(PlaceholderArt.Navy);
            CreateEventSystem();

            var canvas = CreateCanvas("SplashCanvas");
            var safe = CreateSafeArea(canvas.transform);

            var splashGo = new GameObject("Splash", typeof(SplashController));
            var splash = splashGo.GetComponent<SplashController>();

            var logo = CreateImage("Logo", safe, new Vector2(0.5f, 0.55f), new Vector2(720f, 720f), Color.white);
            logo.preserveAspect = true;
            CreateText("Tag", safe, "WAREHOUSE RESCUE", 36, FontStyles.Bold, new Vector2(0f, -420f),
                new Vector2(800f, 60f), PlaceholderArt.Hazard);

            var so = new SerializedObject(splash);
            so.FindProperty("_logoImage").objectReferenceValue = logo;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/0_SplashScene.unity");
        }

        private static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(PlaceholderArt.Navy);
            CreateEventSystem();

            var menuGo = new GameObject("MainMenu", typeof(MainMenuController));
            var menu = menuGo.GetComponent<MainMenuController>();

            var canvas = CreateCanvas("MenuCanvas");
            var root = (RectTransform)canvas.transform;

            var homePanel = CreateFullScreenPanel("Panel_Home", root);
            CreateText("Title", homePanel.transform, "DockIQ", 72, FontStyles.Bold, new Vector2(0f, 900f),
                new Vector2(900f, 100f), Color.white);
            CreateText("Tagline", homePanel.transform, "WAREHOUSE RESCUE", 34, FontStyles.Bold,
                new Vector2(0f, 820f), new Vector2(900f, 50f), PlaceholderArt.Hazard);
            var storyProgress = CreateText("StoryProgress", homePanel.transform, "Story Progress  0/48", 26,
                FontStyles.Normal, new Vector2(0f, 720f), new Vector2(900f, 40f), PlaceholderArt.Text);
            var storyMission = CreateText("StoryMission", homePanel.transform, "Continue: Level 1", 24,
                FontStyles.Normal, new Vector2(0f, 660f), new Vector2(960f, 50f), PlaceholderArt.Text);

            var storyBtn = CreateButton("StoryButton", homePanel.transform, "Story Mode",
                new Vector2(0f, 520f), new Vector2(420f, 90f));
            storyBtn.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.35f, 1f);
            var freePlayBtn = CreateButton("FreePlayButton", homePanel.transform, "Free Play",
                new Vector2(0f, 400f), new Vector2(420f, 90f));
            var achievementsBtn = CreateButton("AchievementsButton", homePanel.transform, "Achievements",
                new Vector2(0f, 280f), new Vector2(420f, 90f));
            var howToBtn = CreateButton("HowToPlayButton", homePanel.transform, "How to Play",
                new Vector2(0f, 160f), new Vector2(420f, 90f));
            CreateText("HomeHint", homePanel.transform,
                "Story advances the rescue campaign. Free Play replays unlocked levels.",
                20, FontStyles.Normal, new Vector2(0f, 20f), new Vector2(920f, 80f), PlaceholderArt.Text);

            var playPanel = CreateFullScreenPanel("Panel_Play", root);
            CreateText("LevelsLabel", playPanel.transform, "Free Play", 30, FontStyles.Bold,
                new Vector2(0f, 900f), new Vector2(400f, 40f), PlaceholderArt.Text);
            var playBack = CreateButton("BackButton", playPanel.transform, "Back",
                new Vector2(0f, 820f), new Vector2(220f, 72f));

            var scrollGo = new GameObject("LevelScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(playPanel.transform, false);
            var scrollRt = (RectTransform)scrollGo.transform;
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -40f);
            scrollRt.sizeDelta = new Vector2(980f, 980f);
            scrollGo.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.16f, 0.55f);
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = (RectTransform)viewportGo.transform;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = new Vector2(12f, 12f);
            viewportRt.offsetMax = new Vector2(-12f, -12f);
            viewportGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            var gridGo = new GameObject("LevelGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            gridGo.transform.SetParent(viewportGo.transform, false);
            var gridRt = (RectTransform)gridGo.transform;
            gridRt.anchorMin = new Vector2(0f, 1f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.anchoredPosition = Vector2.zero;
            gridRt.sizeDelta = Vector2.zero;

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140f, 140f);
            grid.spacing = new Vector2(18f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(8, 8, 8, 8);

            var fitter = gridGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.content = gridRt;
            scroll.viewport = viewportRt;

            var levelViews = new List<LevelButtonView>(LevelCatalog.Count);
            for (int i = 1; i <= LevelCatalog.Count; i++)
            {
                var levelButton = CreateButton($"Level_{i}", gridGo.transform, i.ToString(), Vector2.zero, new Vector2(140f, 140f));
                var label = levelButton.GetComponentInChildren<TextMeshProUGUI>();
                var view = levelButton.gameObject.AddComponent<LevelButtonView>();

                var levelSo = new SerializedObject(view);
                levelSo.FindProperty("_levelId").intValue = i;
                levelSo.FindProperty("_button").objectReferenceValue = levelButton;
                levelSo.FindProperty("_label").objectReferenceValue = label;
                levelSo.ApplyModifiedPropertiesWithoutUndo();

                levelViews.Add(view);
            }

            CreateText("Hint", playPanel.transform,
                "Tap switches, turntables, bridges & liftables. Slide path pieces. Avoid scrap - collisions fail!",
                20, FontStyles.Normal, new Vector2(0f, -920f), new Vector2(960f, 110f), PlaceholderArt.Text);

            var achievementsPanel = CreateModalPanel("Panel_Achievements", root, "Achievements",
                "Achievements coming soon.\n\nComplete Story rescues to unlock future badges for perfect runs, decoy-free clears, and full-yard mastery.",
                out Button achievementsBack);
            var howToPanel = CreateScrollModalPanel("Panel_HowToPlay", root, "How to Play",
                out TextMeshProUGUI howToBody, out Button howToBack);

            playPanel.SetActive(false);
            achievementsPanel.SetActive(false);
            howToPanel.SetActive(false);
            homePanel.SetActive(true);

            var menuSo = new SerializedObject(menu);
            menuSo.FindProperty("_homePanel").objectReferenceValue = homePanel;
            menuSo.FindProperty("_storyButton").objectReferenceValue = storyBtn;
            menuSo.FindProperty("_freePlayButton").objectReferenceValue = freePlayBtn;
            menuSo.FindProperty("_achievementsButton").objectReferenceValue = achievementsBtn;
            menuSo.FindProperty("_howToPlayButton").objectReferenceValue = howToBtn;
            menuSo.FindProperty("_storyProgressText").objectReferenceValue = storyProgress;
            menuSo.FindProperty("_storyMissionText").objectReferenceValue = storyMission;
            menuSo.FindProperty("_playPanel").objectReferenceValue = playPanel;
            menuSo.FindProperty("_playBackButton").objectReferenceValue = playBack;
            menuSo.FindProperty("_levelButtons").arraySize = levelViews.Count;
            for (int i = 0; i < levelViews.Count; i++)
                menuSo.FindProperty("_levelButtons").GetArrayElementAtIndex(i).objectReferenceValue = levelViews[i];
            menuSo.FindProperty("_achievementsPanel").objectReferenceValue = achievementsPanel;
            menuSo.FindProperty("_achievementsBackButton").objectReferenceValue = achievementsBack;
            menuSo.FindProperty("_howToPlayPanel").objectReferenceValue = howToPanel;
            menuSo.FindProperty("_howToPlayBackButton").objectReferenceValue = howToBack;
            menuSo.FindProperty("_howToPlayBody").objectReferenceValue = howToBody;
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/1_MainMenu.unity");
        }

        private static GameObject CreateFullScreenPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            StretchFull((RectTransform)go.transform);
            return go;
        }

        private static GameObject CreateModalPanel(string name, Transform parent, string title, string body,
            out Button backButton)
        {
            var panel = CreateFullScreenPanel(name, parent);
            var card = CreatePanel("Card", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(860f, 720f), PlaceholderArt.Panel);
            CreateText("Title", card.transform, title, 40, FontStyles.Bold,
                new Vector2(0f, 280f), new Vector2(780f, 60f), PlaceholderArt.Hazard);
            CreateText("Body", card.transform, body, 24, FontStyles.Normal,
                new Vector2(0f, 20f), new Vector2(760f, 420f), PlaceholderArt.Text);
            backButton = CreateButton("BackButton", card.transform, "Back",
                new Vector2(0f, -260f), new Vector2(240f, 72f));
            return panel;
        }

        private static GameObject CreateScrollModalPanel(string name, Transform parent, string title,
            out TextMeshProUGUI body, out Button backButton)
        {
            var panel = CreateFullScreenPanel(name, parent);
            var card = CreatePanel("Card", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(920f, 1400f), PlaceholderArt.Panel);
            CreateText("Title", card.transform, title, 40, FontStyles.Bold,
                new Vector2(0f, 600f), new Vector2(840f, 60f), PlaceholderArt.Hazard);

            var scrollGo = new GameObject("BodyScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(card.transform, false);
            var scrollRt = (RectTransform)scrollGo.transform;
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, 40f);
            scrollRt.sizeDelta = new Vector2(840f, 1000f);
            scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.09f, 0.14f, 0.65f);
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = (RectTransform)viewportGo.transform;
            StretchFull(viewportRt);
            viewportRt.offsetMin = new Vector2(16f, 16f);
            viewportRt.offsetMax = new Vector2(-16f, -16f);
            viewportGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            body = CreateText("Body", contentGo.transform, "", 22, FontStyles.Normal,
                Vector2.zero, new Vector2(780f, 2000f), PlaceholderArt.Text);
            body.alignment = TextAlignmentOptions.TopLeft;
            var bodyRt = (RectTransform)body.transform;
            bodyRt.anchorMin = new Vector2(0f, 1f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 1f);
            bodyRt.anchoredPosition = Vector2.zero;
            bodyRt.sizeDelta = new Vector2(-20f, 2000f);
            body.enableAutoSizing = false;

            scroll.content = contentRt;
            scroll.viewport = viewportRt;

            backButton = CreateButton("BackButton", card.transform, "Back",
                new Vector2(0f, -620f), new Vector2(240f, 72f));
            return panel;
        }

        private static void BuildGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(PlaceholderArt.Navy);
            CreateEventSystem();

            var gameRoot = new GameObject("GameRoot", typeof(GameSceneController), typeof(LevelController));
            var controller = gameRoot.GetComponent<LevelController>();

            var canvas = CreateCanvas("GameHUD");
            var safe = CreateSafeArea(canvas.transform);
            var hudGo = new GameObject("HUD", typeof(GameHud));
            hudGo.transform.SetParent(canvas.transform, false);
            var hud = hudGo.GetComponent<GameHud>();

            var top = CreatePanel("TopBar", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(680f, 160f), PlaceholderArt.Panel);
            var title = CreateText("Title", top.transform, "DockIQ", 28, FontStyles.Bold, new Vector2(0f, 50f),
                new Vector2(640f, 40f), PlaceholderArt.Text);
            var request = CreateText("Request", top.transform, "", 22, FontStyles.Normal, new Vector2(0f, 8f),
                new Vector2(640f, 50f), PlaceholderArt.Text);
            var timer = CreateText("Timer", top.transform, "0:00", 32, FontStyles.Bold, new Vector2(0f, -48f),
                new Vector2(200f, 40f), PlaceholderArt.Hazard);

            var bottom = CreatePanel("BottomBar", safe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(680f, 80f), Color.clear);
            var pauseBtn = CreateButton("PauseButton", bottom.transform, "Pause", Vector2.zero, new Vector2(220f, 72f));

            var resultPanel = CreatePanel("ResultPanel", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(560f, 320f), PlaceholderArt.Panel).gameObject;
            var resultText = CreateText("ResultText", resultPanel.transform, "", 34, FontStyles.Bold,
                new Vector2(0f, 60f), new Vector2(500f, 80f), PlaceholderArt.Text);
            var nextBtn = CreateButton("NextButton", resultPanel.transform, "Next Level", new Vector2(0f, -20f), new Vector2(220f, 72f));
            var retryResultBtn = CreateButton("RetryResultButton", resultPanel.transform, "Retry", new Vector2(0f, -100f), new Vector2(220f, 72f));
            var menuResultBtn = CreateButton("MenuResultButton", resultPanel.transform, "Menu", new Vector2(0f, -180f), new Vector2(220f, 72f));
            resultPanel.SetActive(false);

            var backdrop = CreatePanel("PauseBackdrop", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.35f));
            StretchFull((RectTransform)backdrop.transform);
            var backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdrop.gameObject.SetActive(false);

            var pausePanel = CreatePanel("PausePanel", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(560f, 380f), PlaceholderArt.Panel).gameObject;
            CreateText("PauseTitle", pausePanel.transform, "Paused", 34, FontStyles.Bold,
                new Vector2(0f, 120f), new Vector2(500f, 80f), PlaceholderArt.Text);
            var resumeBtn = CreateButton("ResumeButton", pausePanel.transform, "Resume", new Vector2(0f, 45f), new Vector2(220f, 72f));
            var restartBtn = CreateButton("RestartButton", pausePanel.transform, "Restart", new Vector2(0f, -45f), new Vector2(220f, 72f));
            var quitBtn = CreateButton("QuitToMenuButton", pausePanel.transform, "Quit to Menu", new Vector2(0f, -135f), new Vector2(260f, 72f));
            pausePanel.SetActive(false);

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("_requestText").objectReferenceValue = request;
            hudSo.FindProperty("_timerText").objectReferenceValue = timer;
            hudSo.FindProperty("_titleText").objectReferenceValue = title;
            hudSo.FindProperty("_pauseButton").objectReferenceValue = pauseBtn;
            hudSo.FindProperty("_resultPanel").objectReferenceValue = resultPanel;
            hudSo.FindProperty("_resultText").objectReferenceValue = resultText;
            hudSo.FindProperty("_nextButton").objectReferenceValue = nextBtn;
            hudSo.FindProperty("_retryResultButton").objectReferenceValue = retryResultBtn;
            hudSo.FindProperty("_menuResultButton").objectReferenceValue = menuResultBtn;
            hudSo.FindProperty("_pauseBackdrop").objectReferenceValue = backdrop.gameObject;
            hudSo.FindProperty("_pausePanel").objectReferenceValue = pausePanel;
            hudSo.FindProperty("_pauseBackdropButton").objectReferenceValue = backdropButton;
            hudSo.FindProperty("_resumeButton").objectReferenceValue = resumeBtn;
            hudSo.FindProperty("_restartButton").objectReferenceValue = restartBtn;
            hudSo.FindProperty("_quitToMenuButton").objectReferenceValue = quitBtn;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            CreateAndWireTutorialUi(safe, hud);

            var boardArt = EnsureBoardArtCatalog();
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("_boardArt").objectReferenceValue = boardArt;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var sceneSo = new SerializedObject(gameRoot.GetComponent<GameSceneController>());
            sceneSo.FindProperty("_hud").objectReferenceValue = hud;
            sceneSo.FindProperty("_controller").objectReferenceValue = controller;
            sceneSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/2_Game.unity");
        }

        private static void CreateAndWireTutorialUi(Transform safe, GameHud hud)
        {
            var tutorialBackdrop = CreatePanel("TutorialBackdrop", safe, Vector2.zero, Vector2.one, Vector2.zero,
                Vector2.zero, new Color(0f, 0f, 0f, 0.45f));
            StretchFull((RectTransform)tutorialBackdrop.transform);
            tutorialBackdrop.gameObject.SetActive(false);

            var tutorialPanel = CreatePanel("TutorialPanel", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(620f, 420f), PlaceholderArt.Panel).gameObject;
            var tutorialTitle = CreateText("TutorialTitle", tutorialPanel.transform, "Tip", 32, FontStyles.Bold,
                new Vector2(0f, 140f), new Vector2(560f, 60f), PlaceholderArt.Hazard);
            var tutorialBody = CreateText("TutorialBody", tutorialPanel.transform, "", 24, FontStyles.Normal,
                new Vector2(0f, 10f), new Vector2(540f, 200f), PlaceholderArt.Text);
            var gotItBtn = CreateButton("TutorialGotItButton", tutorialPanel.transform, "Got it",
                new Vector2(0f, -145f), new Vector2(240f, 72f));
            gotItBtn.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.35f, 1f);
            tutorialPanel.SetActive(false);

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("_tutorialBackdrop").objectReferenceValue = tutorialBackdrop.gameObject;
            hudSo.FindProperty("_tutorialPanel").objectReferenceValue = tutorialPanel;
            hudSo.FindProperty("_tutorialTitle").objectReferenceValue = tutorialTitle;
            hudSo.FindProperty("_tutorialBody").objectReferenceValue = tutorialBody;
            hudSo.FindProperty("_tutorialGotItButton").objectReferenceValue = gotItBtn;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform CreateSafeArea(Transform parent)
        {
            var go = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            StretchFull(rt);
            return rt;
        }

        private static Image CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = PlaceholderArt.WhiteSquare();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, FontStyles style,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchorCenter, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchorCenter;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = PlaceholderArt.WhiteSquare();
            image.color = new Color(0.15f, 0.35f, 0.55f, 1f);

            CreateText("Label", go.transform, label, 28, FontStyles.Bold, Vector2.zero, new Vector2(size.x - 20f, 60f), PlaceholderArt.Text);
            return go.GetComponent<Button>();
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void CreateCamera(Color bg)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{SceneFolder}/0_SplashScene.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/1_MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/2_Game.unity", true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
