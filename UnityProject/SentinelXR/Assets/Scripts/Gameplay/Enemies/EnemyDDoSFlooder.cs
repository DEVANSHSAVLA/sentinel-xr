using UnityEngine;
using SentinelXR.Gameplay.Player;

namespace SentinelXR.Gameplay.Enemies
{
    public class EnemyDDoSFlooder : EnemyBase
    {
        [Header("DDoS Artillery Settings")]
        [SerializeField] private float optimalRange = 12.0f;
        [SerializeField] private float fireRate = 2.2f;
        [SerializeField] private GameObject packetProjectilePrefab;

        private Transform playerTarget;
        private Renderer flooderRenderer;
        private Color defaultColor = Color.red;

        public float OptimalRange => optimalRange;
        public float FireRate { get => fireRate; set { fireRate = value; attackCooldown = value; } }

        protected override void Start()
        {
            ThreatName = "DDoS Volumetric Flooder";
            maxHealth = 65f;
            moveSpeed = 2.2f;
            attackDamage = 8f;
            attackCooldown = fireRate;
            attackTelegraphDuration = 0.5f;

            flooderRenderer = GetComponent<Renderer>();
            if (flooderRenderer != null && flooderRenderer.material != null)
            {
                defaultColor = flooderRenderer.material.color;
            }

            base.Start();

            if (SentinelPlayerController.Instance != null)
            {
                playerTarget = SentinelPlayerController.Instance.transform;
            }
        }

        protected override void PerformBehavior()
        {
            if (playerTarget == null)
            {
                if (SentinelPlayerController.Instance != null)
                {
                    playerTarget = SentinelPlayerController.Instance.transform;
                }
                return;
            }

            // Stop targeting a dead player
            if (SentinelPlayerController.Instance != null && SentinelPlayerController.Instance.IsDead)
            {
                CancelTelegraph();
                return;
            }

            // Face player
            Vector3 aimDir = (playerTarget.position + Vector3.up * 0.8f) - transform.position;
            if (aimDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aimDir), 5f * Time.deltaTime);
            }

            float distance = Vector3.Distance(transform.position, playerTarget.position);

            // Handle Telegraph Windup state
            if (isTelegraphing)
            {
                telegraphTimer -= Time.deltaTime;

                // Visual telegraph: pulsate warning color
                if (flooderRenderer != null && flooderRenderer.material != null)
                {
                    float pulse = Mathf.PingPong(Time.time * 10f, 1f);
                    flooderRenderer.material.color = Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), pulse);
                }

                if (telegraphTimer <= 0f)
                {
                    isTelegraphing = false;
                    ResetColor();

                    if (SentinelPlayerController.Instance != null && !SentinelPlayerController.Instance.IsDead)
                    {
                        if (SentinelPlayerController.Instance.IsSpawnProtected)
                        {
                            lastAttackTime = Time.time;
                            LogCombatDiag("Fire Suppressed", "Player under spawn protection; firing cancelled.");
                        }
                        else
                        {
                            FirePacketProjectile();
                        }
                    }
                }
                return;
            }

            // Maintain distance
            if (distance > optimalRange + 2f)
            {
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
            else if (distance < optimalRange - 2f)
            {
                transform.position -= transform.forward * moveSpeed * Time.deltaTime;
            }

            // Shoot packet burst with cooldown enforcement
            if (CanAttack)
            {
                if (SentinelPlayerController.Instance != null && SentinelPlayerController.Instance.IsSpawnProtected)
                {
                    // Player protected: hold fire
                    return;
                }

                isTelegraphing = true;
                telegraphTimer = attackTelegraphDuration;
                LogCombatDiag("Attack Telegraph", $"Artillery charging SYN flood ({attackTelegraphDuration:F2}s) targeting player.");
            }
        }

        public void FirePacketProjectile()
        {
            lastAttackTime = Time.time;

            Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.3f;
            GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            proj.name = "MaliciousSYN_Packet";
            proj.transform.position = spawnPos;
            proj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            var col = proj.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true;

            var rb = proj.GetComponent<Rigidbody>();
            if (rb == null) rb = proj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var rend = proj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.red;
                rend.material = mat;
            }

            var packetScript = proj.AddComponent<MaliciousPacketProjectile>();
            packetScript.Damage = attackDamage;
            proj.transform.rotation = transform.rotation;

            LogCombatDiag("Projectile Fired", $"SYN packet dispatched toward player. Cooldown: {attackCooldown:F1}s.");
        }

        private void CancelTelegraph()
        {
            if (isTelegraphing)
            {
                isTelegraphing = false;
                ResetColor();
            }
        }

        private void ResetColor()
        {
            if (flooderRenderer != null && flooderRenderer.material != null)
            {
                flooderRenderer.material.color = defaultColor;
            }
        }

        public override void ApplyStun(float duration)
        {
            CancelTelegraph();
            base.ApplyStun(duration);
        }

        protected override void OnStunRecovered()
        {
            ResetColor();
            base.OnStunRecovered();
        }
    }
}
