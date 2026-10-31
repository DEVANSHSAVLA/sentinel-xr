# SENTINEL-XR 2.0 — Android ARBuild Report

**Evaluation Date:** October 30, 2026  
**Auditor:** Principal Mobile/XR Systems Engineer  
**Release Target:** Release Candidate 1 (RC1)  
**Target Package:** `com.sentinelxr.soc`  

---

## 1. Specification Matrix

| Component | Target Requirement | Implemented Specification | Audit Status |
|---|---|---|---|
| **AR Framework** | Unity AR Foundation 6.0+ | `com.unity.xr.arfoundation: 6.0.2` | **PASS** |
| **AR Provider** | Google ARCore XR Plugin | `com.unity.xr.arcore: 6.0.2` | **PASS** |
| **Camera Permissions** | `android.permission.CAMERA` | Specified in `release/android/AndroidManifest.xml` | **PASS** |
| **ARCore Metadata** | `com.google.ar.core: required` | Configured in Application tag | **PASS** |
| **Network Permissions** | `android.permission.INTERNET` | Configured in Manifest | **PASS** |
| **Scripting Backend** | IL2CPP / ARM64 | Configured in `SentinelBuildPipeline.cs` | **PASS** |
| **Target Android API** | Minimum API 26 (Android 8.0) | Target API 34 (Android 14) | **PASS** |
| **AR Scene** | `Assets/Scenes/ARInvestigationScene.unity` | Configured with `ARMarkerManager` & `ARDeviceOverlay` | **PASS** |
| **Local APK Compilation** | Linux Sandbox CLI Build | Requires Android NDK/SDK & Unity Android module | **BLOCKED** *(Toolchain Missing)* |
| **Physical Device Verification**| Pixel 7 / Galaxy S23 | Requires physical USB hardware tethering | **BLOCKED** *(Hardware Missing)* |

---

## 2. Manifest and Security Configuration
The generated manifest at `release/android/AndroidManifest.xml` enforces strict compliance with Google Play and enterprise MDM standards:
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-feature android:name="android.hardware.camera.ar" android:required="true" />
<meta-data android:name="com.google.ar.core" android:value="required" />
```

---

## 3. AR Marker Tracking & Spatial Registration Logic
In `UnityProject/SentinelXR/Assets/Scripts/AR/ARMarkerManager.cs`:
- Markers are mapped directly to server rack hardware IDs: `server-01`, `db-cluster-01`, `gateway-01`.
- When an AR camera detects the marker, `ARDeviceOverlay.cs` projects the 3D Obsidian Neural HUD directly over the physical server chassis.
- Status indicators mirror the central SOC state (e.g. flashing crimson for active breaches, pulsing cyan for isolated hosts).

---

## 4. Build Pipeline Instructions
To compile the production APK on an Android development workstation:
```bash
# In release/android/:
./build_android.sh
```
Or via the Unity batchmode pipeline:
```bash
Unity.exe -batchmode -quit -projectPath "UnityProject/SentinelXR" \
  -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildAndroidAR \
  -logFile "release/reports/unity_android_build.log"
```
