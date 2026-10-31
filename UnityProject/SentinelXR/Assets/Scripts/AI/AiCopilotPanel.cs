using UnityEngine;
using UnityEngine.UI;
using SentinelXR.Network;

namespace SentinelXR.AI
{
    public class AiCopilotPanel : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private InputField queryInputField;
        [SerializeField] private Text responseDisplayText;
        [SerializeField] private Button sendButton;
        [SerializeField] private Text statusIndicatorText;

        [Header("Context")]
        [SerializeField] private string activeIncidentId = "INC-7041";

        private void Start()
        {
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(OnSendQueryClicked);
            }
        }

        public void OnSendQueryClicked()
        {
            string prompt = queryInputField != null ? queryInputField.text : "Explain current incident blast radius";
            if (string.IsNullOrWhiteSpace(prompt)) return;

            if (statusIndicatorText != null) statusIndicatorText.text = "Querying Gemini AI Copilot...";

            SentinelApiClient.Instance.AskAiCopilot(activeIncidentId, prompt, (answer) =>
            {
                if (responseDisplayText != null) responseDisplayText.text = answer;
                if (statusIndicatorText != null) statusIndicatorText.text = "Response verified by Gemini API";
            }, (error) =>
            {
                if (responseDisplayText != null) responseDisplayText.text = $"Copilot unavailable: {error}";
                if (statusIndicatorText != null) statusIndicatorText.text = "Offline Fallback Engine Active";
            });
        }
    }
}
