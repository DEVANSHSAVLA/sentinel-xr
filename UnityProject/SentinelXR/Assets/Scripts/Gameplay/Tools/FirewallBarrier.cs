using UnityEngine;

namespace SentinelXR.Gameplay.Tools
{
    /// <summary>
    /// Component attached to deployed Firewall Barriers to provide
    /// type-safe, component-based collision identification without
    /// relying solely on tag string matching.
    /// Default integrity: 150 HP.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FirewallBarrier : MonoBehaviour
    {
        [SerializeField] private float barrierHealth = 150.0f;
        [SerializeField] private float currentHealth = 150.0f;

        public float Health => currentHealth;
        public float MaxHealth => barrierHealth;
        public bool IsBroken => currentHealth <= 0f;

        private void Reset()
        {
            barrierHealth = 150.0f;
            currentHealth = 150.0f;
        }

        private void Awake()
        {
            if (currentHealth <= 0f)
            {
                currentHealth = barrierHealth;
            }
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsBroken) return;

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, barrierHealth);

            Debug.Log($"[FirewallBarrier] Barrier integrity reduced by {amount:F0}. Remaining: {currentHealth:F0}/{barrierHealth:F0}");

            if (currentHealth <= 0f)
            {
                Debug.Log("[FirewallBarrier] Barrier integrity depleted by threat barrage. Barrier collapsed.");
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        public void Repair(float amount = 150.0f)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, barrierHealth);
            Debug.Log($"[FirewallBarrier] Barrier repaired by {amount:F0}. Integrity: {currentHealth:F0}/{barrierHealth:F0}");
        }

        public void ResetBarrier()
        {
            currentHealth = barrierHealth;
        }
    }
}
