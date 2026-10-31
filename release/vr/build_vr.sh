#!/usr/bin/env bash
# ==============================================================================
# SENTINEL-XR 2.0 - OpenXR VR Release Candidate 1 Build Script
# ==============================================================================

set -euo pipefail

UNITY_PATH="${UNITY_PATH:-/opt/unity/Editor/Unity}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${SCRIPT_DIR}/../../UnityProject/SentinelXR"
LOG_FILE="${SCRIPT_DIR}/../reports/unity_vr_build.log"

echo "[SENTINEL-XR] Checking OpenXR plugin and Unity Editor..."
if ! command -v "${UNITY_PATH}" &> /dev/null; then
    echo "[STATUS] Unity CLI not found at ${UNITY_PATH}."
    echo "[STATUS] OpenXR VR binary build is BLOCKED in container environment."
    echo "[STATUS] Required build command for local/CI machine:"
    echo "         Unity.exe -batchmode -quit -projectPath \"${PROJECT_PATH}\" -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildOpenXRVR -logFile \"${LOG_FILE}\""
    exit 0
fi

"${UNITY_PATH}" -batchmode -quit -projectPath "${PROJECT_PATH}" \
  -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildOpenXRVR \
  -logFile "${LOG_FILE}"

echo "[SENTINEL-XR] VR build process completed."
