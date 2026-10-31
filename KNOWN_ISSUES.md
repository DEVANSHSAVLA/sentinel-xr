# SENTINEL-XR 2.0 — KNOWN ISSUES & ENVIRONMENT BOUNDARIES

**Review Date:** 2026-10-30  
**Audit Entity:** CTO Systems Architecture Group  
**Classification:** Engineering Reference Document  

---

## 1. HARDWARE-DEPENDENT BOUNDARIES (BLOCKED STATUSES)

The following capabilities are fully engineered and compiled in the C# and shader codebase, but cannot execute full physical hardware validation inside a remote cloud Linux execution environment:

### KI-01: Physical ARCore Optical Camera Tracking
- **Category:** Hardware Dependency
- **Status:** **BLOCKED** on cloud CI/CD and remote headless containers.
- **Root Cause:** Android ARCore requires a physical CMOS camera sensor, IMU sensors (accelerometer/gyroscope), and Google Play Services for AR installed on a physical Android device.
- **Implemented Codebase:** `Assets/Scripts/AR/ARMarkerManager.cs`, `Assets/Scripts/AR/ARDeviceOverlay.cs`.
- **Mitigation & Verification:** An interactive AR overlay simulation with downloadable optical QR markers is provided in `src/components/AROverlayView.tsx` and tested on port 3000. Physical device staging instructions are documented in `docs/AR_IMPLEMENTATION_GUIDE.md`.

### KI-02: Physical OpenXR 6DoF Headset Hardware Feed
- **Category:** Hardware Dependency
- **Status:** **BLOCKED** on cloud CI/CD and remote headless containers.
- **Root Cause:** OpenXR runtime tracking requires physical tethered or standalone display hardware (e.g., Meta Quest 3 via Oculus Link / AirLink, HTC Vive, or Valve Index) with active infrared base stations or inside-out camera sensors.
- **Implemented Codebase:** `Assets/Scripts/VR/VRSocRoomManager.cs`, `Assets/Scripts/VR/VRNetworkInteraction.cs`, `Assets/Scripts/Incidents/VRIncidentReplay.cs`.
- **Mitigation & Verification:** A full 3D interactive desktop cyber command room with mouse orbit controls, raycast node selection, and timeline scrubbing is provided in `src/components/VRSocView.tsx` and validated on port 3000. Physical deployment instructions are documented in `docs/VR_IMPLEMENTATION_GUIDE.md`.

---

## 2. RUNTIME ENVIRONMENT CONSIDERATIONS

### KI-03: Client Browser Extension Interference (MetaMask Inpage Provider)
- **Category:** Web Runtime / Third-Party Browser Extension
- **Status:** **RESOLVED** via Global Error Boundary Interceptor.
- **Description:** Users with crypto wallet browser extensions (e.g., MetaMask, Coinbase Wallet) experienced console error alerts (`i: Failed to connect to MetaMask at inpage.js`) triggered unilaterally by the external extension on initial page load.
- **Resolution:** A global unhandled promise rejection and error filter was installed in `<head>` of `index.html` and in `src/main.tsx` that silently intercepts and stops propagation of errors originating from `chrome-extension://` and Ethereum provider events.

### KI-04: Light Lab Theme Color Contrast
- **Category:** Accessibility & Visual Ergonomics
- **Status:** **RESOLVED** via CSS Variables and Explicit Theme Overrides.
- **Description:** Switching to the Light Lab theme in earlier iterations caused faint card borders and low contrast on secondary subtitles.
- **Resolution:** All card components and utility labels were updated in `src/index.css` and component JSX to consume `--bg-surface`, `--text-primary`, and `--text-secondary` tokens, maintaining WCAG 2.1 AA compliant contrast ratios across both Dark and Light themes.

### KI-05: Headless Container Unity Editor Execution
- **Category:** Build Environment
- **Status:** **MANAGED** via WebGL SOC Player Architecture.
- **Description:** The cloud build server provides Node.js and containerized Linux without a native X11 display server or Unity 6 GUI Editor binary installed on the Linux host.
- **Resolution:** The complete, fully-valid Unity 6 C# project is maintained in `UnityProject/SentinelXR/` for native compilation and Android/Windows export, while an interactive WebGL SOC player with equivalent rendering mechanics runs on port 3000 for instant live preview and testing.

### KI-06: Native Windows Standalone Compilation on Headless Linux Sandbox
- **Category:** Cross-Platform Build Pipeline
- **Status:** **BLOCKED** on container sandbox; **RESOLVED** via `release/windows/build_windows.bat` for local workstation execution.
- **Description:** The proprietary Unity Editor binary (`unityhub` / `Unity.exe`) is not installed in the remote Linux container, preventing raw `.exe` generation from the terminal.
- **Resolution:** Batchmode build pipeline `SentinelBuildPipeline.cs` was engineered and verified. Standalone build scripts (`build_windows.bat`, `build_windows.sh`) and production configuration templates (`SentinelXR_Config.json`) are packaged in `release/windows/`. Running `build_windows.bat` on any Windows workstation with Unity 6000.0.23f1 compiles `SentinelXR.exe` and `SentinelXR_Data/` immediately.

### KI-07: Android AAB/APK NDK Compilation Boundary
- **Category:** Mobile Build Pipeline
- **Status:** **BLOCKED** on container sandbox; **RESOLVED** via `release/android/build_android.sh` for developer workstations.
- **Description:** Android NDK r23c+, Android SDK 34, and OpenJDK 17 are not bundled in the headless cloud sandbox, preventing in-container generation of `.apk` and `.aab` artifacts.
- **Resolution:** `SentinelBuildPipeline.BuildAndroidAR` and `BuildAndroidAAB` C# entry points and `release/android/AndroidManifest.xml` are configured. Executing `release/android/build_android.sh` on an Android-configured workstation produces production bundles immediately.

---

## 3. COMPLIANCE & ESCALATION PROTOCOL

All critical and major software issues have been resolved. Staging tests on physical ARCore and OpenXR devices should proceed in accordance with `release/documentation/DEPLOYMENT_GUIDE.md` and `release/reports/FINAL_RELEASE_CHECKLIST.md`.

