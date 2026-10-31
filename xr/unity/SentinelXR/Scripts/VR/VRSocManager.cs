using UnityEngine;
using SentinelXR.Network;

namespace SentinelXR.VR
{
    public class VRSocManager : MonoBehaviour
    {
        [Header("VR Holographic Rig")]
        [SerializeField] private Transform holographicTableCenter;
        [SerializeField] private GameObject networkNodePrefab;
        [SerializeField] private GameObject connectionEdgePrefab;
        [SerializeField] private Light roomAmbientLight;

        [Header("VR AI & Incident Displays")]
        [SerializeField] private Transform aiAssistantDisplay;
        [SerializeField] private Transform incidentQueueBoard;

        private void Start()
        {
            Debug.Log("[SENTINEL-XR VR] Initializing 6DoF Virtual Reality Security Operations Center...");
            ConfigureLighting();
        }

        private void ConfigureLighting()
        {
            if (roomAmbientLight != null)
            {
                roomAmbientLight.color = new Color(0.02f, 0.05f, 0.12f);
                RenderSettings.ambientSkyColor = new Color(0.04f, 0.08f, 0.18f);
            }
        }

        public void HighlightAttackedNode(string nodeId)
        {
            Debug.Log($"[SENTINEL-XR VR] Node {nodeId} under active simulated attack vector");
        }
    }
}
