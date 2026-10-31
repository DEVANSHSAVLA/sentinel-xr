using UnityEngine;

namespace SentinelXR.Gameplay.Environment
{
    [RequireComponent(typeof(LineRenderer))]
    public class NetworkConnectionLink : MonoBehaviour
    {
        public CyberNetworkNode SourceNode;
        public CyberNetworkNode TargetNode;

        private LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            UpdateLinePositions();
        }

        private void Update()
        {
            UpdateLinePositions();

            // Dynamic color based on node health
            if (SourceNode != null && TargetNode != null)
            {
                if (SourceNode.IsCompromised || TargetNode.IsCompromised)
                {
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;
                }
                else if (SourceNode.CorruptionLevel > 30f || TargetNode.CorruptionLevel > 30f)
                {
                    lineRenderer.startColor = new Color(1f, 0.5f, 0f);
                    lineRenderer.endColor = new Color(1f, 0.5f, 0f);
                }
                else
                {
                    lineRenderer.startColor = new Color(0f, 0.8f, 1f, 0.6f);
                    lineRenderer.endColor = new Color(0f, 1f, 0.6f, 0.6f);
                }
            }
        }

        private void UpdateLinePositions()
        {
            if (SourceNode != null && TargetNode != null && lineRenderer != null)
            {
                lineRenderer.SetPosition(0, SourceNode.transform.position);
                lineRenderer.SetPosition(1, TargetNode.transform.position);
            }
        }
    }
}
