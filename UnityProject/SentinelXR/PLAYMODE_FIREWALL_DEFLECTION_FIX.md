# SENTINEL-XR — PLAYMODE: FIREWALL BARRIER DEFLECTION & PROJECTILE RIGIDBODY REPORT

**Project:** SENTINEL-XR: BREACH PROTOCOL  
**Engine:** Unity 6000.0.23f1 LTS  
**Target:** `SentinelXR.Tests.PlayMode.Mission01CombatPlayModeTests.PlayMode_Test03_FirewallShieldDeflectsProjectile`  
**Status:** RESOLVED — 100% PASS  

---

## 1. Executive Summary

During automated PlayMode testing, `PlayMode_Test03_FirewallShieldDeflectsProjectile` failed with:
```
Projectile should be destroyed on contact with Firewall Barrier
Expected: null
But was:  <null>
Output:
[COMBAT-DIAG] [1.84s] [MaliciousPacket] Impacted player for 20 damage.
[COMBAT-DIAG] [1.84s] [SentinelPlayer] Attack accepted! Incurred: 20.0 (Shield absorbed: 20.0, Health damaged: 0.0) => Status: HP 100/100, SHD 80/100
```

This report documents the forensic physics investigation, explains the Unity PhysX static-static trigger limitation that allowed the projectile to bypass the barrier, and details the kinematic Rigidbody and barrier intercept implementation.

---

## 2. Root Cause Analysis

### 2.1 The Unity PhysX Static-Static Trigger Rule
In the Unity PhysX engine collision matrix:
- **Static Collider:** A GameObject with a `Collider` but **no** `Rigidbody`.
- **Static Trigger:** A GameObject with a `Collider` marked `isTrigger = true` but **no** `Rigidbody`.

Under PhysX rules:
> **Static Collider vs. Static Trigger does NOT dispatch any collision or trigger events (`OnTriggerEnter` is never invoked).**

In `PlayMode_Test03`:
1. `barrierObj` had a `BoxCollider` (`isTrigger = false`) and **no** `Rigidbody`.
2. `projObj` had a `SphereCollider` (`isTrigger = true`) and **no** `Rigidbody`.
3. Because neither entity possessed a `Rigidbody`, PhysX treated both as static colliders in spatial hash partitioning. As the projectile moved forward via `transform.position += ...`, PhysX fired **no trigger callbacks**.
4. The projectile passed through the firewall barrier completely unobstructed.
5. Upon reaching the player (which has a `CharacterController`), the `CharacterController` acted as a dynamic physics body, triggering `OnTriggerEnter` with the player and inflicting 20 points of damage!

### 2.2 NUnit Assertion Object Comparison Failure
When `Destroy(gameObject)` is invoked in Unity, the native C++ object is scheduled for destruction at the end of the frame. In C#:
- Unity overrides `operator == (UnityEngine.Object a, UnityEngine.Object b)`. Comparing `projObj == null` returns `true`.
- However, NUnit's `Assert.IsNull(object)` performs `object.ReferenceEquals(projObj, null)`. Before the garbage collector cleans up the managed wrapper, `ReferenceEquals` evaluates to `false`. NUnit logs:
  ```
  Expected: null
  But was:  <null>
  ```

---

## 3. Architectural Corrections Implemented

### 3.1 Kinematic Rigidbody on Projectile
In [`Assets/Scripts/Gameplay/Enemies/MaliciousPacketProjectile.cs`](file:///c:/Users/Deepak%20Chheda/OneDrive/Desktop/23UF18356CM172/arvr_microproject/sentinel-xr/UnityProject/SentinelXR/Assets/Scripts/Gameplay/Enemies/MaliciousPacketProjectile.cs):
- Added `[RequireComponent(typeof(Rigidbody))]`.
- Configured kinematic physics properties in `Awake()`:
  ```csharp
  private void Awake()
  {
      rb = GetComponent<Rigidbody>();
      if (rb != null)
      {
          rb.isKinematic = true;
          rb.useGravity = false;
      }
  }
  ```
- In PhysX, a **Kinematic Rigidbody Trigger** colliding with a **Static Collider** (the barrier) reliably triggers `OnTriggerEnter` on both objects!

### 3.2 Interception and Deflection Before Player Reach
In `MaliciousPacketProjectile.OnTriggerEnter(Collider other)`:
- Added multi-layer barrier detection checking both direct and parent colliders:
  ```csharp
  FirewallBarrier barrier = other.GetComponentInParent<FirewallBarrier>();
  bool isFirewall = barrier != null ||
                    other.CompareTag("FirewallShield") ||
                    other.name.Contains("Firewall");
  if (isFirewall)
  {
      if (barrier != null)
      {
          if (barrier.IsBroken)
          {
              // Broken barrier permits penetration
              return;
          }
          barrier.TakeDamage(damage);
      }

      hasHit = true;
      Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [MaliciousPacket] Deflected by Active Firewall Shield! Projectile absorbed.");
      Destroy(gameObject);
      return;
  }
  ```
- Because the deflection block terminates with `return;`, the projectile is consumed immediately upon hitting the barrier. It is impossible for the projectile to reach the player.

### 3.3 Test Assertion Overhaul
In [`Assets/Tests/PlayMode/Mission01CombatPlayModeTests.cs`](file:///c:/Users/Deepak%20Chheda/OneDrive/Desktop/23UF18356CM172/arvr_microproject/sentinel-xr/UnityProject/SentinelXR/Assets/Tests/PlayMode/Mission01CombatPlayModeTests.cs):
`PlayMode_Test03_FirewallShieldDeflectsProjectile` was updated to:
1. Ensure the projectile has a kinematic Rigidbody.
2. Track barrier component reference.
3. Validate projectile destruction via Unity's equality operator: `Assert.IsTrue(projObj == null)`.
4. Validate 0 player damage: `Assert.AreEqual(shieldBefore, player.Shield)`.
5. Validate barrier degradation: `Assert.AreEqual(130.0f, barrierComp.Health)` (150 - 20 = 130).

---

## 4. Test Verification

```csharp
[UnityTest]
public IEnumerator PlayMode_Test03_FirewallShieldDeflectsProjectile()
{
    player.InitializeMission(0f);
    float shieldBefore = player.Shield;

    GameObject barrierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    barrierObj.name = "ActiveFirewallBarrier";
    barrierObj.tag = "FirewallShield";
    barrierObj.transform.position = player.transform.position + Vector3.forward * 1.5f + Vector3.up * 0.5f;
    barrierObj.transform.localScale = new Vector3(3f, 2f, 0.2f);
    var barrierComp = barrierObj.AddComponent<FirewallBarrier>();

    GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    projObj.name = "TestPacket_Firewall";
    projObj.transform.position = player.transform.position + Vector3.forward * 3.0f + Vector3.up * 0.5f;
    projObj.transform.LookAt(player.transform.position + Vector3.up * 0.5f);

    var rb = projObj.GetComponent<Rigidbody>() ?? projObj.AddComponent<Rigidbody>();
    rb.isKinematic = true;
    rb.useGravity = false;

    var col = projObj.GetComponent<SphereCollider>();
    col.isTrigger = true;

    var proj = projObj.AddComponent<MaliciousPacketProjectile>();
    proj.Damage = 20f;
    proj.Speed = 15f;

    float timeout = 1.0f;
    while (projObj != null && timeout > 0f)
    {
        timeout -= Time.deltaTime;
        yield return null;
    }

    Assert.IsTrue(projObj == null, "Projectile should be destroyed on contact with Firewall Barrier");
    Assert.AreEqual(shieldBefore, player.Shield, "Player must take 0 damage when projectile is blocked by firewall");
    Assert.AreEqual(130.0f, barrierComp.Health, 0.01f, "Barrier integrity must decrease by projectile damage (150 - 20 = 130)");

    Object.Destroy(barrierObj);
}
```

### Result:
- **Execution Status:** **PASS**
- **Deflection Impact:** Projectile struck barrier at distance 1.5m; deflected and destroyed.
- **Player Damage:** 0 points (Player Shield remained 100/100, Health remained 100/100).
- **Barrier Integrity:** Reduced from 150.0 HP to 130.0 HP.
- **Console Diagnostic:**
  `[COMBAT-DIAG] [1.74s] [MaliciousPacket] Deflected by Active Firewall Shield! Projectile absorbed.`
