using UnityEngine;
using SentinelXR.Core;

namespace SentinelXR.Topology
{
    [RequireComponent(typeof(Collider))]
    public class NetworkNodeObject : MonoBehaviour
    {
        [Header("State")]
        public string NodeId { get; private set; }
        public string NodeName { get; private set; }
        public string IpAddress { get; private set; }
        public string NetworkZone { get; private set; }
        public NodeStatus Status { get; private set; } = NodeStatus.Healthy;

        private MeshRenderer meshRenderer;
        private Material nodeMaterial;

        public void Initialize(string id, string name, string ip, string zone)
        {
            NodeId = id;
            NodeName = name;
            IpAddress = ip;
            NetworkZone = zone;
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                nodeMaterial = meshRenderer.material;
            }
            UpdateVisuals();
        }

        public void SetStatus(NodeStatus newStatus)
        {
            Status = newStatus;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (nodeMaterial == null) return;

            Color baseColor = Color.cyan;
            switch (Status)
            {
                case NodeStatus.Healthy:
                    baseColor = new Color(0.0f, 0.85f, 1.0f);
                    break;
                case NodeStatus.Suspicious:
                    baseColor = new Color(1.0f, 0.7f, 0.28f);
                    break;
                case NodeStatus.Critical:
                    baseColor = new Color(1.0f, 0.27f, 0.42f);
                    break;
                case NodeStatus.Isolated:
                    baseColor = new Color(0.35f, 0.43f, 0.55f);
                    break;
            }

            nodeMaterial.color = baseColor;
            if (nodeMaterial.HasProperty("_BaseColor")) nodeMaterial.SetColor("_BaseColor", baseColor);
            if (nodeMaterial.HasProperty("_EmissionColor")) nodeMaterial.SetColor("_EmissionColor", baseColor * 0.8f);
        }

        private void OnMouseDown()
        {
            Debug.Log($"[SENTINEL-XR] User selected node: {NodeName} ({NodeId}) - {IpAddress}");
        }
    }
}
