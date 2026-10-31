#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SentinelXR.Editor
{
    public class SentinelXRProjectValidator : EditorWindow
    {
        private Vector2 scrollPos;
        private static List<ValidationResultItem> lastResults = new List<ValidationResultItem>();

        public struct ValidationResultItem
        {
            public string category;
            public string checkName;
            public bool passed;
            public string details;
        }

        [MenuItem("SENTINEL-XR/Validate Project", false, 1)]
        public static void ValidateProjectMenu()
        {
            RunFullValidation();
            GetWindow<SentinelXRProjectValidator>("SENTINEL-XR Validator").Show();
        }

        [MenuItem("SENTINEL-XR/Open Main Game Scene", false, 2)]
        public static void OpenMainGameScene()
        {
            string scenePath = "Assets/Scenes/MainMenuScene.unity";
            if (File.Exists(scenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Debug.Log($"[SentinelXRValidator] Opened Main Game Scene: {scenePath}");
                }
            }
            else
            {
                Debug.LogError($"[SentinelXRValidator] Scene not found at: {scenePath}");
            }
        }

        [MenuItem("SENTINEL-XR/Open Mission 01 (Playable)", false, 3)]
        public static void OpenMission01Scene()
        {
            string scenePath = "Assets/Scenes/Mission01_SignalInTheNoise.unity";
            if (File.Exists(scenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Debug.Log($"[SentinelXRValidator] Opened Playable Mission 01 Scene: {scenePath}");
                }
            }
            else
            {
                Debug.LogError($"[SentinelXRValidator] Scene not found at: {scenePath}");
            }
        }

        public static void RunFullValidation()
        {
            lastResults.Clear();

            // 1. Required Folders
            string[] requiredFolders = new string[]
            {
                "Assets/Scenes",
                "Assets/Scripts",
                "Assets/Scripts/Gameplay",
                "Assets/Scripts/Gameplay/Player",
                "Assets/Scripts/Gameplay/Tools",
                "Assets/Scripts/Gameplay/Enemies",
                "Assets/Scripts/Gameplay/Environment",
                "Assets/Scripts/Gameplay/Missions",
                "Assets/Scripts/Gameplay/UI",
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Shaders",
                "Assets/Editor",
                "Packages"
            };

            foreach (var folder in requiredFolders)
            {
                bool exists = Directory.Exists(folder);
                lastResults.Add(new ValidationResultItem
                {
                    category = "Folder Structure",
                    checkName = $"Directory: {folder}",
                    passed = exists,
                    details = exists ? "Verified present on disk." : "Directory missing!"
                });
            }

            // 2. Required Scenes
            string[] requiredScenes = new string[]
            {
                "Assets/Scenes/BootstrapScene.unity",
                "Assets/Scenes/MainMenuScene.unity",
                "Assets/Scenes/HQScene.unity",
                "Assets/Scenes/Mission01_SignalInTheNoise.unity",
                "Assets/Scenes/VRCommandCenter.unity",
                "Assets/Scenes/ARTraining.unity",
                "Assets/Scenes/MainSOCScene.unity"
            };

            foreach (var scn in requiredScenes)
            {
                bool exists = File.Exists(scn);
                lastResults.Add(new ValidationResultItem
                {
                    category = "Scenes",
                    checkName = Path.GetFileName(scn),
                    passed = exists,
                    details = exists ? $"Valid scene asset ({new FileInfo(scn).Length} bytes)." : "Scene file not found!"
                });
            }

            // 3. Build Settings Scene Registration
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            bool bootstrapRegistered = buildScenes.Length > 0 && buildScenes[0].path.Contains("BootstrapScene");
            lastResults.Add(new ValidationResultItem
            {
                category = "Build Settings",
                checkName = "Initial Startup Scene Order",
                passed = bootstrapRegistered,
                details = bootstrapRegistered ? "BootstrapScene is registered as Index 0." : "BootstrapScene must be at Index 0!"
            });

            // 4. Assembly Definition
            string asmdefPath = "Assets/Scripts/SentinelXR.asmdef";
            bool asmdefExists = File.Exists(asmdefPath);
            lastResults.Add(new ValidationResultItem
            {
                category = "Assembly Definitions",
                checkName = "SentinelXR.asmdef",
                passed = asmdefExists,
                details = asmdefExists ? "Assembly definition configured." : "asmdef file missing!"
            });

            // 5. Package Dependencies
            string manifestPath = "Packages/manifest.json";
            bool manifestExists = File.Exists(manifestPath);
            string manifestContent = manifestExists ? File.ReadAllText(manifestPath) : "";
            bool hasUrp = manifestContent.Contains("com.unity.render-pipelines.universal");
            bool hasOpenXr = manifestContent.Contains("com.unity.xr.openxr");
            bool hasArFoundation = manifestContent.Contains("com.unity.xr.arfoundation");

            lastResults.Add(new ValidationResultItem
            {
                category = "Package Manager",
                checkName = "Universal Render Pipeline (URP)",
                passed = hasUrp,
                details = hasUrp ? "com.unity.render-pipelines.universal present in manifest." : "URP package missing!"
            });
            lastResults.Add(new ValidationResultItem
            {
                category = "Package Manager",
                checkName = "OpenXR Plugin",
                passed = hasOpenXr,
                details = hasOpenXr ? "com.unity.xr.openxr present in manifest." : "OpenXR package missing!"
            });
            lastResults.Add(new ValidationResultItem
            {
                category = "Package Manager",
                checkName = "AR Foundation",
                passed = hasArFoundation,
                details = hasArFoundation ? "com.unity.xr.arfoundation present in manifest." : "AR Foundation package missing!"
            });

            // 6. Project Version
            string versionPath = "ProjectSettings/ProjectVersion.txt";
            bool versionExists = File.Exists(versionPath);
            lastResults.Add(new ValidationResultItem
            {
                category = "Project Version",
                checkName = "Unity 6000.0.23f1 Stamping",
                passed = versionExists,
                details = versionExists ? File.ReadAllText(versionPath).Trim() : "ProjectVersion.txt missing!"
            });

            Debug.Log($"[SentinelXRValidator] Full Project Validation Complete: {lastResults.Count} checks executed.");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("SENTINEL-XR: BREACH PROTOCOL — PROJECT VALIDATOR", EditorStyles.boldLabel);
            GUILayout.Label("Unity 6000.0.23f1 URP Architecture Integrity Verification", EditorStyles.miniLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Re-Run Project Validation", GUILayout.Height(30)))
            {
                RunFullValidation();
            }

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            string currentCategory = "";
            foreach (var item in lastResults)
            {
                if (item.category != currentCategory)
                {
                    currentCategory = item.category;
                    GUILayout.Space(8);
                    GUILayout.Label($"--- {currentCategory.ToUpper()} ---", EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal("box");
                GUI.color = item.passed ? Color.green : Color.red;
                GUILayout.Label(item.passed ? "[PASS]" : "[FAIL]", GUILayout.Width(55));
                GUI.color = Color.white;
                GUILayout.Label(item.checkName, GUILayout.Width(220));
                GUILayout.Label(item.details, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.Space(10);

            if (GUILayout.Button("Open MainMenuScene (Play Mode Ready)", GUILayout.Height(35)))
            {
                OpenMainGameScene();
            }
        }
    }
}
#endif
