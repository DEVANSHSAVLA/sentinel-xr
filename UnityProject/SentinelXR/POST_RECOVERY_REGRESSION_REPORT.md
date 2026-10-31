# POST_RECOVERY_REGRESSION_REPORT

## SENTINEL-XR: BREACH PROTOCOL — Post-Recovery Gameplay Regression Report
**Date:** 2026-10-31  
**Project:** `UnityProject/SentinelXR`  
**Unity Version:** 6000.0.23f1 LTS  
**Audit Scope:** End-to-End Mission 01 Gameplay & Core Systems  
**Overall Regression Result:** **PASS** (15 of 15 Systems Verified)

---

## 1. System-by-System Regression Test Matrix

| # | System / Feature | Test Description | Expected Behavior | Observed Result | Status |
|---|------------------|------------------|-------------------|-----------------|--------|
| 1 | **Bootstrap Startup** | Application launch through `BootstrapController` and `AppBootstrapper` | Sets target frame rate (60 FPS), timescale (1.0), transitions state to `MainMenu` | Clean initialization; no missing singletons | **PASS** |
| 2 | **Main Menu** | Navigation in `MainMenuScene` | UI buttons trigger scene loads through `SceneTransitionManager` | Transition triggers `Mission01_SignalInTheNoise` | **PASS** |
| 3 | **Mission 01 Load** | Scene load and GameObject binding | Scene loads without missing script warnings; sets state to `InMission` | Cursor locks; 0 missing component warnings | **PASS** |
| 4 | **Player Movement & Look** | WASD, Shift sprint, Space jump, mouse look | CharacterController moves, applies gravity (-19.62), clamped pitch look (-85 to +85) | Smooth movement and camera rotation | **PASS** |
| 5 | **Tool Switching** | Keyboard keys `1`, `2`, `3`, `4` and mouse scroll | Edge-triggered tool selection, single log per unique switch, energy display updates | Switches tools cleanly; zero duplicate log spam | **PASS** |
| 6 | **Defensive Tool Actions** | Activation of all 4 restored tool components | - `TraceScanner`: Decodes anomaly telemetry<br>- `PatchTool`: Restores node integrity<br>- `QuarantinePulse`: Stuns malware in radius<br>- `FirewallShield`: Blocks projectiles | All 4 tool action scripts execute and consume tool energy correctly | **PASS** |
| 7 | **Health & Shield** | Damage absorption, depletion, clamping | Shield absorbs first (100 -> 0); overflow depletes health; health clamped to [0, max] | Clamped to 0 minimum; health never negative | **PASS** |
| 8 | **Gateway Interaction** | Approaching and patching `Node_DMZGateway` | `PlayerInteraction` detects node within 4m; `PatchTool` repairs node to 100% | Integrity restored; triggers breach core shield drop | **PASS** |
| 9 | **Threat Spawning** | `Mission01Controller` wave spawns | Spawns MalwareWorm, DDoSFlooder, TrojanInfiltrator up to max simultaneous limit | Spawner respects wave interval and max limits | **PASS** |
| 10 | **Mission Objectives** | Progression from `OBJ-01` through `OBJ-04` | Objectives evaluate in sequence upon telemetry, kills, gateway repair, and boss purge | Each objective completes and fires events | **PASS** |
| 11 | **Boss Encounter** | Combat against `BreachCoreAlpha` (`BreachCoreBoss`) | Shield active until gateway patched; susceptible to damage once shield drops | Shield drops upon OBJ-03; health depletes | **PASS** |
| 12 | **Pause & Resume** | Pressing `Escape` to toggle `GameMenuManager` | `Time.timeScale` set to 0 on pause, 1 on resume; cursor unlocks | Game pauses cleanly; resume restores state | **PASS** |
| 13 | **Mission Failure** | Player lethal damage or subnet integrity collapse | `Die()` executes exactly once; `TriggerGameOver()` transitions to `MissionFailed`; enemies stop; spawner halts | Enemies freeze, damage ceases, spawner terminates | **PASS** |
| 14 | **Mission Debrief** | Post-mission telemetry and forensic seal | Displays threats eliminated, integrity preserved, time elapsed, and deterministic SHA-256 seal | Debrief card renders; SHA-256 seal computed | **PASS** |
| 15 | **Mission Restart** | Reloading scene after failure or victory | Player `isDead` resets, health/shield reset to max, enemy lists clear | Clean state reset; full replayability verified | **PASS** |

---

## 2. In-Depth Regression Findings

### 2.1 Tool Action Functionality (Restored Components)
Before GUID recovery, the tool action scripts were missing, meaning pressing keys `1`–`4` changed the `activeTool` enum in `ToolInventory` but did not execute actual tool behaviors. Now that `TraceScanner`, `PatchTool`, `QuarantinePulse`, and `FirewallShield` are active:
- `TraceScanner` executes raycasts to detect corrupted cyber nodes and logs decoded incident telemetry.
- `PatchTool` injects integrity units into damaged network nodes.
- `QuarantinePulse` queries nearby `EnemyBase` colliders and applies `ApplyStun(3.0f)`, pausing enemy AI.
- `FirewallShield` instantiates a barrier with tag `"FirewallShield"`, causing incoming `MaliciousPacketProjectile` instances to deflect and destroy harmlessly.

### 2.2 Combat & Game Over Stability
- All Phase 1 guards (`isDead`, `GameManager.IsMissionActive`, `Mathf.Clamp`, `StopSpawning()`) continue to operate flawlessly in conjunction with the restored components.
- No damage leaks occur after player death.
- No enemies spawn after mission failure.

---

## 3. Regression Verdict

| Status | Count |
|--------|-------|
| **PASS** | **15** |
| **FAIL** | **0** |
| **BLOCKED** | **0** |
| **NOT TESTED** | **0** |

Mission 01 is stable, fully playable, and complete with all intended components operational.
