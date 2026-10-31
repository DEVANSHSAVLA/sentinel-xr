using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SentinelXR.Network
{
    [System.Serializable]
    public class DeviceData
    {
        public string id;
        public string name;
        public string type;
        public string ip;
        public string status;
        public int riskScore;
        public string zone;
        public int cpuUsage;
        public int memoryUsage;
        public int trafficRate;
    }

    [System.Serializable]
    public class IncidentData
    {
        public string id;
        public string title;
        public string severity;
        public string status;
        public string targetDeviceId;
        public string sourceIp;
        public string description;
        public int confidenceScore;
        public string recommendedAction;
        public bool containmentApproved;
    }

    [System.Serializable]
    public class CopilotRequest
    {
        public string incidentId;
        public string deviceId;
        public string userPrompt;
    }

    [System.Serializable]
    public class CopilotResponse
    {
        public string answer;
        public string modelUsed;
        public string source;
    }

    public class SentinelApiClient : MonoBehaviour
    {
        public static SentinelApiClient Instance { get; private set; }

        [Header("Backend Configuration")]
        [SerializeField] private string backendBaseUrl = "http://localhost:3000/api/v1";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void GetDeviceTelemetry(string deviceId, Action<DeviceData> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequestCoroutine($"{backendBaseUrl}/devices/{deviceId}", (json) =>
            {
                try
                {
                    DeviceData data = JsonUtility.FromJson<DeviceData>(json);
                    onSuccess?.Invoke(data);
                }
                catch (Exception e)
                {
                    onError?.Invoke(e.Message);
                }
            }, onError));
        }

        public void ApproveContainment(string incidentId, Action<IncidentData> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequestCoroutine($"{backendBaseUrl}/incidents/{incidentId}/contain", "{}", (json) =>
            {
                try
                {
                    IncidentData data = JsonUtility.FromJson<IncidentData>(json);
                    onSuccess?.Invoke(data);
                }
                catch (Exception e)
                {
                    onError?.Invoke(e.Message);
                }
            }, onError));
        }

        public void AskAiCopilot(string incidentId, string prompt, Action<string> onSuccess, Action<string> onError)
        {
            CopilotRequest req = new CopilotRequest { incidentId = incidentId, userPrompt = prompt };
            string bodyJson = JsonUtility.ToJson(req);

            StartCoroutine(PostRequestCoroutine($"{backendBaseUrl}/ai/investigate", bodyJson, (json) =>
            {
                try
                {
                    CopilotResponse resp = JsonUtility.FromJson<CopilotResponse>(json);
                    onSuccess?.Invoke(resp.answer);
                }
                catch (Exception e)
                {
                    onError?.Invoke(e.Message);
                }
            }, onError));
        }

        private IEnumerator GetRequestCoroutine(string uri, Action<string> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    onError?.Invoke(webRequest.error);
                }
                else
                {
                    onSuccess?.Invoke(webRequest.downloadHandler.text);
                }
            }
        }

        private IEnumerator PostRequestCoroutine(string uri, string jsonBody, Action<string> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    onError?.Invoke(webRequest.error);
                }
                else
                {
                    onSuccess?.Invoke(webRequest.downloadHandler.text);
                }
            }
        }
    }
}
