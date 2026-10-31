#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SentinelXR.Editor
{
    /// <summary>
    /// SentinelBuildPipeline provides headless CLI batchmode builds for Windows Desktop,
    /// Android AR (ARCore), and OpenXR VR runtimes.
    /// Usage:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildWindowsStandalone
    /// </summary>
    public static class SentinelBuildPipeline
    {
        private static readonly string[] GAME_SCENES = new[]
        {
            "Assets/Scenes/BootstrapScene.unity",
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/HQScene.unity",
            "Assets/Scenes/Mission01_SignalInTheNoise.unity",
            "Assets/Scenes/VRCommandCenter.unity",
            "Assets/Scenes/ARTraining.unity",
            "Assets/Scenes/MainSOCScene.unity"
        };

        private static readonly string[] AR_SCENES = new[]
        {
            "Assets/Scenes/ARTraining.unity",
            "Assets/Scenes/ARInvestigationScene.unity"
        };

        private static readonly string[] VR_SCENES = new[]
        {
            "Assets/Scenes/VRCommandCenter.unity",
            "Assets/Scenes/VRSOCScene.unity"
        };

        [MenuItem("Sentinel-XR/Build/Mission 01 Standalone Windows", false, 1)]
        public static bool BuildMission01Standalone()
        {
            Debug.Log("[SentinelBuildPipeline] Starting Mission 01 Windows Standalone Build...");

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Windows"));
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string exePath = Path.Combine(outputDir, "SENTINEL-XR.exe");

            string[] missionScenes = new[]
            {
                "Assets/Scenes/BootstrapScene.unity",
                "Assets/Scenes/MainMenuScene.unity",
                "Assets/Scenes/HQScene.unity",
                "Assets/Scenes/Mission01_SignalInTheNoise.unity"
            };

            PlayerSettings.productName = "SENTINEL-XR";
            PlayerSettings.companyName = "Aegis Sentinel Cybernetics";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = missionScenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EvaluateReport(report, "Mission 01 Windows Standalone");
            return report.summary.result == BuildResult.Succeeded;
        }

        [MenuItem("Sentinel-XR/Build/Windows Standalone (RC2)")]
        public static void BuildWindowsStandalone()
        {
            Debug.Log("[SentinelBuildPipeline] Starting Windows Standalone RC2 Build...");

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../release/windows"));
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string exePath = Path.Combine(outputDir, "SentinelXR.exe");

            PlayerSettings.productName = "SENTINEL-XR";
            PlayerSettings.companyName = "Aegis Sentinel Cybernetics";
            PlayerSettings.bundleVersion = "2.0.0-rc2";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GAME_SCENES,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EvaluateReport(report, "Windows Standalone");
        }

        [MenuItem("Sentinel-XR/Build/Android ARCore APK (RC2)")]
        public static void BuildAndroidAR()
        {
            Debug.Log("[SentinelBuildPipeline] Starting Android ARCore APK Build...");

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../release/android"));
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string apkPath = Path.Combine(outputDir, "SentinelXR_v2.0.0-rc2_arcore.apk");

            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.sentinelxr.soc");
            PlayerSettings.bundleVersion = "2.0.0-rc2";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = AR_SCENES,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EvaluateReport(report, "Android ARCore APK");
        }

        [MenuItem("Sentinel-XR/Build/Android ARCore AAB Release Bundle (RC2)")]
        public static void BuildAndroidAAB()
        {
            Debug.Log("[SentinelBuildPipeline] Starting Android ARCore AAB Release Bundle Build...");

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../release/android"));
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string aabPath = Path.Combine(outputDir, "SentinelXR_v2.0.0-rc2_release.aab");

            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.sentinelxr.soc");
            PlayerSettings.bundleVersion = "2.0.0-rc2";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = AR_SCENES,
                locationPathName = aabPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EvaluateReport(report, "Android ARCore AAB Bundle");
        }

        [MenuItem("Sentinel-XR/Build/OpenXR VR Desktop (RC2)")]
        public static void BuildOpenXRVR()
        {
            Debug.Log("[SentinelBuildPipeline] Starting OpenXR VR Desktop Build...");

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../release/vr"));
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string exePath = Path.Combine(outputDir, "SentinelXR_VR.exe");

            PlayerSettings.productName = "SENTINEL-XR VR";
            PlayerSettings.bundleVersion = "2.0.0-rc2";
            PlayerSettings.runInBackground = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = VR_SCENES,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EvaluateReport(report, "OpenXR VR Desktop");
        }

        private static void EvaluateReport(BuildReport report, string platformName)
        {
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[SentinelBuildPipeline] SUCCESS: {platformName} built successfully! Total Size: {summary.totalSize} bytes, Time: {summary.totalTime.TotalSeconds:F1}s");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[SentinelBuildPipeline] FAILED: {platformName} build failed with {summary.totalErrors} errors!");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
