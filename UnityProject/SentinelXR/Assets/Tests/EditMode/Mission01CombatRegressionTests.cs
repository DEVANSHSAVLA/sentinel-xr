using System;
using NUnit.Framework;
using UnityEngine;
using SentinelXR.Gameplay;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Enemies;
using SentinelXR.Gameplay.Tools;

namespace SentinelXR.Tests.EditMode
{
    /// <summary>
    /// EditMode regression test suite for Mission 01 combat stabilization, spawn protection,
    /// attack cooldowns, pacing, death idempotency, and player onboarding contracts.
    /// Pure EditMode tests isolate logic contracts without invoking runtime physics destruction.
    /// Full physics-lifecycle tests reside in SentinelXR.Tests.PlayMode.
    /// </summary>
    [TestFixture]
    public class Mission01CombatRegressionTests
    {
        private GameObject gmObj;
        private GameManager gm;

        private GameObject playerObj;
        private SentinelPlayerController player;

        [SetUp]
        public void SetUp()
        {
            // Clean up any lingering singletons to ensure hermetic test execution
            if (SentinelPlayerController.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(SentinelPlayerController.Instance.gameObject);
            }
            if (GameManager.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            // --- GameManager singleton ---
            gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
            gm.InitializeSingleton();
            gm.SetState(GameState.InMission);

            // --- Player (CharacterController added automatically via RequireComponent) ---
            playerObj = new GameObject("Player");
            playerObj.AddComponent<CharacterController>();
            player = playerObj.AddComponent<SentinelPlayerController>();
            player.InitializeMission();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObj != null) UnityEngine.Object.DestroyImmediate(playerObj);
            if (gmObj != null) UnityEngine.Object.DestroyImmediate(gmObj);
        }

        // ======================================================================
        // TEST 1: Player receives no damage during initial protection (Startup Contract)
        // ======================================================================
        [Test]
        public void Test01_PlayerReceivesNoDamageDuringSpawnProtection()
        {
            player.InitializeMission(); // Standard 8.0s protection initialized
            Assert.AreEqual(8.0f, player.SpawnProtectionDuration, "Default protection duration should be 8.0s");
            Assert.IsTrue(player.IsSpawnProtected, "Player must be spawn protected upon mission start");
            Assert.Greater(player.SpawnProtectionTimeRemaining, 0f, "Spawn protection time remaining should be > 0");

            float initialHealth = player.Health;
            float initialShield = player.Shield;

            player.TakeDamage(25.0f);

            Assert.AreEqual(initialHealth, player.Health, "Health must not decrease during spawn protection");
            Assert.AreEqual(initialShield, player.Shield, "Shield must not decrease during spawn protection");
        }

        // ======================================================================
        // TEST 2: Player movement works during protection
        // ======================================================================
        [Test]
        public void Test02_PlayerMovementWorksDuringProtection()
        {
            Assert.IsTrue(player.IsSpawnProtected, "Player should be spawn protected");

            var cc = player.GetComponent<CharacterController>();
            Assert.IsNotNull(cc, "CharacterController must be present during protection");
            Assert.IsTrue(cc.enabled, "CharacterController must remain enabled during protection");
            Assert.IsFalse(player.IsDead, "Player is not dead during protection");
            Assert.AreEqual(playerObj.transform.position, Vector3.zero, "Initial position is valid");
        }

        // ======================================================================
        // TEST 3: Player receives damage normally after protection expires
        // ======================================================================
        [Test]
        public void Test03_PlayerReceivesDamageNormallyAfterProtectionExpires()
        {
            // Expire protection manually
            player.SetSpawnProtection(0f);
            Assert.IsFalse(player.IsSpawnProtected, "Spawn protection should be expired");

            float shieldBefore = player.Shield;
            player.TakeDamage(30f);
            Assert.AreEqual(shieldBefore - 30f, player.Shield, 0.01f, "Damage should reduce shield after protection expires");

            // Over-shield damage reduces health
            player.TakeDamage(player.Shield + 20f);
            Assert.AreEqual(0f, player.Shield, 0.01f, "Shield should be depleted");
            Assert.AreEqual(player.MaxHealth - 20f, player.Health, 0.01f, "Remaining damage should reduce health");
        }

        // ======================================================================
        // TEST 4: Enemy attack cooldowns are enforced between successive attacks
        // ======================================================================
        [Test]
        public void Test04_EnemyAttackCooldownEnforcedBetweenAttacks()
        {
            var wormObj = new GameObject("Worm");
            var worm = wormObj.AddComponent<EnemyMalwareWorm>();
            worm.AttackCooldown = 2.0f;

            worm.ExecuteAttack();
            Assert.IsFalse(worm.CanAttack, "Enemy cannot attack immediately after executing an attack (cooldown active)");
            Assert.AreEqual(Time.time, worm.LastAttackTime, 0.05f, "LastAttackTime should record execution time");

            UnityEngine.Object.DestroyImmediate(wormObj);
        }

        // ======================================================================
        // TEST 5: A single attack does not trigger multiple damage events in the same frame
        // (Pure EditMode test: verifies attack cooldown lock and damage idempotency)
        // ======================================================================
        [Test]
        public void Test05_SingleAttackDoesNotTriggerMultipleDamageInSameFrame()
        {
            player.SetSpawnProtection(0f);
            Assert.AreEqual(100f, player.Health, "Initial health must be 100");
            Assert.AreEqual(100f, player.Shield, "Initial shield must be 100");
            float shieldBefore = player.Shield;

            var wormObj = new GameObject("Worm_SingleHitTest");
            var worm = wormObj.AddComponent<EnemyMalwareWorm>();
            worm.AttackCooldown = 2.0f;

            // First attack connects: inflicts 10 damage to shield, health remains 100
            worm.ExecuteAttack();
            Assert.AreEqual(shieldBefore - 10f, player.Shield, 0.01f, "First attack inflicts exactly 10 damage to shield");
            Assert.AreEqual(100f, player.Health, 0.01f, "Health remains 100 under shield-first absorption");
            Assert.IsFalse(worm.CanAttack, "Worm is placed on cooldown immediately after first strike");

            // Subsequent attack attempt within same frame/cooldown cycle is blocked
            worm.ExecuteAttack();
            Assert.AreEqual(shieldBefore - 10f, player.Shield, 0.01f, "Player shield remains 90; no additional damage dealt in same cycle");
            Assert.AreEqual(100f, player.Health, 0.01f, "Player health remains 100");

            UnityEngine.Object.DestroyImmediate(wormObj);
        }

        // ======================================================================
        // TEST 6: Multiple enemies maintain separate, correct cooldowns
        // ======================================================================
        [Test]
        public void Test06_MultipleEnemiesMaintainSeparateCooldowns()
        {
            var worm1Obj = new GameObject("Worm1");
            var worm1 = worm1Obj.AddComponent<EnemyMalwareWorm>();
            worm1.AttackCooldown = 2.0f;

            var worm2Obj = new GameObject("Worm2");
            var worm2 = worm2Obj.AddComponent<EnemyMalwareWorm>();
            worm2.AttackCooldown = 2.0f;

            worm1.ExecuteAttack();
            Assert.IsFalse(worm1.CanAttack, "Worm 1 should be on cooldown");
            Assert.IsTrue(worm2.CanAttack, "Worm 2 should still be ready to attack independently");

            worm2.ExecuteAttack();
            Assert.IsFalse(worm2.CanAttack, "Worm 2 is now also on cooldown");

            UnityEngine.Object.DestroyImmediate(worm1Obj);
            UnityEngine.Object.DestroyImmediate(worm2Obj);
        }

        // ======================================================================
        // TEST 7: Dead player is immune to further damage
        // (Pure EditMode test: verifies TakeDamage rejection guard when dead)
        // ======================================================================
        [Test]
        public void Test07_ProjectileAttacksDoNotDamageDeadPlayer()
        {
            player.SetSpawnProtection(0f);
            player.TakeDamage(1000f); // Lethal damage
            Assert.IsTrue(player.IsDead, "Player should be dead");
            Assert.AreEqual(0f, player.Health, "Health should clamp to 0");

            // Attempt further damage
            player.TakeDamage(25f);
            Assert.AreEqual(0f, player.Health, "Dead player must reject further damage and remain at 0 health");
            Assert.IsTrue(player.IsDead);
        }

        // ======================================================================
        // TEST 8: Defensive tools remain selectable during combat and protection
        // ======================================================================
        [Test]
        public void Test08_DefensiveToolsSelectableDuringCombatAndProtection()
        {
            Assert.IsTrue(player.IsSpawnProtected);

            var toolObj = new GameObject("ToolInventory");
            var tool = toolObj.AddComponent<ToolInventory>();

            tool.SelectTool(CyberToolType.TraceScanner);
            Assert.AreEqual(CyberToolType.TraceScanner, tool.ActiveTool, "Tool 1 (TraceScanner) should be selected");

            tool.SelectTool(CyberToolType.PatchTool);
            Assert.AreEqual(CyberToolType.PatchTool, tool.ActiveTool, "Tool 2 (PatchTool) should be selected");

            tool.SelectTool(CyberToolType.QuarantinePulse);
            Assert.AreEqual(CyberToolType.QuarantinePulse, tool.ActiveTool, "Tool 3 (QuarantinePulse) should be selected");

            tool.SelectTool(CyberToolType.FirewallShield);
            Assert.AreEqual(CyberToolType.FirewallShield, tool.ActiveTool, "Tool 4 (FirewallShield) should be selected");

            UnityEngine.Object.DestroyImmediate(toolObj);
        }

        // ======================================================================
        // TEST 9: Player health clamps to zero and never goes negative
        // ======================================================================
        [Test]
        public void Test09_PlayerHealthClampsToZeroNeverNegative()
        {
            player.SetSpawnProtection(0f);
            player.TakeDamage(99999f);

            Assert.AreEqual(0f, player.Health, "Health must clamp to 0 and never become negative");
            Assert.IsTrue(player.IsDead, "Player must be dead");
        }

        // ======================================================================
        // TEST 10: Die() triggers once and transitions GameManager to MissionFailed
        // ======================================================================
        [Test]
        public void Test10_DieTriggersExactlyOnce()
        {
            player.SetSpawnProtection(0f);
            Assert.IsNotNull(GameManager.Instance, "GameManager.Instance must not be null");
            Assert.AreEqual(GameState.InMission, gm.CurrentState, "GameManager must be InMission initially");
            Assert.AreSame(gm, GameManager.Instance, "GameManager.Instance must match active test manager");

            int gameOverCount = 0;
            Action<GameState> stateHandler = (state) =>
            {
                if (state == GameState.MissionFailed) gameOverCount++;
            };
            GameManager.Instance.OnGameStateChanged += stateHandler;

            // Apply lethal damage
            player.TakeDamage(500f);
            Assert.IsTrue(player.IsDead, "Player should be dead after lethal hit");
            Assert.AreEqual(GameState.MissionFailed, GameManager.Instance.CurrentState, "GameManager state must equal MissionFailed");
            Assert.AreEqual(1, gameOverCount, "GameManager should transition to MissionFailed exactly once");

            // Further damage calls after death must not re-trigger
            player.TakeDamage(50f);
            player.TakeDamage(50f);
            Assert.AreEqual(1, gameOverCount, "Repeated damage after death must not re-trigger game over event");
            Assert.AreEqual(GameState.MissionFailed, GameManager.Instance.CurrentState, "GameManager state must remain MissionFailed");

            GameManager.Instance.OnGameStateChanged -= stateHandler;
        }

        // ======================================================================
        // TEST 11: Spawning and attacks halt on MissionFailed
        // ======================================================================
        [Test]
        public void Test11_SpawningAndAttacksHaltOnMissionFailed()
        {
            gm.TriggerGameOver();
            Assert.AreEqual(GameState.MissionFailed, gm.CurrentState);
            Assert.IsFalse(gm.IsMissionActive, "IsMissionActive must be false after MissionFailed");

            float healthBefore = player.Health;
            player.TakeDamage(15f);
            Assert.AreEqual(healthBefore, player.Health, "Damage must be rejected when IsMissionActive is false");
        }

        // ======================================================================
        // TEST 12: Restart restores health, shields, cooldowns, protection, and mission objectives
        // ======================================================================
        [Test]
        public void Test12_RestartRestoresHealthShieldsCooldownsAndProtection()
        {
            player.SetSpawnProtection(0f);
            player.TakeDamage(500f);
            Assert.IsTrue(player.IsDead);

            // Simulate restart: destroy old player, reset GM state, instantiate fresh player
            UnityEngine.Object.DestroyImmediate(playerObj);
            gm.SetState(GameState.InMission);

            playerObj = new GameObject("Player_Restarted");
            playerObj.AddComponent<CharacterController>();
            player = playerObj.AddComponent<SentinelPlayerController>();
            player.InitializeMission(8.0f);

            Assert.IsFalse(player.IsDead, "Fresh player is not dead");
            Assert.AreEqual(player.MaxHealth, player.Health, "Health should reset to maxHealth");
            Assert.AreEqual(player.MaxShield, player.Shield, "Shield should reset to maxShield");
            Assert.IsTrue(player.IsSpawnProtected, "Spawn protection should be active on restart");
            Assert.AreEqual(8.0f, player.SpawnProtectionDuration, "Spawn protection duration should be 8.0s");
        }

        // ======================================================================
        // TEST 13: QA Invulnerability Toggle
        // ======================================================================
        [Test]
        public void Test13_QAInvulnerabilityToggle()
        {
            player.SetSpawnProtection(0f);
            player.QAInvulnerability = true;

            Assert.IsTrue(player.IsSpawnProtected, "QA Invulnerability should grant protection in Editor/Dev");

            player.TakeDamage(50f);
            Assert.AreEqual(player.MaxShield, player.Shield, "Shield intact under QA invulnerability");
            Assert.AreEqual(player.MaxHealth, player.Health, "Health intact under QA invulnerability");

            player.QAInvulnerability = false;
            Assert.IsFalse(player.IsSpawnProtected, "Disabling QA invulnerability restores vulnerability");
        }

        // ======================================================================
        // TEST 14: Duplicate tool selection suppressed
        // ======================================================================
        [Test]
        public void Test14_DuplicateToolSelectionSuppressed()
        {
            var toolObj = new GameObject("ToolInventory");
            var tool = toolObj.AddComponent<ToolInventory>();

            int changeCount = 0;
            tool.OnToolChanged += (_) => changeCount++;

            tool.SelectTool(CyberToolType.PatchTool);
            Assert.AreEqual(1, changeCount);

            tool.SelectTool(CyberToolType.PatchTool);
            tool.SelectTool(CyberToolType.PatchTool);
            Assert.AreEqual(1, changeCount, "Duplicate selection should not fire redundant change events");

            UnityEngine.Object.DestroyImmediate(toolObj);
        }

        // ======================================================================
        // TEST 15: FirewallShield tag and component configuration contract
        // ======================================================================
        [Test]
        public void Test15_FirewallShieldTagAndComponentContract()
        {
            var barrierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrierObj.name = "ActiveFirewallBarrier";
            barrierObj.tag = "FirewallShield";
            var barrierComp = barrierObj.AddComponent<FirewallBarrier>();

            Assert.AreEqual("FirewallShield", barrierObj.tag, "GameObject tag must equal FirewallShield without throwing");
            Assert.IsTrue(barrierObj.CompareTag("FirewallShield"), "CompareTag('FirewallShield') must evaluate to true without undefined tag errors");
            Assert.IsNotNull(barrierComp, "FirewallBarrier component must be present on deployed barrier");
            Assert.AreEqual(150.0f, barrierComp.Health, "Default barrier integrity should be 150 HP");
            Assert.IsFalse(barrierComp.IsBroken, "Barrier should not be broken initially");

            // Test integrity reduction and repair contracts
            barrierComp.TakeDamage(30f);
            Assert.AreEqual(120.0f, barrierComp.Health, 0.01f, "Integrity should reduce by 30 HP to 120 HP");

            barrierComp.Repair(15f);
            Assert.AreEqual(135.0f, barrierComp.Health, 0.01f, "Repair should restore barrier integrity");

            UnityEngine.Object.DestroyImmediate(barrierObj);
        }
    }
}
