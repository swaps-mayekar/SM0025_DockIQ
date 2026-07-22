using System;
using System.Collections.Generic;
using System.IO;
using DockIQ.Core;
using DockIQ.Gameplay;
using DockIQ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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
            try
            {
                EnsureFolders();
                EnsureLogoResource();
                BuildSplashScene();
                BuildMenuScene();
                BuildGameScene();
                ConfigureBuildSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("DOCKIQ_BUILD_COMPLETE");
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
            {
                AssetDatabase.CopyAsset(src, dst);
            }

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
            new GameObject("Splash", typeof(SplashController));
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/0_SplashScene.unity");
        }

        private static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(PlaceholderArt.Navy);
            CreateEventSystem();
            new GameObject("MainMenu", typeof(MainMenuController));
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/1_MainMenu.unity");
        }

        private static void BuildGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(PlaceholderArt.Navy);
            CreateEventSystem();
            new GameObject("Game", typeof(GameSceneController));
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/2_Game.unity");
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
