# SENTINEL-XR — MISSION 01 GATEWAY DIFFICULTY & PACING BALANCING REPORT

**Project:** SENTINEL-XR: BREACH PROTOCOL  
**Engine:** Unity 6 LTS (6000.0.23f1)  
**Target Level:** Mission 01: Signal in the Noise (`Node_DMZGateway`)  
**Execution Timestamp:** 2026-10-31 08:24:00 UTC  
**Target Platform:** Windows Standalone / URP 17.0.3  

---

## 1. Problem Investigation & Runtime Evidence

In previous playtesting logs of Mission 01, runtime diagnostics captured a severe difficulty spike that resulted in unfair mission failure:

```
[00:18.42] [ObjectiveManager] Objective 01 (Trace Breach Signal) Completed!
[00:19.10] [Mission01Controller] Spawning Infiltration Wave: TrojanInfiltrator_2
[00:20.15] [TrojanInfiltrator_2] Decloaking at target node: Node_DMZGateway
[00:20.15] [Node_DMZGateway] Infiltration corruption injection started! Current Integrity: 35.0%
[00:21.15] [Node_DMZGateway] Integrity: 27.0% (-8.0/s)
[00:22.15] [Node_DMZGateway] Integrity: 19.0% (-8.0/s)
[00:23.15] [Node_DMZGateway] Integrity: 11.0% (-8.0/s)
[00:24.15] [Node_DMZGateway] Integrity: 3.0% (-8.0/s)
[00:24.52] [Node_DMZGateway] Integrity: 0.0% -> CRITICAL BREACH!
[00:24.55] [GameManager] Transitioning state: InMission -> MissionFailed
```

### Forensic Root Cause:
1. **Unforgiving Starting Integrity:** `MissionObjectiveManager.cs` deliberately lowered `Node_DMZGateway` integrity to **35%** upon mission initialization to simulate existing breach damage.
2. **Aggressive Drain Rate:** `EnemyTrojanInfiltrator.cs` applied continuous corruption damage at **8.0 integrity points per second** ($8.0\text{ HP/s}$).
3. **Zero Telegraph Grace Period:** The instant the Trojan arrived at the gateway collider, corruption injection began immediately ($0.0\text{s}$ grace period).
4. **Physical Distance to Node:** The player spawns on the observation upper deck at coordinates $(0, 1.1, -10)$, while `Node_DMZGateway` is located approximately $22\text{ meters}$ away in the server room. At standard sprint speed ($5.0\text{ m/s}$), reaching the node requires $\sim 4.4\text{ seconds}$. Thus, the gateway was completely destroyed before the player could physically reach it.
5. **No Directional Telegraph:** The player had no visual waypoint arrow indicating the gateway's location or its impending destruction.

---

## 2. Difficulty & Pacing Balancing Modifications

To create a tense yet fair cyber-defense experience, all balancing parameters were re-engineered and exposed as configurable serialized fields:

| Parameter | Baseline Value | Tuned Balance Value | Engineering & Design Rationale |
|---|---|---|---|
| **Gateway Starting Integrity** | `35.0%` | **`50.0%`** | Provides an initial buffer while player explores observation deck and inspects HUD. |
| **Trojan Corruption Drain Rate** | `8.0 HP/s` | **`3.5 HP/s`** | Extends the time-to-failure window from $4.3\text{s}$ to **$14.3\text{ seconds}$**, enabling tactical interception. |
| **Trojan Arrival Telegraph Grace Period** | `0.0s (Instant)` | **`3.5s (Grace Period)`** | Trojan decloaks, sounds an audio alert klaxon, and flashes amber before starting injection. |
| **Objective Gating (`delayAttackUntilObjectiveActive`)** | Disabled | **`Enabled (true)`** | Trojan stalks towards the gateway but holds corruption injection until Objective 01 is completed or player damages it. |
| **Patch Tool Continuous Repair Rate** | `25.0 HP/s` | **`25.0 HP/s`** | Net positive repair rate of $+21.5\text{ HP/s}$ when player counters Trojan drain directly. |
| **Manual Quick-Patch [E]** | `35.0 HP/press` | **`35.0 HP/press`** | High-impact burst repair rewarding close-quarters physical positioning. |
| **Objective 02 Target Integrity** | `100.0%` | **`100.0%`** | Requires deliberate full restoration to secure DMZ sector before advancing. |

> [!NOTE]
> The gateway is **not** invulnerable. If a player ignores the audio-visual warning and stays idle for more than $14.3\text{ seconds}$, the DMZ Gateway will collapse, triggering `GameManager.TriggerGameOver()` and displaying the Mission Failed debrief screen as designed.

---

## 3. Directional Waypoint Navigation & HUD Telemetry

To ensure immediate situational awareness, `MissionHUD.cs` was upgraded with real-time dynamic guidance:

### A. Dynamic Waypoint Navigation Guide (`GetGatewayNavGuide`)
The HUD calculates the player camera's forward vector relative to `Node_DMZGateway` in world space and computes a 2D planar bearing:
- **`[▲ XXm]` Ahead:** Target is within $\pm 35^\circ$ of the crosshairs.
- **`[◄ XXm]` Left:** Target is to the operator's left ($35^\circ$ to $135^\circ$).
- **`[► XXm]` Right:** Target is to the operator's right ($-35^\circ$ to $-135^\circ$).
- **`[▼ XXm]` Behind:** Target is behind the operator ($> 135^\circ$).

### B. Pulsing Critical Alert Banner
When `Node_DMZGateway` falls below $45\%$ integrity or takes active damage, an animated amber-red banner flashes across the upper viewport:
```
⚠ CRITICAL ALERT: [NODE_DMZGATEWAY] UNDER ACTIVE INFILTRATION! INTEGRITY: 48% — ENGAGE PATCH TOOL! [▲ 14m]
```

### C. Dedicated Telemetry Display
The top-right telemetry panel now houses a dedicated dual-bar readout:
- **Subnet Integrity:** Global network health percentage.
- **DMZ Gateway Integrity:** Real-time gauge colored green ($>70\%$), amber ($30\% - 70\%$), or flashing critical red ($<30\%$).
- **Status Indicator:** `SECURE` (100%), `UNDER ATTACK` (taking drain), `COMPROMISED` (0%).

---

## 4. Patch Tool Interaction & State Integration

```
+-----------------------------------------------------------------------------------+
|                        PATCH TOOL REPAIR STATE MACHINE                            |
+-----------------------------------------------------------------------------------+
| 1. Player engages Patch Tool (Key [2] or Scroll)                                   |
|    └─► ToolInventory dispatches OnToolSelected(CyberToolType.PatchTool)           |
|    └─► HUD displays "TOOL ENGAGED: [PATCH TOOL]" toast notification               |
|                                                                                   |
| 2. Aiming at Node_DMZGateway within 8.0 meters:                                   |
|    └─► Raycast identifies CyberNetworkNode component with EdgeGateway role        |
|    └─► HUD displays reticle prompt: "HOLD [LMB] TO PATCH NODE (+25 HP/s)"         |
|    └─► Proximity prompt: "PRESS [E] QUICK-PATCH (+35 HP)"                         |
|                                                                                   |
| 3. Execution & Event Dispatch:                                                    |
|    └─► CyberNetworkNode.RepairNode(amount) invoked                                |
|    └─► CurrentIntegrity increments, clamped to maxIntegrity (100.0)               |
|    └─► Static event OnNodeIntegrityChanged dispatched to HUD and ObjectiveManager |
|    └─► Audio: Procedural continuous patch harmonic hum plays                      |
|                                                                                   |
| 4. Objective 02 Resolution:                                                       |
|    └─► When CurrentIntegrity reaches 100.0%:                                      |
|         └─► CyberNetworkNode.OnNodeSecured dispatched                             |
|         └─► MissionObjectiveManager.Instance.CompleteObjective("OBJ-02")          |
|         └─► Objective 03 (Contain Threat Infiltration) activates                  |
+-----------------------------------------------------------------------------------+
```

---

## 5. Requirements Verification & Classification Matrix

| # | System Contract / Requirement | Test / Verification Method | Result | Notes & Verification Details |
|---|---|---|---|---|
| 1 | Prominent gateway attack warning | `MissionHUD.DrawGatewayWarningBanner` unit check | **PASS** | Flashing banner renders with correct node identifier and alert color. |
| 2 | Directional waypoint navigation | Bearing calculation unit test across 4 quadrants | **PASS** | Correct directional glyph (`◄`, `▲`, `►`, `▼`) and metric distance rendered. |
| 3 | Gateway starting integrity = 50% | `MissionObjectiveManager.gatewayStartingIntegrity` | **PASS** | Initialized at 50% instead of former 35%. |
| 4 | Trojan drain rate tuned to 3.5 HP/s | `EnemyTrojanInfiltrator.corruptionDrainRate` | **PASS** | Reduced from 8.0 HP/s; gives 14.3s defense window. |
| 5 | Arrival telegraph grace period = 3.5s | `EnemyTrojanInfiltrator.arrivalTelegraphDuration` | **PASS** | 3.5s decloak and audio klaxon before corruption starts. |
| 6 | Objective gating on Trojan attack | `delayAttackUntilObjectiveActive` contract check | **PASS** | Trojan holds attack until Objective 01 completed or damaged. |
| 7 | Configurable serialized parameters | Unity Inspector serialized fields audit | **PASS** | All values exposed in Inspector for easy designer balancing. |
| 8 | Gateway vulnerability preserved | Pacing simulation ($14.3\text{s}$ inactivity test) | **PASS** | Gateway successfully fails if ignored; invulnerability rejected. |
| 9 | Patch Tool repairs gateway | `CyberNetworkNode.RepairNode()` unit contract | **PASS** | Continuous (+25/s) and burst (+35) restore integrity cleanly. |
| 10 | Real-time HUD telemetry | `MissionHUD` Top-Right telemetry rendering | **PASS** | Displays live DMZ Gateway gauge alongside subnet integrity. |
| 11 | Objective 02 advancement | State machine inspection on 100% integrity | **PASS** | `CompleteObjective("OBJ-02")` only fires upon 100% repair. |
| 12 | End-to-end full manual playthrough | Human play session across full mission | **NOT TESTED** | Manual end-to-end human speedrun pending user confirmation. |

---

## 6. Conclusion

Mission 01 gateway pacing has been transformed from an instant-failure bottleneck into an engaging, readable, and balanced defensive objective. The player has adequate time ($14.3\text{s}$) and unambiguous navigation cues to reach the gateway and employ the Patch Tool effectively.
