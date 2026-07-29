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

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/1_MainMenu.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/2_Game.unity") != null)
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

        private static void BuildInternal(bool overwriteExistingScenes)
        {
            try
            {
                EnsureFolders();
                EnsureLogoResource();
                EnsureScene($"{SceneFolder}/0_SplashScene.unity", BuildSplashScene, overwriteExistingScenes);
                EnsureScene($"{SceneFolder}/1_MainMenu.unity", BuildMenuScene, overwriteExistingScenes);
                EnsureScene($"{SceneFolder}/2_Game.unity", BuildGameScene, overwriteExistingScenes);

                // Non-destructive: if game scene was kept, still add tutorial UI when missing.
                if (!overwriteExistingScenes &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneFolder}/2_Game.unity") != null)
                    EnsureTutorialUiInGameScene();

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
            var safe = CreateSafeArea(canvas.transform);

            CreateText("Title", safe, "DockIQ", 72, FontStyles.Bold, new Vector2(0f, 900f),
                new Vector2(900f, 100f), Color.white);
            CreateText("Subtitle", safe, "Warehouse Rescue", 34, FontStyles.Bold, new Vector2(0f, 820f),
                new Vector2(900f, 50f), PlaceholderArt.Hazard);
            var playButton = CreateButton("PlayButton", safe, "Play", new Vector2(0f, 680f), new Vector2(220f, 72f));
            playButton.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.35f, 1f);
            CreateText("LevelsLabel", safe, "Levels", 30, FontStyles.Bold, new Vector2(0f, 560f),
                new Vector2(400f, 40f), PlaceholderArt.Text);

            var scrollGo = new GameObject("LevelScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(safe, false);
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

            CreateText("Hint", safe,
                "Tap switches, turntables, bridges & liftables. Slide path pieces. Avoid scrap - collisions fail!",
                20, FontStyles.Normal, new Vector2(0f, -920f), new Vector2(960f, 110f), PlaceholderArt.Text);

            var menuSo = new SerializedObject(menu);
            menuSo.FindProperty("_playButton").objectReferenceValue = playButton;
            menuSo.FindProperty("_levelButtons").arraySize = levelViews.Count;
            for (int i = 0; i < levelViews.Count; i++)
                menuSo.FindProperty("_levelButtons").GetArrayElementAtIndex(i).objectReferenceValue = levelViews[i];
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/1_MainMenu.unity");
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
            tmp.enableWordWrapping = true;
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
