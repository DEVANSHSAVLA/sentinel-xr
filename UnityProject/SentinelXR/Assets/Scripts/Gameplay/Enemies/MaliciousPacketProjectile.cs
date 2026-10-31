using UnityEngine;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Environment;
using SentinelXR.Gameplay.Tools;

namespace SentinelXR.Gameplay.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public class MaliciousPacketProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 14.0f;
        [SerializeField] private float damage = 8.0f;
        [SerializeField] private float lifetime = 5.0f;

        private bool hasHit = false;
        private Rigidbody rb;

        public float Damage
        {
            get => damage;
            set => damage = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        public bool HasHit => hasHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Destroy active projectiles if the mission is aborted or ended
            if (GameManager.Instance != null && !GameManager.Instance.IsMissionActive)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Single-hit guard: cannot damage multiple entities or re-trigger
            if (hasHit) return;

            // Blocked by Firewall Shield (component check, parent check, tag check, or name check)
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
                        // Broken barrier does not deflect
                        return;
                    }
                    barrier.TakeDamage(damage);
                }

                hasHit = true;
                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [MaliciousPacket] Deflected by Active Firewall Shield! Projectile absorbed.");
                Destroy(gameObject);
                return;
            }

            // Hit Player
            SentinelPlayerController player = other.GetComponentInParent<SentinelPlayerController>();
            if (player != null)
            {
                hasHit = true;

                if (player.IsDead)
                {
                    Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [MaliciousPacket] Passed harmlessly through dead player.");
                    Destroy(gameObject);
                    return;
                }

                if (player.IsSpawnProtected)
                {
                    Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [MaliciousPacket] Deflected by Player Spawn Protection ({player.SpawnProtectionTimeRemaining:F2}s remaining).");
                    Destroy(gameObject);
                    return;
                }

                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [MaliciousPacket] Impacted player for {damage:F0} damage.");
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Hit Cyber Node
            CyberNetworkNode node = other.GetComponentInParent<CyberNetworkNode>();
            if (node != null)
            {
                hasHit = true;
                node.TakeCorruptionDamage(damage * 0.5f);
                Destroy(gameObject);
                return;
            }

            // Hit obstacles
            if (!other.isTrigger && !other.GetComponentInParent<EnemyBase>())
            {
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }
}
