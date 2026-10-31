using UnityEngine;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Enemies;

namespace SentinelXR.Gameplay.Tools
{
    public class QuarantinePulse : MonoBehaviour
    {
        [Header("Pulse Settings")]
        [SerializeField] private float pulseRadius = 14.0f;
        [SerializeField] private float stunDuration = 4.0f;
        [SerializeField] private float pulseDamage = 35.0f;
        [SerializeField] private float knockbackForce = 12.0f;
        [SerializeField] private float energyCost = 30.0f;
        [SerializeField] private float cooldown = 3.0f;

        private float lastPulseTime = -10f;

        private void Update()
        {
            if (ToolInventory.Instance == null || ToolInventory.Instance.ActiveTool != CyberToolType.QuarantinePulse)
            {
                return;
            }

            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Q)) && Time.time - lastPulseTime >= cooldown)
            {
                ExecuteQuarantinePulse();
            }
        }

        private void ExecuteQuarantinePulse()
        {
            if (!ToolInventory.Instance.ConsumeEnergy(energyCost))
            {
                Debug.LogWarning("[QuarantinePulse] Insufficient energy for quarantine shockwave!");
                return;
            }

            lastPulseTime = Time.time;
            SentinelAudioSystem.Instance?.PlayQuarantinePulse();
            StartCoroutine(AnimateEmpShockwave(transform.position, pulseRadius));
            Debug.Log("[QuarantinePulse] Discharging Subnet Quarantine EMP wave!");

            Collider[] hits = Physics.OverlapSphere(transform.position, pulseRadius);
            int quarantinedCount = 0;

            foreach (var hit in hits)
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.ApplyStun(stunDuration);
                    enemy.TakeDamage(pulseDamage);

                    // Knockback
                    Rigidbody rb = enemy.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                        rb.AddForce(pushDir * knockbackForce, ForceMode.Impulse);
                    }

                    quarantinedCount++;
                }
            }

            Debug.Log($"[QuarantinePulse] Quarantined and stunned {quarantinedCount} hostile breach entities.");
        }

        private System.Collections.IEnumerator AnimateEmpShockwave(Vector3 origin, float maxRadius)
        {
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "EmpShockwaveFX";
            shockwave.transform.position = origin;
            shockwave.transform.localScale = new Vector3(0.2f, 0.04f, 0.2f);
            var col = shockwave.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var rend = shockwave.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.83f, 0.36f, 1.0f, 0.5f); // Violet / Magenta
                rend.material = mat;
            }

            float elapsed = 0f;
            float dur = 0.5f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentRad = Mathf.Lerp(0.5f, maxRadius * 2f, t);
                if (shockwave != null)
                {
                    shockwave.transform.localScale = new Vector3(currentRad, 0.04f, currentRad);
                    if (rend != null)
                    {
                        rend.material.color = new Color(0.83f, 0.36f, 1.0f, (1f - t) * 0.5f);
                    }
                }
                yield return null;
            }
            if (shockwave != null) Destroy(shockwave);
        }
    }
}
