using System;
using UnityEngine;
using SentinelXR.Gameplay.Player;

namespace SentinelXR.Gameplay.Enemies
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        public string ThreatName = "Malware Entity";
        [SerializeField] protected float maxHealth = 50.0f;
        [SerializeField] protected float currentHealth = 50.0f;
        [SerializeField] protected float moveSpeed = 3.5f;
        [SerializeField] protected float attackDamage = 15.0f;
        [SerializeField] protected int scoreReward = 100;

        [Header("Combat & Cooldowns")]
        [SerializeField] protected float attackCooldown = 2.0f;
        [SerializeField] protected float attackTelegraphDuration = 0.4f;
        protected float lastAttackTime = -999f;
        protected bool isTelegraphing = false;
        protected float telegraphTimer = 0f;

        protected bool isStunned = false;
        protected float stunTimer = 0f;

        public float Health => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsStunned => isStunned;
        public float AttackCooldown
        {
            get => attackCooldown;
            set => attackCooldown = value;
        }
        public float LastAttackTime
        {
            get => lastAttackTime;
            set => lastAttackTime = value;
        }
        public bool IsTelegraphing => isTelegraphing;
        public float AttackTelegraphDuration
        {
            get => attackTelegraphDuration;
            set => attackTelegraphDuration = value;
        }
        public bool CanAttack => (Time.time - lastAttackTime) >= attackCooldown && !isTelegraphing && !isStunned;

        public event Action<EnemyBase> OnEnemyDefeated;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            lastAttackTime = -999f;
            isTelegraphing = false;
        }

        protected virtual void Update()
        {
            // Stop all enemy behavior when mission is no longer active
            if (GameManager.Instance != null && !GameManager.Instance.IsMissionActive)
            {
                return;
            }

            // Stop enemy behavior if the player is dead
            if (SentinelPlayerController.Instance != null && SentinelPlayerController.Instance.IsDead)
            {
                return;
            }

            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    isStunned = false;
                    OnStunRecovered();
                }
                return;
            }

            PerformBehavior();
        }

        protected abstract void PerformBehavior();

        protected void LogCombatDiag(string eventName, string details)
        {
            Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [{ThreatName}#{GetInstanceID()}] {eventName}: {details}");
        }

        public virtual void TakeDamage(float amount)
        {
            currentHealth -= amount;
            Debug.Log($"[Enemy] {ThreatName} took {amount:F0} damage. Health: {currentHealth:F0}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void ApplyStun(float duration)
        {
            isStunned = true;
            stunTimer = duration;
            isTelegraphing = false;
            Debug.Log($"[Enemy] {ThreatName} QUARANTINED! Stunned for {duration} seconds.");
        }

        protected virtual void OnStunRecovered()
        {
            Debug.Log($"[Enemy] {ThreatName} recovered from quarantine stun.");
        }

        protected virtual void Die()
        {
            Debug.Log($"[Enemy] {ThreatName} PURGED and DE-REZZED!");
            SentinelAudioSystem.Instance?.PlayEnemyPurged();
            OnEnemyDefeated?.Invoke(this);

            if (Missions.MissionObjectiveManager.Instance != null)
            {
                Missions.MissionObjectiveManager.Instance.RecordThreatEliminated(this);
            }

            Destroy(gameObject);
        }
    }
}
