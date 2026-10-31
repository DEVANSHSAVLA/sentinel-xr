using UnityEngine;
using SentinelXR.Gameplay.Environment;
using SentinelXR.Gameplay.Missions;

namespace SentinelXR.Gameplay.Enemies
{
    public class EnemyTrojanInfiltrator : EnemyBase
    {
        [Header("Stealth Parameters")]
        [SerializeField] private bool isCloaked = true;
        [SerializeField] private float corruptionDrainRate = 3.5f; // Paced for onboarding reaction window
        [SerializeField] private float arrivalTelegraphDuration = 3.5f; // Grace period before active corruption starts
        [SerializeField] private bool delayAttackUntilObjectiveActive = true;

        private CyberNetworkNode targetNode;
        private Renderer meshRenderer;
        private float revealTimer = 0f;
        private float lastCorruptionDiagTime = 0f;
        private float arrivalTimeAtNode = -1f;
        private bool hasTelegraphed = false;

        public bool IsCloaked => isCloaked;
        public float CorruptionDrainRate { get => corruptionDrainRate; set => corruptionDrainRate = value; }
        public float ArrivalTelegraphDuration { get => arrivalTelegraphDuration; set => arrivalTelegraphDuration = value; }
        public new bool IsTelegraphing => isTelegraphing || (arrivalTimeAtNode > 0 && (Time.time - arrivalTimeAtNode < arrivalTelegraphDuration));
        public bool IsCorrupting => arrivalTimeAtNode > 0 && (Time.time - arrivalTimeAtNode >= arrivalTelegraphDuration);

        protected override void Start()
        {
            ThreatName = "Trojan Infiltrator";
            maxHealth = 45f;
            moveSpeed = 2.4f; // Balanced movement speed
            attackDamage = 12f;
            attackCooldown = 3.0f;
            attackTelegraphDuration = 0.5f;
            base.Start();

            meshRenderer = GetComponent<Renderer>();
            SetCloak(true);
            FindNearestNode();
        }

        protected override void PerformBehavior()
        {
            if (revealTimer > 0)
            {
                revealTimer -= Time.deltaTime;
                if (revealTimer <= 0)
                {
                    SetCloak(true);
                }
            }

            if (targetNode == null || targetNode.IsCompromised)
            {
                FindNearestNode();
                if (targetNode == null) return;
            }

            float distance = Vector3.Distance(transform.position, targetNode.transform.position);
            if (distance > 2.0f)
            {
                arrivalTimeAtNode = -1f;
                hasTelegraphed = false;

                Vector3 dir = (targetNode.transform.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
                }
            }
            else
            {
                // Infiltrator has reached target node
                if (delayAttackUntilObjectiveActive && MissionObjectiveManager.Instance != null)
                {
                    // If Objective 01 (Scan) is not yet complete, hold until player has scanned or damaged this unit
                    var objs = MissionObjectiveManager.Instance.Objectives;
                    if (objs != null && objs.Count > 0 && !objs[0].isCompleted && currentHealth >= maxHealth)
                    {
                        // In holding / priming pattern
                        return;
                    }
                }

                if (arrivalTimeAtNode < 0f)
                {
                    arrivalTimeAtNode = Time.time;
                }

                // 1. Telegraph & Warning Phase
                if (Time.time - arrivalTimeAtNode < arrivalTelegraphDuration)
                {
                    if (!hasTelegraphed)
                    {
                        hasTelegraphed = true;
                        RevealFromStealth(arrivalTelegraphDuration + 2.0f);
                        SentinelAudioSystem.Instance?.PlayGatewayAlert();
                        MissionObjectiveManager.Instance?.NotifyGatewayTargeted(targetNode);
                        LogCombatDiag("Exploit Priming", $"Hostile infiltrator arrived at {targetNode.NodeName}. Priming exploit payload ({arrivalTelegraphDuration:F1}s telegraph).");
                    }
                    return;
                }

                // 2. Active Corruption Injection Phase (paced drain)
                targetNode.TakeCorruptionDamage(corruptionDrainRate * Time.deltaTime);

                if (Time.time - lastCorruptionDiagTime >= 2.0f)
                {
                    lastCorruptionDiagTime = Time.time;
                    LogCombatDiag("Corrupting Node", $"Injecting {corruptionDrainRate:F1} corruption/s into {targetNode.name} (Integrity: {targetNode.CurrentIntegrity:F0}%).");
                }
            }
        }

        private void FindNearestNode()
        {
            CyberNetworkNode[] nodes = FindObjectsOfType<CyberNetworkNode>();
            float bestDist = Mathf.Infinity;
            targetNode = null;
            foreach (var n in nodes)
            {
                if (!n.IsCompromised)
                {
                    float d = Vector3.Distance(transform.position, n.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        targetNode = n;
                    }
                }
            }
        }

        public void RevealFromStealth(float duration)
        {
            SetCloak(false);
            revealTimer = duration;
            LogCombatDiag("Revealed", $"DECLOAKED by Trace Scanner for {duration:F1}s!");
        }

        private void SetCloak(bool cloaked)
        {
            isCloaked = cloaked;
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = cloaked ? new Color(0.8f, 0.2f, 0.8f, 0.35f) : new Color(0.9f, 0.1f, 0.9f, 1.0f);
            }
        }

        public override void ApplyStun(float duration)
        {
            // Stun decloaks and pauses corruption
            SetCloak(false);
            arrivalTimeAtNode = Time.time; // Resets telegraph window after stun recovery
            base.ApplyStun(duration);
        }
    }
}
