using UnityEngine;
using SentinelXR.Gameplay.Player;

namespace SentinelXR.Gameplay.Environment
{
    public enum NetworkNodeRole
    {
        EdgeGateway,
        DMZWebServer,
        DatabaseVault,
        CoreRouter,
        AuthService
    }

    public class CyberNetworkNode : MonoBehaviour, IInteractable
    {
        [Header("Node Identity")]
        public string NodeId = "node-01";
        public string NodeName = "DMZ Gateway Alpha";
        public string NodeIp = "10.0.1.1";
        public NetworkNodeRole Role = NetworkNodeRole.EdgeGateway;

        [Header("Integrity & Corruption")]
        [SerializeField] private float maxIntegrity = 100.0f;
        [SerializeField] private float currentIntegrity = 100.0f;
        [SerializeField] private float corruptionLevel = 0.0f; // 0 to 100%

        [Header("Visual Components")]
        [SerializeField] private Renderer nodeRenderer;
        [SerializeField] private Light statusLight;

        private Material nodeMaterial;
        private bool isDiagnosticsVisible = false;
        private float diagnosticsTimer = 0f;

        public float CurrentIntegrity => currentIntegrity;
        public float MaxIntegrity => maxIntegrity;
        public float CorruptionLevel => corruptionLevel;
        public bool IsCompromised => currentIntegrity <= 0 || corruptionLevel >= 100f;

        public static event System.Action<CyberNetworkNode, float> OnNodeUnderAttack;
        public static event System.Action<CyberNetworkNode, float> OnNodeIntegrityChanged;
        public static event System.Action<CyberNetworkNode> OnNodeSecured;

        private void Start()
        {
            if (nodeRenderer == null)
            {
                nodeRenderer = GetComponent<Renderer>();
            }

            if (nodeRenderer != null)
            {
                nodeMaterial = nodeRenderer.material;
            }

            if (statusLight == null)
            {
                statusLight = GetComponentInChildren<Light>();
            }

            UpdateVisualState();
        }

        private void Update()
        {
            if (isDiagnosticsVisible)
            {
                diagnosticsTimer -= Time.deltaTime;
                if (diagnosticsTimer <= 0)
                {
                    isDiagnosticsVisible = false;
                }
            }
        }

        public void TakeCorruptionDamage(float amount)
        {
            currentIntegrity = Mathf.Clamp(currentIntegrity - amount, 0f, maxIntegrity);
            corruptionLevel = Mathf.Clamp(corruptionLevel + amount * 0.8f, 0f, 100f);
            UpdateVisualState();

            OnNodeUnderAttack?.Invoke(this, currentIntegrity);
            OnNodeIntegrityChanged?.Invoke(this, currentIntegrity);

            if (IsCompromised)
            {
                Debug.LogWarning($"[CyberNetworkNode] {NodeName} ({NodeIp}) has been COMPROMISED by breach!");
            }
        }

        public void RepairNode(float amount)
        {
            currentIntegrity = Mathf.Clamp(currentIntegrity + amount, 0f, maxIntegrity);
            corruptionLevel = Mathf.Clamp(corruptionLevel - amount * 1.2f, 0f, 100f);
            UpdateVisualState();

            OnNodeIntegrityChanged?.Invoke(this, currentIntegrity);
            if (currentIntegrity >= 98f && corruptionLevel <= 2f)
            {
                OnNodeSecured?.Invoke(this);
            }
        }

        public void RevealNodeDiagnostics(float duration = 4.0f)
        {
            isDiagnosticsVisible = true;
            diagnosticsTimer = duration;
        }

        private void UpdateVisualState()
        {
            Color nodeColor;
            if (corruptionLevel > 60f || currentIntegrity < 30f)
            {
                nodeColor = Color.red; // Critical / Compromised
            }
            else if (corruptionLevel > 20f || currentIntegrity < 70f)
            {
                nodeColor = new Color(1f, 0.6f, 0f); // Warning / Amber
            }
            else
            {
                nodeColor = Color.cyan; // Nominal
            }

            if (nodeMaterial != null)
            {
                nodeMaterial.color = nodeColor;
                if (nodeMaterial.HasProperty("_EmissionColor"))
                {
                    nodeMaterial.SetColor("_EmissionColor", nodeColor * 1.5f);
                }
            }

            if (statusLight != null)
            {
                statusLight.color = nodeColor;
            }
        }

        public string GetInteractionPrompt()
        {
            if (corruptionLevel > 10f || currentIntegrity < maxIntegrity)
            {
                return $"[E] Quick-Patch {NodeName} ({currentIntegrity:F0}%)";
            }
            return $"[E] Access {NodeName} Terminal";
        }

        public void Interact(GameObject player)
        {
            RepairNode(35.0f);
            Debug.Log($"[CyberNetworkNode] Operator initiated manual patch protocol on {NodeName}. Integrity restored to {currentIntegrity:F0}%");
        }
    }
}
