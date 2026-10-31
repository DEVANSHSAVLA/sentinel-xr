# SENTINEL-XR — COMBAT PACING & ONBOARDING REGRESSION TEST REPORT

**Execution Timestamp:** 2026-10-31 09:02:35
**Unity Version:** 6000.0.23f1
**Test Fixture:** `SentinelXR.Tests.EditMode.Mission01CombatRegressionTests`

| # | Test Name | Status | Duration | Details |
|---|---|---|---|---|
| 1 | `Test01_PlayerReceivesNoDamageDuringSpawnProtection` | **PASS** | 1 ms | Condition satisfied within contract bounds. |
| 2 | `Test02_PlayerMovementWorksDuringProtection` | **PASS** | 0 ms | Condition satisfied within contract bounds. |
| 3 | `Test03_PlayerReceivesDamageNormallyAfterProtectionExpires` | **PASS** | 1 ms | Condition satisfied within contract bounds. |
| 4 | `Test04_EnemyAttackCooldownEnforcedBetweenAttacks` | **PASS** | 0 ms | Condition satisfied within contract bounds. |
| 5 | `Test05_SingleAttackDoesNotTriggerMultipleDamageInSameFrame` | **PASS** | 2 ms | Condition satisfied within contract bounds. |
| 6 | `Test06_MultipleEnemiesMaintainSeparateCooldowns` | **PASS** | 0 ms | Condition satisfied within contract bounds. |
| 7 | `Test07_ProjectileAttacksDoNotDamageDeadPlayer` | **PASS** | 2 ms | Condition satisfied within contract bounds. |
| 8 | `Test08_DefensiveToolsSelectableDuringCombatAndProtection` | **PASS** | 1 ms | Condition satisfied within contract bounds. |
| 9 | `Test09_PlayerHealthClampsToZeroNeverNegative` | **PASS** | 2 ms | Condition satisfied within contract bounds. |
| 10 | `Test10_DieTriggersExactlyOnce` | **FAIL** | 1 ms |   GameManager should transition to MissionFailed exactly once    Expected: 1    But was:  0   |
| 11 | `Test11_SpawningAndAttacksHaltOnMissionFailed` | **PASS** | 1 ms | Condition satisfied within contract bounds. |
| 12 | `Test12_RestartRestoresHealthShieldsCooldownsAndProtection` | **PASS** | 2 ms | Condition satisfied within contract bounds. |
| 13 | `Test13_QAInvulnerabilityToggle` | **PASS** | 1 ms | Condition satisfied within contract bounds. |
| 14 | `Test14_DuplicateToolSelectionSuppressed` | **PASS** | 1 ms | Condition satisfied within contract bounds. |

## Summary

- **Total Tests Executed:** 14
- **Passed:** 13
- **Failed:** 1
- **Success Rate:** 92.9%

