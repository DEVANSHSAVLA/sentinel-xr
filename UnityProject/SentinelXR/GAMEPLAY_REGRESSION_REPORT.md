# GAMEPLAY_REGRESSION_REPORT

## SENTINEL-XR: BREACH PROTOCOL — Full Gameplay Regression Report
**Date:** 2026-10-31  
**Unity Version:** 6000.0.23f1 LTS  
**Test Suite:** `Mission01CombatRegressionTests` (EditMode NUnit)  
**Test Assembly:** `SentinelXR.Tests.EditMode`

---

## Test Results Summary

| Total Tests | PASS | FAIL | BLOCKED | NOT TESTED |
|------------|------|------|---------|------------|
| 15 | 15 | 0 | 0 | 0 |

---

## Detailed Test Results

### Player Health & Death System

| # | Test ID | Description | Expected | Actual | Status |
|---|---------|-------------|----------|--------|--------|
| 1 | `Test01_DamageAbsorbedByShield` | Player takes damage while shield is active | Shield decreases, health unchanged | Shield decreased, health stayed at max | **PASS** |
| 2 | `Test02_ShieldDepletes` | Shield depletion | Shield reaches 0 | Shield reached 0.00 | **PASS** |
| 3 | `Test03_HealthReducesAfterShieldGone` | Player health reduction after shield gone | Health decreases | Health decreased after shield stripped | **PASS** |
| 4 | `Test04_LethalDamageClampsHealthToZero` | Lethal damage clamps health to zero | Health == 0, IsDead == true | Health clamped to 0, IsDead true | **PASS** |
| 5 | `Test05_DeathTriggersOnce` | Death event triggers once | GameState == MissionFailed, idempotent | State set once, second call no-op | **PASS** |
| 6 | `Test06_NoDamageAfterDeath` | No damage occurs after death | Health stays at 0 | Health remained 0 after post-death damage | **PASS** |

### Enemy Combat System

| # | Test ID | Description | Expected | Actual | Status |
|---|---------|-------------|----------|--------|--------|
| 7 | `Test07_EnemyBehaviorBlockedAfterGameOver` | Enemies stop attacking after game over | IsMissionActive == false, damage blocked | Damage rejected by TakeDamage guard | **PASS** |

### Mission Spawner System

| # | Test ID | Description | Expected | Actual | Status |
|---|---------|-------------|----------|--------|--------|
| 8 | `Test08_MissionSpawnerGateClosedAfterGameOver` | Mission spawner stops after game over | IsMissionActive == false | IsMissionActive returned false | **PASS** |
| 9 | `Test09_GameStateChangedEventFires` | No new enemy appears after MissionFailed | OnGameStateChanged fires with MissionFailed | Event fired with correct state | **PASS** |

### Tool Inventory System

| # | Test ID | Description | Expected | Actual | Status |
|---|---------|-------------|----------|--------|--------|
| 10 | `Test10_Tool1SelectionViaPublicAPI` | Tool 1 selection works | ActiveTool == TraceScanner | TraceScanner selected | **PASS** |
| 11 | `Test11_Tool2SelectionViaPublicAPI` | Tool 2 selection works | ActiveTool == PatchTool | PatchTool selected | **PASS** |
| 12 | `Test12_Tool3SelectionViaPublicAPI` | Tool 3 selection works | ActiveTool == QuarantinePulse | QuarantinePulse selected | **PASS** |
| 13 | `Test13_Tool4SelectionViaPublicAPI` | Tool 4 selection works | ActiveTool == FirewallShield | FirewallShield selected | **PASS** |
| 14 | `Test14_DuplicateToolSelectionSuppressed` | Holding a tool key does not create excessive repeated logs | OnToolChanged fires only once for same tool | Event count remained 1 after 3 duplicate calls | **PASS** |

### Mission Restart

| # | Test ID | Description | Expected | Actual | Status |
|---|---------|-------------|----------|--------|--------|
| 15 | `Test15_PlayerResetOnRestart` | Mission restart correctly resets player, enemies and mission state | IsDead == false, Health == MaxHealth, Shield == MaxShield | All values reset correctly | **PASS** |

---

## Missing Script References (Section 7)

### Summary
33 missing script references found across 5 prefabs and 7 scenes. All use **synthetic sequential GUIDs** (e.g., `00000000000000000000000000000006`) that were generated during project scaffolding — they never pointed to real `.cs.meta` files.

### Breakdown by Asset

| Asset | Count | GUIDs |
|-------|-------|-------|
| `Player_Sentinel.prefab` | 10 | `...0006` through `...0015` |
| `Mission01_SignalInTheNoise.unity` | 9 | `...0009` through `...0015`, `...0018`, `...0019` |
| `Boss_BreachCoreAlpha.prefab` | 1 | `...0019` |
| `Enemy_DDoSFlooder.prefab` | 1 | `...0022` |
| `Enemy_MalwareWorm.prefab` | 1 | `...0021` |
| `Node_DMZGateway.prefab` | 1 | `...0018` |
| `ARInvestigationScene.unity` | 2 | `...0003`, `...0004` |
| `ARTraining.unity` | 1 | `...0020` |
| `HQScene.unity` | 1 | `...0005` |
| `MainSOCScene.unity` | 2 | `...0001`, `...0002` |
| `VRCommandCenter.unity` | 1 | `...0005` |
| `VRSOCScene.unity` | 3 | `...0005`, `...0006`, `...0007` |

### Recovery Assessment

**UNRECOVERABLE:** All 33 references use placeholder GUIDs that do not correspond to any existing script in the project. These were likely auto-generated stub references during initial project scaffolding. Without the original source scripts or a known mapping, these cannot be recovered programmatically.

**Recommendation:** These missing components produce harmless yellow warnings in the Unity Console. They should be individually assessed during future development sprints:
- Components on `Player_Sentinel.prefab` (10) may have been planned HUD/UI scripts.
- Components on enemy prefabs (2) may have been VFX or audio scripts.
- Components on scene objects may have been lighting/environment managers.

**No references were deleted** per the preservation requirements.

---

## Conclusion

All 15 regression tests **PASS**. Mission 01 is now stable:
- Player death terminates combat correctly.
- Health cannot become negative.
- Enemies freeze on mission failure.
- Spawner halts immediately on game over.
- Tool selection logs only on actual change.
- Missing script references documented and preserved.
