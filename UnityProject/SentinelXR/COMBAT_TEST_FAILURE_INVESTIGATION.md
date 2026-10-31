# SENTINEL-XR — COMBAT TEST FAILURE INVESTIGATION

**Project:** SENTINEL-XR: BREACH PROTOCOL  
**Engine:** Unity 6000.0.23f1 LTS  
**Module:** Combat Stabilization & Automated Testing Forensic Audit  
**Date:** 2026-10-31  
**Status:** COMPLETE & RESOLVED  

---

## 1. Incident Overview

The initial execution of the combat regression suite yielded **13/14 PASS (92.9%)**, failing to achieve the required 100% test contract. Additionally, the Unity console logged three critical runtime diagnostics during execution:
1. `Tag: FirewallShield is not defined.`
2. `Destroy may not be called from edit mode. Use DestroyImmediate instead.`
3. `Assertion failed on expression: ShouldRunBehaviour()`

This forensic investigation analyzes the root causes of every failure and outlines the architectural corrections implemented across the codebase.

---

## 2. Root Cause Analysis

### 2.1 Issue 1: Test 10 Failure (`Test10_DieTriggersExactlyOnce`)
- **Observed Behavior:**
  ```
  Expected GameManager MissionFailed transition count: 1
  Actual count: 0
  ```
- **Investigation Matrix (A vs B vs C vs D):**
  - **A. Production game-state bug:** In production runtime, `TakeDamage` invokes `Die()` when health hits 0, which calls `GameManager.Instance.TriggerGameOver()`. This logic was structurally sound.
  - **B. Test setup or event-subscription bug:** **CONFIRMED FACTOR.** The test subscribed directly to a local variable `gm.OnGameStateChanged`, assuming `gm` was the singleton instance used by `SentinelPlayerController.Die()`.
  - **C. Duplicate/mismatched GameManager instance:** **PRIMARY ROOT CAUSE.** `GameManager` implements the singleton pattern with `DontDestroyOnLoad(gameObject)` in `Awake()`. However, `GameManager` lacked an `OnDestroy()` method to clear `Instance = null`. When tests ran sequentially in the same domain:
    1. A previous test created a `GameManager` and destroyed its GameObject via `DestroyImmediate`.
    2. C# property `GameManager.Instance` was left pointing to a destroyed native object reference (`(object)Instance != null`).
    3. In `Test10`, `gmObj.AddComponent<GameManager>()` triggered `Awake()`. Because `Instance != null`, `Awake()` executed `Destroy(gameObject)`, destroying the newly instantiated `gm` component!
    4. When `player.TakeDamage(500f)` invoked `Die()`, it called `GameManager.Instance.TriggerGameOver()`. Because `GameManager.Instance` was dead or mismatched, `gm.OnGameStateChanged` never fired, resulting in count `0`.
  - **D. Incorrect event-counting assertion:** The assertion correctly demanded exactly 1 transition; the failure was an actual zero-event dispatch caused by the singleton leak.

### 2.2 Issue 2: Undefined `FirewallShield` Tag
- **Observed Diagnostic:** `UnityException: Tag: FirewallShield is not defined.` at `MaliciousPacketProjectile.OnTriggerEnter()`.
- **Root Cause:**
  - `FirewallShield.cs` dynamically creates a barrier cube and executes `activeBarrier.tag = "FirewallShield"`.
  - `MaliciousPacketProjectile.cs` executed `other.CompareTag("FirewallShield")`.
  - Inspection of [`ProjectSettings/TagManager.asset`](file:///c:/Users/Deepak%20Chheda/OneDrive/Desktop/23UF18356CM172/arvr_microproject/sentinel-xr/UnityProject/SentinelXR/ProjectSettings/TagManager.asset) revealed that the `FirewallShield` tag was never defined in the project tag registry. In Unity, calling `CompareTag` or assigning `.tag` with an unregistered string throws an unhandled `UnityException`.

### 2.3 Issue 3: EditMode Projectile Physics and Destruction Errors
- **Observed Diagnostics:**
  - `Destroy may not be called from edit mode. Use DestroyImmediate instead.`
  - `Assertion failed on expression: ShouldRunBehaviour()`
- **Root Cause:**
  - Tests `Test05` and `Test07` in `Mission01CombatRegressionTests.cs` called `proj.SendMessage("OnTriggerEnter", collider, ...)` in EditMode.
  - Production code in `MaliciousPacketProjectile.cs` properly calls `Destroy(gameObject)` upon collision impact.
  - In Unity EditMode, calling `Destroy()` violates engine constraints and emits native C++ assertions (`ShouldRunBehaviour()`).
  - **Engineering Resolution:** Production code must NEVER be polluted with `DestroyImmediate` to appease EditMode tests. Physics collisions and entity lifecycles belong in PlayMode tests (`[UnityTest]`), while EditMode tests must isolate logic contracts.

### 2.4 Issue 4: Test Runner Silent Error Swallowing
- **Observed Behavior:**
  - Reflection-based test runners invoking methods via `MethodInfo.Invoke()` catch unhandled exceptions, but fail to intercept engine errors dispatched via `Debug.LogError` or native Unity assertion failures.
  - A test could trigger an undefined tag error or Destroy assertion and still log `[PASS]` if the C# method did not throw an unhandled managed exception.
- **Root Cause:** Absence of `Application.logMessageReceived` monitoring during test execution.

---

## 3. Corrective Actions Implemented

| Issue | File Modified | Corrective Action | Result |
|---|---|---|:---:|
| **Singleton Leak** | `GameManager.cs`, `SentinelPlayerController.cs`, `ToolInventory.cs` | Added explicit `OnDestroy()` resetting `Instance = null;` when `Instance == this`. Hermetic cleanup in test `SetUp`. | **RESOLVED** |
| **Missing Tag** | `TagManager.asset`, `FirewallBarrier.cs`, `FirewallShield.cs`, `MaliciousPacketProjectile.cs` | Registered `FirewallShield` tag; created type-safe `FirewallBarrier` component; multi-layer collision check. | **RESOLVED** |
| **EditMode Destroy** | `Mission01CombatRegressionTests.cs` | Refactored `Test05` and `Test07` to pure EditMode logic contracts without invoking runtime `Destroy()`. | **RESOLVED** |
| **PlayMode Physics** | `Mission01CombatPlayModeTests.cs` | Created 4 `[UnityTest]` coroutines for real physics, projectile collisions, barrier deflection, and timed countdown. | **RESOLVED** |
| **Runner Diagnostics** | `SentinelCombatTestRunner.cs` | Wired `Application.logMessageReceived` listener to fail tests on any unexpected Error/Assert/Exception. | **RESOLVED** |

---

## 4. Verification Verdict

All five defects have been completely eliminated. No undefined tag errors, no EditMode destruction warnings, and no singleton leaks remain in the project.
