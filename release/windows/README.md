# SENTINEL-XR 2.0 — Windows Standalone Release Candidate 1

## Overview
This directory contains the release distribution files, build scripts, configuration manifests, and documentation for the native Windows Standalone desktop build of SENTINEL-XR 2.0.

## Deliverables Checklist
- `SentinelXR_Config.json`: Production desktop configuration for backend endpoint connection, graphics, and authentication.
- `package_manifest.json`: Release package manifest, architecture specification, and dependency requirements.
- `build_windows.bat`: One-click batch build script for local Windows workstations running Unity 6000.0.23f1.
- `build_windows.sh`: Headless CI compilation script for cross-compiling or validating build scripts in Linux environments.

## How to Compile Standalone Binary
To compile `SentinelXR.exe` and `SentinelXR_Data/` on a workstation with Unity 6000.0.23f1 installed:
```cmd
build_windows.bat
```
Or run the Unity batchmode command directly:
```cmd
"C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe" -batchmode -quit -projectPath "..\..\UnityProject\SentinelXR" -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildWindowsStandalone -logFile "..\reports\unity_windows_build.log"
```

## How to Run
1. Ensure the Sentinel Backend is running (`npm run dev` or `node dist/server.js` listening on port 3000).
2. Launch `SentinelXR.exe`.
3. The application will read `SentinelXR_Config.json` from the working directory, connect to `ws://127.0.0.1:3000/ws` and `http://127.0.0.1:3000/api/v1`, load the 9-node 3D network topology, and initialize the Obsidian Neural UI.
