using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SentinelXR.VR
{
    public class VRNetworkInteraction : MonoBehaviour
    {
        [Header("Node Properties")]
        [SerializeField] private string nodeId;
        [SerializeField] private MeshRenderer coreRenderer;
        [SerializeField] private Light auraLight;

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log($"[SENTINEL-XR VR] Controller selected node: {nodeId}");
            if (auraLight != null)
            {
                auraLight.intensity = 2.5f;
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (auraLight != null)
            {
                auraLight.intensity = 1.0f;
            }
        }
    }
}
