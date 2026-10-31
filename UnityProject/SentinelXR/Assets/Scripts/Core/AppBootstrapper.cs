using UnityEngine;
using SentinelXR.Network;

namespace SentinelXR.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Runtime Configuration")]
        [SerializeField] private ApplicationMode initialMode = ApplicationMode.DesktopSoc;
        [SerializeField] private string backendUrl = "http://localhost:3000/api/v1";
        [SerializeField] private string webSocketUrl = "ws://localhost:3000/ws";

        public static AppBootstrapper Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSubsystems();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSubsystems()
        {
            SentinelRuntimeConfig.LoadConfiguration();
            backendUrl = SentinelRuntimeConfig.Current.apiBaseUrl;
            webSocketUrl = SentinelRuntimeConfig.Current.webSocketUrl;

            Debug.Log($"[SENTINEL-XR 2.0] Bootstrapping Unity Cyber Threat Platform in mode: {initialMode} (API: {backendUrl})");
            
            // Setup Network Singletons
            if (GetComponent<SentinelApiClient>() == null)
            {
                gameObject.AddComponent<SentinelApiClient>();
            }

            if (GetComponent<SentinelWebSocketClient>() == null)
            {
                gameObject.AddComponent<SentinelWebSocketClient>();
            }
        }
    }
}
