using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SentinelXR.Gameplay;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Enemies;
using SentinelXR.Gameplay.Tools;

namespace SentinelXR.Tests.PlayMode
{
    [TestFixture]
    public class Mission01CombatPlayModeTests
    {
        private GameObject gmObj;
        private GameManager gm;
        private GameObject playerObj;
        private SentinelPlayerController player;

        [SetUp]
        public void SetUp()
        {
            if (SentinelPlayerController.Instance != null)
            {
                Object.Destroy(SentinelPlayerController.Instance.gameObject);
            }
            if (GameManager.Instance != null)
            {
                Object.Destroy(GameManager.Instance.gameObject);
            }

            gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
            gm.SetState(GameState.InMission);

            playerObj = new GameObject("Player");
            playerObj.AddComponent<CharacterController>();
            player = playerObj.AddComponent<SentinelPlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObj != null) Object.Destroy(playerObj);
            if (gmObj != null) Object.Destroy(gmObj);

            // Clean up any stray projectiles or barriers
            foreach (var proj in Object.FindObjectsByType<MaliciousPacketProjectile>(FindObjectsSortMode.None))
            {
                Object.Destroy(proj.gameObject);
            }
            foreach (var barrier in Object.FindObjectsByType<FirewallBarrier>(FindObjectsSortMode.None))
            {
                Object.Destroy(barrier.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator PlayMode_Test01_RealSpawnProtectionTimerAndExpiration()
        {
            // Configure a testable real-time duration of 0.4 seconds
            player.InitializeMission(0.4f);
            Assert.IsTrue(player.IsSpawnProtected, "Player must be spawn protected at mission start");
            Assert.Greater(player.SpawnProtectionTimeRemaining, 0.2f);

            // Wait 0.15s (still within 0.4s protection window)
            yield return new WaitForSeconds(0.15f);
            Assert.IsTrue(player.IsSpawnProtected, "Protection must remain active before timer elapses");

            // Damage should be rejected
            float shieldBefore = player.Shield;
            player.TakeDamage(20f);
            Assert.AreEqual(shieldBefore, player.Shield, "Damage must be rejected during active protection window");

            // Wait additional 0.35s (total 0.50s > 0.40s) so protection expires
            yield return new WaitForSeconds(0.35f);
            Assert.IsFalse(player.IsSpawnProtected, "Protection must expire once elapsed time exceeds duration");
            Assert.AreEqual(0f, player.SpawnProtectionTimeRemaining, "Remaining protection time must be 0");

            // Normal damage processing should now occur
            player.TakeDamage(25f);
            Assert.AreEqual(shieldBefore - 25f, player.Shield, 0.01f, "Damage must be accepted after protection expires");
        }

        /// <summary>
        /// Centralized helper to create and configure a test projectile for PlayMode physics tests.
        /// Strictly enforces component initialization order:
        /// 1. Create Primitive (Sphere).
        /// 2. Add Rigidbody explicitly if missing BEFORE MaliciousPacketProjectile is attached.
        /// 3. Configure isKinematic = true, useGravity = false.
        /// 4. Configure Collider isTrigger = true.
        /// 5. Add MaliciousPacketProjectile component (its Awake accesses Rigidbody safely).
        /// 6. Verify Rigidbody, Collider, and projectile components are not null.
        /// 7. Return the fully configured GameObject.
        /// </summary>
        private GameObject CreateTestProjectile(string name, Vector3 position, Vector3 targetLookAt, float damage = 15f, float speed = 20f)
        {
            // 1. Create the projectile GameObject.
            GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projObj.name = name;
            projObj.transform.position = position;
            projObj.transform.LookAt(targetLookAt);

            // 2. Add a Rigidbody explicitly if one does not already exist.
            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = projObj.AddComponent<Rigidbody>();
            }

            // 3. Configure Rigidbody.isKinematic = true.
            rb.isKinematic = true;

            // 4. Configure Rigidbody.useGravity = false.
            rb.useGravity = false;

            // 5. Add/configure the required Collider.
            SphereCollider col = projObj.GetComponent<SphereCollider>();
            if (col == null)
            {
                col = projObj.AddComponent<SphereCollider>();
            }

            // 6. Set its trigger behaviour according to the production collision architecture.
            col.isTrigger = true;

            // 7. Add MaliciousPacketProjectile (Rigidbody is already present for its Awake).
            MaliciousPacketProjectile proj = projObj.AddComponent<MaliciousPacketProjectile>();
            proj.Damage = damage;
            proj.Speed = speed;

            // 8. Verify that Rigidbody, Collider and projectile components are not null.
            Assert.IsNotNull(rb, $"[{name}] Rigidbody must be non-null.");
            Assert.IsTrue(rb.isKinematic, $"[{name}] Rigidbody must be kinematic.");
            Assert.IsFalse(rb.useGravity, $"[{name}] Rigidbody gravity must be false.");
            Assert.IsNotNull(col, $"[{name}] Collider must be non-null.");
            Assert.IsTrue(col.isTrigger, $"[{name}] Collider must be trigger.");
            Assert.IsNotNull(proj, $"[{name}] MaliciousPacketProjectile component must be non-null.");

            // 9. Return the fully configured test object.
            return projObj;
        }

        private GameObject CreateTestProjectile(string name)
        {
            return CreateTestProjectile(name, Vector3.zero, Vector3.forward, 15f, 20f);
        }

        [UnityTest]
        public IEnumerator PlayMode_Test02_ProjectilePhysicsCollisionAndSingleHit()
        {
            player.InitializeMission(0f); // Expire protection immediately
            float shieldBefore = player.Shield;

            // Spawn projectile 1.0m ahead aimed directly at player using helper
            Vector3 spawnPos = player.transform.position + Vector3.forward * 1.0f + Vector3.up * 0.5f;
            Vector3 targetLookAt = player.transform.position + Vector3.up * 0.5f;
            GameObject projObj = CreateTestProjectile("TestPacket_PlayMode", spawnPos, targetLookAt, 15f, 20f);

            // Wait physics frames for projectile to collide with player
            float timeout = 1.0f;
            while (projObj != null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(projObj == null, "Projectile should be destroyed after impacting player");
            Assert.AreEqual(shieldBefore - 15f, player.Shield, 0.01f, "Projectile impact should deal exactly 15 damage");

            // Wait 2 more frames to confirm single-hit and no residual damage
            yield return null;
            yield return null;
            Assert.AreEqual(shieldBefore - 15f, player.Shield, 0.01f, "No duplicate damage should be applied after impact");
        }

        [UnityTest]
        public IEnumerator PlayMode_Test03_FirewallShieldDeflectsProjectile()
        {
            player.InitializeMission(0f);
            float shieldBefore = player.Shield;

            // Spawn Firewall Barrier between player and projectile
            GameObject barrierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrierObj.name = "ActiveFirewallBarrier";
            barrierObj.tag = "FirewallShield";
            barrierObj.transform.position = player.transform.position + Vector3.forward * 1.5f + Vector3.up * 0.5f;
            barrierObj.transform.localScale = new Vector3(3f, 2f, 0.2f);
            var barrierComp = barrierObj.AddComponent<FirewallBarrier>();

            // Spawn projectile 3.0m ahead aimed through the barrier towards player using helper
            Vector3 spawnPos = player.transform.position + Vector3.forward * 3.0f + Vector3.up * 0.5f;
            Vector3 targetLookAt = player.transform.position + Vector3.up * 0.5f;
            GameObject projObj = CreateTestProjectile("TestPacket_Firewall", spawnPos, targetLookAt, 20f, 15f);

            // Wait physics frames for collision with firewall barrier
            float timeout = 1.0f;
            while (projObj != null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            // Confirm projectile was destroyed on barrier contact and player took 0 damage
            Assert.IsTrue(projObj == null, "Projectile should be destroyed on contact with Firewall Barrier");
            Assert.AreEqual(shieldBefore, player.Shield, "Player must take 0 damage when projectile is blocked by firewall");
            Assert.AreEqual(130.0f, barrierComp.Health, 0.01f, "Barrier integrity must decrease by projectile damage (150 - 20 = 130)");

            Object.Destroy(barrierObj);
        }

        [UnityTest]
        public IEnumerator PlayMode_Test04_ProjectileDoesNotDamageDeadPlayer()
        {
            player.InitializeMission(0f);
            player.TakeDamage(1000f); // Kill player
            Assert.IsTrue(player.IsDead);
            Assert.AreEqual(0f, player.Health);

            // Spawn projectile aimed at dead player using helper
            Vector3 spawnPos = player.transform.position + Vector3.forward * 1.0f + Vector3.up * 0.5f;
            Vector3 targetLookAt = player.transform.position + Vector3.up * 0.5f;
            GameObject projObj = CreateTestProjectile("TestPacket_DeadPlayer", spawnPos, targetLookAt, 20f, 20f);

            float timeout = 1.0f;
            while (projObj != null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(0f, player.Health, "Dead player must remain at 0 health");
            Assert.IsTrue(player.IsDead, "Dead player must remain dead");
            Assert.IsTrue(projObj == null, "Projectile should be destroyed after passing dead player");
        }
    }
}
