using UnityEngine;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Environment;
using SentinelXR.Gameplay.Enemies;

namespace SentinelXR.Gameplay.Tools
{
    public class PatchTool : MonoBehaviour
    {
        [Header("Patch Settings")]
        [SerializeField] private float patchRange = 12.0f;
        [SerializeField] private float repairRate = 25.0f; // Integrity per second
        [SerializeField] private float cleanseDamageRate = 40.0f; // DPS to malware
        [SerializeField] private float energyDrainRate = 12.0f; // Energy per second
        [SerializeField] private LineRenderer beamRenderer;

        private Camera playerCamera;
        private bool isPatching = false;

        public bool IsPatching => isPatching;

        private void Start()
        {
            playerCamera = GetComponentInParent<Camera>();
            if (playerCamera == null && SentinelPlayerController.Instance != null)
            {
                playerCamera = SentinelPlayerController.Instance.PlayerCamera;
            }

            if (beamRenderer == null)
            {
                beamRenderer = GetComponent<LineRenderer>();
                if (beamRenderer == null)
                {
                    beamRenderer = gameObject.AddComponent<LineRenderer>();
                    beamRenderer.startWidth = 0.05f;
                    beamRenderer.endWidth = 0.08f;
                    beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    beamRenderer.startColor = Color.cyan;
                    beamRenderer.endColor = Color.green;
                    beamRenderer.positionCount = 2;
                    beamRenderer.enabled = false;
                }
            }
        }

        private void Update()
        {
            if (ToolInventory.Instance == null || ToolInventory.Instance.ActiveTool != CyberToolType.PatchTool)
            {
                if (beamRenderer != null) beamRenderer.enabled = false;
                isPatching = false;
                return;
            }

            if (Input.GetMouseButton(0) && ToolInventory.Instance.ConsumeEnergy(energyDrainRate * Time.deltaTime))
            {
                ExecutePatchBeam();
            }
            else
            {
                if (beamRenderer != null) beamRenderer.enabled = false;
                isPatching = false;
            }
        }

        private void ExecutePatchBeam()
        {
            isPatching = true;
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 endPoint = ray.origin + ray.direction * patchRange;

            if (Physics.Raycast(ray, out RaycastHit hit, patchRange))
            {
                endPoint = hit.point;

                // 1. Repair Cyber Network Node
                CyberNetworkNode node = hit.collider.GetComponent<CyberNetworkNode>();
                if (node != null)
                {
                    node.RepairNode(repairRate * Time.deltaTime);
                }

                // 2. Cleanse / Damage Malware Enemies
                EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(cleanseDamageRate * Time.deltaTime);
                }
            }

            if (beamRenderer != null)
            {
                beamRenderer.enabled = true;
                beamRenderer.SetPosition(0, transform.position);
                beamRenderer.SetPosition(1, endPoint);
            }
        }
    }
}
