# SENTINEL-XR 2.0 — Android ARCore Release Candidate 1

## Overview
This directory contains the release configuration, manifest specifications, and automated build scripts for the Android AR application of SENTINEL-XR 2.0.

## Supported Devices
- Samsung Galaxy S21 / S22 / S23 / S24 Ultra & Plus
- Google Pixel 6 / 7 / 8 / 9 Pro
- Lenovo ThinkReality A3 / Motorola Edge+
- Any Android 8.0+ (API 26+) device with ARCore (Google Play Services for AR) support.

## Key Configurations
- **Manifest:** `release/android/AndroidManifest.xml` specifying `com.google.ar.core: required`, camera permissions, and network communication.
- **Scripting Backend:** IL2CPP with ARM64 architecture for high-performance neural graph calculations.
- **Target Graphics API:** Vulkan (primary) and OpenGLES3 (fallback).
- **Scene:** `Assets/Scenes/ARInvestigationScene.unity` with `ARMarkerManager` and `ARDeviceOverlay`.

## Deployment Instructions via ADB
```bash
# 1. Connect physical Android device via USB with USB Debugging enabled
adb devices

# 2. Verify Google Play Services for AR is installed
adb shell pm list packages | grep arcore

# 3. Install release APK
adb install -r release/android/SentinelXR_v2.0.0-rc1_arcore.apk

# 4. Grant runtime camera permissions
adb shell pm grant com.sentinelxr.soc android.permission.CAMERA

# 5. Launch application
adb shell monkey -p com.sentinelxr.soc -c android.intent.category.LAUNCHER 1
```
