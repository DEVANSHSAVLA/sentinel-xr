using System;
using UnityEngine;

namespace SentinelXR.Gameplay.Player
{
    public enum CyberToolType
    {
        TraceScanner = 0,
        PatchTool = 1,
        QuarantinePulse = 2,
        FirewallShield = 3
    }

    public class ToolInventory : MonoBehaviour
    {
        public static ToolInventory Instance { get; private set; }

        [Header("Tools")]
        [SerializeField] private CyberToolType activeTool = CyberToolType.TraceScanner;
        [SerializeField] private float maxToolEnergy = 100.0f;
        [SerializeField] private float currentToolEnergy = 100.0f;
        [SerializeField] private float energyRechargeRate = 15.0f;

        public CyberToolType ActiveTool => activeTool;
        public float ToolEnergy => currentToolEnergy;
        public float MaxToolEnergy => maxToolEnergy;

        public event Action<CyberToolType> OnToolChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            // Force-log the initial tool selection (bypasses the change-only guard)
            Debug.Log($"[ToolInventory] Initial defensive cyber tool: {activeTool}");
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.InMission)
            {
                return;
            }

            // Keyboard selection 1, 2, 3, 4
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectTool(CyberToolType.TraceScanner);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectTool(CyberToolType.PatchTool);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectTool(CyberToolType.QuarantinePulse);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectTool(CyberToolType.FirewallShield);

            // Mouse wheel selection
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.05f)
            {
                int next = ((int)activeTool + 1) % 4;
                SelectTool((CyberToolType)next);
            }
            else if (scroll < -0.05f)
            {
                int prev = ((int)activeTool - 1 + 4) % 4;
                SelectTool((CyberToolType)prev);
            }

            // Energy recharge
            if (currentToolEnergy < maxToolEnergy)
            {
                currentToolEnergy = Mathf.MoveTowards(currentToolEnergy, maxToolEnergy, energyRechargeRate * Time.deltaTime);
            }
        }

        public void SelectTool(CyberToolType tool)
        {
            // Only log and fire event when the tool actually changes
            if (activeTool == tool) return;

            activeTool = tool;
            Debug.Log($"[ToolInventory] Active defensive cyber tool: {activeTool}");
            OnToolChanged?.Invoke(activeTool);
        }

        public bool ConsumeEnergy(float amount)
        {
            if (currentToolEnergy >= amount)
            {
                currentToolEnergy -= amount;
                return true;
            }
            return false;
        }
    }
}
