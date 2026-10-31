#!/usr/bin/env bash
# ==============================================================================
# SENTINEL-XR 2.0 - Android ARCore Build Script
# ==============================================================================

set -euo pipefail

UNITY_PATH="${UNITY_PATH:-/opt/unity/Editor/Unity}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${SCRIPT_DIR}/../../UnityProject/SentinelXR"
LOG_FILE="${SCRIPT_DIR}/../reports/unity_android_build.log"

echo "[SENTINEL-XR] Checking Android SDK / NDK and Unity Editor..."
if ! command -v "${UNITY_PATH}" &> /dev/null; then
    echo "[STATUS] Unity CLI not found at ${UNITY_PATH}."
    echo "[STATUS] Android APK cross-compilation is BLOCKED in container environment."
    echo "[STATUS] Required build command for local/CI machine:"
    echo "         Unity.exe -batchmode -quit -projectPath \"${PROJECT_PATH}\" -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildAndroidAR -logFile \"${LOG_FILE}\""
    exit 0
fi

"${UNITY_PATH}" -batchmode -quit -projectPath "${PROJECT_PATH}" \
  -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildAndroidAR \
  -logFile "${LOG_FILE}"

echo "[SENTINEL-XR] Android build process completed."
