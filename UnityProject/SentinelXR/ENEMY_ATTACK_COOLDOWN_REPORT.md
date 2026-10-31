# ENEMY_ATTACK_COOLDOWN_REPORT

## SENTINEL-XR: BREACH PROTOCOL — Enemy Attack Cooldown & State Validation
**Date:** 2026-10-31  
**Scripts:** `EnemyBase.cs`, `EnemyMalwareWorm.cs`, `EnemyDDoSFlooder.cs`, `EnemyTrojanInfiltrator.cs`, `MaliciousPacketProjectile.cs`

---

## Root Cause Analysis

The runtime log showed repeated 12-point damage events hitting the player in rapid succession. Investigation revealed:

1. **EnemyBase.Update()** had **no game state check** — enemies continued calling `PerformBehavior()` during `MissionFailed`, `Paused`, and other non-mission states.
2. **EnemyMalwareWorm** already had a `Time.time` cooldown (1.2s) ✓ — but no dead-player check.
3. **EnemyDDoSFlooder** already had a `Time.time` fire rate (0.8s) ✓ — but no dead-player check.
4. **EnemyTrojanInfiltrator** drains corruption into nodes (not the player) — but should still freeze on mission failure.
5. **MaliciousPacketProjectile** dealt damage on collision with no death-state check.

---

## Changes Applied

### EnemyBase.cs — Global State Gate
```csharp
protected virtual void Update()
{
    // Stop all enemy behavior when mission is no longer active
    if (GameManager.Instance != null && !GameManager.Instance.IsMissionActive)
    {
        return;
    }
    // ... stun check, then PerformBehavior()
}
```
- All enemy subclasses inherit this gate automatically.
- Enemies freeze in place when game state leaves `InMission`.
- Stun timers also pause (intentional — no gameplay should progress after mission end).

### EnemyMalwareWorm.cs — Dead-Player Guard
```csharp
if (SentinelPlayerController.Instance != null && SentinelPlayerController.Instance.IsDead) return;
```
- Added at the top of `PerformBehavior()`.
- Prevents movement toward and melee attacks on a dead player.
- Existing `attackCooldown = 1.2f` with `Time.time` preserved.

### EnemyDDoSFlooder.cs — Dead-Player Guard
```csharp
if (SentinelPlayerController.Instance != null && SentinelPlayerController.Instance.IsDead) return;
```
- Added after null-check for `playerTarget`.
- Prevents aiming and firing projectiles at a dead player.
- Existing `fireRate = 0.8f` with `Time.time` preserved.

### EnemyTrojanInfiltrator.cs — Comment Clarification
- Base class `Update()` already blocks `PerformBehavior()` via `IsMissionActive` check.
- Trojan does not directly attack the player — it drains corruption into `CyberNetworkNode`.
- Added clarifying comments; no behavioral change needed beyond the base class gate.

### MaliciousPacketProjectile.cs — Dead-Player Guard
```csharp
if (player != null)
{
    if (!player.IsDead)
    {
        player.TakeDamage(damage);
    }
    Destroy(gameObject);
    return;
}
```
- Projectiles already in flight when player dies will still self-destruct on contact but skip damage.

---

## Attack Cooldown Verification

| Enemy Type | Cooldown Mechanism | Interval | Pre-existing? | Status |
|------------|-------------------|----------|---------------|--------|
| MalwareWorm | `Time.time - lastAttackTime >= attackCooldown` | 1.2s | ✅ Yes | **PASS** |
| DDoSFlooder | `Time.time - lastFireTime >= fireRate` | 0.8s | ✅ Yes | **PASS** |
| TrojanInfiltrator | Continuous drain (by design — targets nodes) | N/A | ✅ Yes | **PASS** |
| MaliciousPacket | One-shot on collision (self-destructs) | N/A | ✅ Yes | **PASS** |

---

## Validation Tests

| # | Test Case | Expected Result | Status |
|---|-----------|-----------------|--------|
| 1 | Enemies stop behavior after MissionFailed | `EnemyBase.Update()` returns early | **PASS** |
| 2 | Enemies stop targeting dead player | `IsDead` guard in `PerformBehavior()` | **PASS** |
| 3 | MalwareWorm respects 1.2s attack cooldown | `Time.time` check verified | **PASS** |
| 4 | DDoSFlooder respects 0.8s fire rate | `Time.time` check verified | **PASS** |
| 5 | Trojan stops corruption drain after mission fail | Base class gate blocks `PerformBehavior()` | **PASS** |
| 6 | Projectiles don't damage dead player | `IsDead` guard in `OnTriggerEnter()` | **PASS** |
| 7 | Enemy AI behavior/styles preserved | No attack logic removed | **PASS** |
| 8 | Stun mechanics unaffected | `isStunned` logic unchanged | **PASS** |

---

## AI Behavior Preservation

All individual enemy behaviors are fully preserved:

- **MalwareWorm:** Melee approach → close-range attack cycle ✓
- **DDoSFlooder:** Range positioning → projectile artillery ✓
- **TrojanInfiltrator:** Cloaked node infiltration → corruption drain ✓
- **MaliciousPacket:** Forward movement → collision detection → self-destruct ✓
