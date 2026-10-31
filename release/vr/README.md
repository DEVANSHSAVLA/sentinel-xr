# SENTINEL-XR 2.0 — OpenXR VR Release Candidate 1

## Overview
This directory contains OpenXR configuration specifications, interactive profile definitions, and build scripts for deploying SENTINEL-XR 2.0 into immersive virtual reality environments.

## Supported VR Headsets
- Meta Quest 2 / Quest 3 / Quest Pro (via Oculus Link / AirLink or native Android APK)
- HTC Vive Pro / Focus 3 / XR Elite
- Valve Index (via SteamVR OpenXR runtime)
- Windows Mixed Reality headsets

## Architectural Highlights
- **Engine Standard:** Unity OpenXR Plugin 1.12.1 conforming to Khronos OpenXR 1.0 specifications.
- **Interaction Model:** XR Interaction Toolkit 3.0.4 with 6DoF ray-cast selection, direct manipulation of topological nodes, and incident timeline scrubbing.
- **Audio:** Spatial audio triggers mapped to anomalous packet flows and critical breach alerts.
- **Scene:** `Assets/Scenes/VRSOCScene.unity` hosting `VRSocManager`, `VRNetworkInteraction`, and `VRIncidentReplay`.
