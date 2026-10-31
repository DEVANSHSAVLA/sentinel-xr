@echo off
REM ==============================================================================
REM SENTINEL-XR 2.0 - Windows Standalone Release Candidate 1 Build Script
REM ==============================================================================

set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe"
set PROJECT_PATH=%~dp0..\..\UnityProject\SentinelXR
set LOG_FILE=%~dp0..\reports\unity_windows_build.log

echo [SENTINEL-XR] Initiating Windows Standalone RC1 Build Pipeline...
echo [SENTINEL-XR] Project Path: %PROJECT_PATH%
echo [SENTINEL-XR] Target Executable: %~dp0SentinelXR.exe

if not exist %UNITY_PATH% (
    echo [ERROR] Unity Editor binary not found at %UNITY_PATH%!
    echo Please ensure Unity 6000.0.23f1 is installed or update UNITY_PATH.
    exit /b 1
)

%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod SentinelXR.Editor.SentinelBuildPipeline.BuildWindowsStandalone -logFile "%LOG_FILE%"

if %ERRORLEVEL% equ 0 (
    echo [SUCCESS] Windows Standalone Build completed successfully!
    echo Deliverables packaged in %~dp0
) else (
    echo [FAILURE] Build failed. Inspect log at %LOG_FILE%
    exit /b %ERRORLEVEL%
)
