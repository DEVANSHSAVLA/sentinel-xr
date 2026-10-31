using System.Collections;
using UnityEngine;
using SentinelXR.Gameplay.Player;

namespace SentinelXR.Gameplay.Tools
{
    public class FirewallShield : MonoBehaviour
    {
        [Header("Shield Settings")]
        [SerializeField] private float shieldDuration = 6.0f;
        [SerializeField] private float shieldHealth = 150.0f;
        [SerializeField] private float energyCost = 35.0f;
        [SerializeField] private float cooldown = 5.0f;
        [SerializeField] private GameObject shieldBarrierPrefab;

        private float lastDeployTime = -10f;
        private GameObject activeBarrier;

        private void Update()
        {
            if (ToolInventory.Instance == null || ToolInventory.Instance.ActiveTool != CyberToolType.FirewallShield)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0) && Time.time - lastDeployTime >= cooldown)
            {
                DeployFirewall();
            }
        }

        private void DeployFirewall()
        {
            if (!ToolInventory.Instance.ConsumeEnergy(energyCost))
            {
                Debug.LogWarning("[FirewallShield] Insufficient energy to deploy firewall barrier!");
                return;
            }

            lastDeployTime = Time.time;
            SentinelAudioSystem.Instance?.PlayFirewallDeploy();
            Debug.Log("[FirewallShield] Deploying Directional Stateful Firewall Barrier...");

            if (activeBarrier != null)
            {
                Destroy(activeBarrier);
            }

            // Spawn barrier in front of player
            Vector3 spawnPos = transform.position + transform.forward * 2.5f + Vector3.up * 0.5f;
            Quaternion spawnRot = Quaternion.LookRotation(transform.forward);

            activeBarrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            activeBarrier.name = "ActiveFirewallBarrier";
            activeBarrier.transform.position = spawnPos;
            activeBarrier.transform.rotation = spawnRot;
            activeBarrier.transform.localScale = new Vector3(3.5f, 2.2f, 0.2f);
            activeBarrier.tag = "FirewallShield";
            activeBarrier.AddComponent<FirewallBarrier>();

            // Visual setup
            Renderer rend = activeBarrier.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader holoShader = Shader.Find("SentinelXR/ObsidianHologram");
                if (holoShader == null) holoShader = Shader.Find("Sprites/Default");
                Material mat = new Material(holoShader);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0f, 0.85f, 1f, 0.65f));
                if (mat.HasProperty("_RimColor")) mat.SetColor("_RimColor", new Color(0.28f, 0.48f, 1f, 1f));
                rend.material = mat;
            }

            StartCoroutine(DestroyBarrierAfterTime(activeBarrier, shieldDuration));
        }

        private IEnumerator DestroyBarrierAfterTime(GameObject barrier, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (barrier != null)
            {
                Destroy(barrier);
            }
        }
    }
}
