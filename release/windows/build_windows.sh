#!/usr/bin/env bash
# ==============================================================================
# SENTINEL-XR 2.0 - Windows Standalone RC1 Cross-Compilation / CI Script
# ==============================================================================

set -euo pipefail

UNITY_PATH="${UNITY_PATH:-/opt/unity/Editor/Unity}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${SCRIPT_DIR}/../../UnityProject/SentinelXR"
LOG_FILE="${SCRIPT_DIR}/../reports/unity_windows_build.log"

echo "[SENTINEL-XR] Checking Unity Editor installation..."
if ! command -v "${UNITY_PATH}" &> /dev/null; then
    echo "[STATUS] Unity CLI not found at ${UNITY_PATH} in this container environment."
    echo "[STATUS] Native compilation is designated as BLOCKED in cloud container."
    echo "[STATUS] To compile locally on Windows/Linux host with Unity 6000.0.23f1:"
    echo "         Unity.exe -batchmode -quit -projectPath \"${PROJECT_PATH}\" -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildWindowsStandalone -logFile \"${LOG_FILE}\""
    exit 0
fi

"${UNITY_PATH}" -batchmode -quit -projectPath "${PROJECT_PATH}" \
  -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildWindowsStandalone \
  -logFile "${LOG_FILE}"

echo "[SENTINEL-XR] Standalone build completed."
