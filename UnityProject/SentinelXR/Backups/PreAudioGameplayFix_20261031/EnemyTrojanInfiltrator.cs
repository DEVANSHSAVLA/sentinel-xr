using UnityEngine;
using SentinelXR.Gameplay.Environment;

namespace SentinelXR.Gameplay.Enemies
{
    public class EnemyTrojanInfiltrator : EnemyBase
    {
        [Header("Stealth Parameters")]
        [SerializeField] private bool isCloaked = true;
        [SerializeField] private float corruptionDrainRate = 8.0f; // Tuned for onboarding pacing

        private CyberNetworkNode targetNode;
        private Renderer meshRenderer;
        private float revealTimer = 0f;
        private float lastCorruptionDiagTime = 0f;

        public bool IsCloaked => isCloaked;
        public float CorruptionDrainRate { get => corruptionDrainRate; set => corruptionDrainRate = value; }

        protected override void Start()
        {
            ThreatName = "Trojan Infiltrator";
            maxHealth = 45f;
            moveSpeed = 2.8f;
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
                Vector3 dir = (targetNode.transform.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
                }
            }
            else
            {
                // Infiltrate and inject corruption into cyber node
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
                meshRenderer.material.color = cloaked ? new Color(0.8f, 0.2f, 0.8f, 0.25f) : new Color(0.9f, 0.1f, 0.9f, 1.0f);
            }
        }

        public override void ApplyStun(float duration)
        {
            // Stun decloaks and pauses corruption
            SetCloak(false);
            base.ApplyStun(duration);
        }
    }
}
