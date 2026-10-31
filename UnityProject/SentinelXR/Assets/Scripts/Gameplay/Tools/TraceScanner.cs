using UnityEngine;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Environment;

namespace SentinelXR.Gameplay.Tools
{
    public class TraceScanner : MonoBehaviour
    {
        [Header("Scanner Parameters")]
        [SerializeField] private float scanRange = 25.0f;
        [SerializeField] private float scanCooldown = 1.0f;
        [SerializeField] private float energyCost = 15.0f;
        [SerializeField] private LayerMask scanLayer = ~0;

        private float lastScanTime = -10f;
        private Camera playerCamera;
        private string lastDecodedTelemetry = "";

        public string LastDecodedTelemetry => lastDecodedTelemetry;

        private void Start()
        {
            playerCamera = GetComponentInParent<Camera>();
            if (playerCamera == null && SentinelPlayerController.Instance != null)
            {
                playerCamera = SentinelPlayerController.Instance.PlayerCamera;
            }
        }

        private void Update()
        {
            if (ToolInventory.Instance == null || ToolInventory.Instance.ActiveTool != CyberToolType.TraceScanner)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0) && Time.time - lastScanTime >= scanCooldown)
            {
                ExecuteTraceScan();
            }
        }

        private void ExecuteTraceScan()
        {
            if (!ToolInventory.Instance.ConsumeEnergy(energyCost))
            {
                Debug.LogWarning("[TraceScanner] Insufficient cyber energy for scan pulse!");
                return;
            }

            lastScanTime = Time.time;
            SentinelAudioSystem.Instance?.PlayScanPulse();
            StartCoroutine(AnimateScanRing(transform.position, scanRange));
            Debug.Log("[TraceScanner] Emitting 360° electromagnetic trace scan pulse...");

            // Spherecast / overlap for nodes and enemies
            Collider[] colliders = Physics.OverlapSphere(transform.position, scanRange, scanLayer);
            int detectedEntities = 0;

            foreach (var col in colliders)
            {
                // Check Cyber Network Nodes
                CyberNetworkNode node = col.GetComponent<CyberNetworkNode>();
                if (node != null)
                {
                    node.RevealNodeDiagnostics();
                    lastDecodedTelemetry = $"NODE: {node.NodeName} | IP: {node.NodeIp} | INTEGRITY: {node.CurrentIntegrity:F0}% | THREAT: {node.CorruptionLevel:F0}%";
                    detectedEntities++;
                }

                // Check Enemies (reveal stealth Trojan Infiltrators)
                var stealthEnemy = col.GetComponent<Enemies.EnemyTrojanInfiltrator>();
                if (stealthEnemy != null)
                {
                    stealthEnemy.RevealFromStealth(5.0f);
                    detectedEntities++;
                }
            }

            if (detectedEntities == 0)
            {
                lastDecodedTelemetry = "SCAN ACTIVE: No corrupted packets or breach signatures in immediate sector.";
            }
        }

        private System.Collections.IEnumerator AnimateScanRing(Vector3 origin, float maxRadius)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ScanPulseFX";
            ring.transform.position = origin;
            ring.transform.localScale = new Vector3(0.1f, 0.02f, 0.1f);
            var col = ring.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var rend = ring.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0f, 0.9f, 1f, 0.4f);
                rend.material = mat;
            }

            float elapsed = 0f;
            float dur = 0.45f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentRad = Mathf.Lerp(0.5f, maxRadius * 2f, t);
                if (ring != null)
                {
                    ring.transform.localScale = new Vector3(currentRad, 0.02f, currentRad);
                    if (rend != null)
                    {
                        rend.material.color = new Color(0f, 0.9f, 1f, (1f - t) * 0.4f);
                    }
                }
                yield return null;
            }
            if (ring != null) Destroy(ring);
        }
    }
}
