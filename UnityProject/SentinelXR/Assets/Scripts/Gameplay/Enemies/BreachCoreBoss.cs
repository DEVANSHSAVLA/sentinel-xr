using UnityEngine;

namespace SentinelXR.Gameplay.Enemies
{
    public class BreachCoreBoss : EnemyBase
    {
        [Header("Breach Core Parameters")]
        [SerializeField] private bool isShielded = true;
        [SerializeField] private float pulseAttackInterval = 3.5f;

        private float lastPulseTime = 0f;
        private Renderer coreRenderer;

        public bool IsShielded => isShielded;

        protected override void Start()
        {
            ThreatName = "BREACH CORE ALPHA";
            maxHealth = 200f;
            moveSpeed = 0f; // Immobile anchor
            attackDamage = 25f;
            base.Start();

            coreRenderer = GetComponent<Renderer>();
            UpdateShieldVisual();
        }

        protected override void PerformBehavior()
        {
            // Spin slowly
            transform.Rotate(Vector3.up * 20f * Time.deltaTime);

            if (Time.time - lastPulseTime >= pulseAttackInterval)
            {
                lastPulseTime = Time.time;
                EmitRadialDischarge();
            }
        }

        public void SetShieldActive(bool active)
        {
            isShielded = active;
            UpdateShieldVisual();
            Debug.Log($"[BreachCoreBoss] Core vulnerability state: Shielded={isShielded}");
        }

        public override void TakeDamage(float amount)
        {
            if (isShielded)
            {
                Debug.LogWarning("[BreachCoreBoss] Attack absorbed by Subnet Anomaly Barrier! Patch gateway nodes first.");
                return;
            }
            base.TakeDamage(amount);
        }

        private void EmitRadialDischarge()
        {
            // Fire 6 projectiles outwards
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                proj.name = "CorePulsePacket";
                proj.transform.position = transform.position + dir * 1.5f;
                proj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

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
                    mat.color = new Color(1f, 0.1f, 0.3f);
                    rend.material = mat;
                }

                proj.AddComponent<MaliciousPacketProjectile>();
                proj.transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void UpdateShieldVisual()
        {
            if (coreRenderer != null)
            {
                coreRenderer.material.color = isShielded ? new Color(1f, 0.5f, 0f, 0.9f) : new Color(0.9f, 0.1f, 0.1f, 1f);
            }
        }

        protected override void Die()
        {
            Debug.Log("[BreachCoreBoss] *** BREACH CORE ALPHA NEUTRALIZED! SUBNET THREAT RESOLVED! ***");
            base.Die();
        }
    }
}
