# SENTINEL-XR 2.0 — Unity Setup & Build Guide

## 1. Opening the Project in Unity Editor

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select directory: `SENTINEL-XR/UnityProject/SentinelXR/`.
4. Select **Unity 6 LTS (6000.0.x)** or the installed compatible 2022/2023 LTS version.
5. Unity will resolve dependencies from `Packages/manifest.json`.

---

## 2. Project Settings Configuration

### Graphics & Rendering
- **Render Pipeline**: Set to `UniversalRenderPipelineAsset` (URP).
- **Color Space**: `Linear` (Project Settings -> Player -> Other Settings -> Color Space).
- **Post-Processing**: Enable `Bloom`, `ACES Tonemapping`, and `Depth of Field` in Global Volume.

### XR Plugin Management
- **Desktop Target**: Disable standalone XR providers for normal non-headset operation.
- **Android Target**: Enable **ARCore** in `XR Plug-in Management -> Android`.
- **OpenXR Target**: Enable **OpenXR** in `XR Plug-in Management -> Standalone/Windows` with the `Oculus Touch` and `HTC Vive` interaction profiles.

---

## 3. Platform Build Instructions

### Building Desktop SOC (Windows / macOS / WebGL)
```
File -> Build Profiles -> Desktop (Standalone) -> Build and Run
```
Output: `Builds/Desktop/SentinelXR.exe`

### Building Android AR (ARCore)
1. In Build Settings, switch platform to **Android**.
2. Target API Level: `API 31+ (Android 12+)`.
3. Minimum API Level: `API 26 (Android 8.0 Oreo)`.
4. Export or Build APK: `Builds/Android/SentinelXR_AR.apk`.
5. Deploy via ADB:
   ```bash
   adb install -r Builds/Android/SentinelXR_AR.apk
   ```

### Building Meta Quest / OpenXR VR
1. Switch platform to **Android** (for Quest native APK) or **Windows Standalone** (for PCVR AirLink/Link Cable).
2. Set Texture Compression: `ASTC`.
3. Build and deploy to headset.
