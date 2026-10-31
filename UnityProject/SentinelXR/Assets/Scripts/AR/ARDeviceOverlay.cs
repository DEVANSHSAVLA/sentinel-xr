using UnityEngine;
using UnityEngine.UI;
using SentinelXR.Network;

namespace SentinelXR.AR
{
    public class ARDeviceOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text deviceNameText;
        [SerializeField] private Text ipAddressText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text riskScoreText;
        [SerializeField] private Text telemetryText;
        [SerializeField] private Image riskBarImage;
        [SerializeField] private Button isolateButton;

        private string boundDeviceId;
        private bool isIsolated = false;

        public void Initialize(string deviceId)
        {
            boundDeviceId = deviceId;
            FetchTelemetry();
        }

        public void FetchTelemetry()
        {
            if (SentinelApiClient.Instance == null) return;

            SentinelApiClient.Instance.GetDeviceTelemetry(boundDeviceId, (device) =>
            {
                if (deviceNameText != null) deviceNameText.text = device.name;
                if (ipAddressText != null) ipAddressText.text = $"{device.ip} ({device.zone.ToUpper()})";
                if (statusText != null) statusText.text = device.status.ToUpper();
                if (riskScoreText != null) riskScoreText.text = $"RISK: {device.riskScore}/100";
                if (telemetryText != null) telemetryText.text = $"CPU: {device.cpuUsage}% | RAM: {device.memoryUsage}% | Traffic: {device.trafficRate} p/s";

                if (riskBarImage != null)
                {
                    riskBarImage.color = device.riskScore > 65 ? Color.red : device.riskScore > 35 ? Color.yellow : Color.cyan;
                }

                isIsolated = device.status == "isolated";
                if (isolateButton != null)
                {
                    Text btnText = isolateButton.GetComponentInChildren<Text>();
                    if (btnText != null) btnText.text = isIsolated ? "Reconnect Node" : "Quarantine Node";
                }
            }, (error) =>
            {
                Debug.LogWarning($"Failed to fetch telemetry for {boundDeviceId}: {error}");
            });
        }

        public void OnToggleIsolationClicked()
        {
            // Trigger containment or reconnect via API
            Debug.Log($"[SENTINEL-XR AR] Isolation toggle clicked for {boundDeviceId}");
            FetchTelemetry();
        }
    }
}
