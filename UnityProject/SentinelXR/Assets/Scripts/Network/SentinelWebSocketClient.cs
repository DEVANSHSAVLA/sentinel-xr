using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SentinelXR.Network
{
    public class SentinelWebSocketClient : MonoBehaviour
    {
        public static SentinelWebSocketClient Instance { get; private set; }

        [SerializeField] private string wsUri = "ws://localhost:3000/ws";
        private ClientWebSocket webSocket;
        private CancellationTokenSource cts;
        private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        public event Action<string> OnSecurityEventReceived;
        public event Action<string> OnTopologyUpdated;
        public event Action<string> OnIncidentUpdated;

        public void SetWebSocketUri(string newUri)
        {
            if (!string.IsNullOrEmpty(newUri))
            {
                wsUri = newUri;
                Debug.Log($"[SentinelWebSocketClient] WebSocket URI set to: {wsUri}");
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                var config = SentinelXR.Core.SentinelRuntimeConfig.Current;
                if (!string.IsNullOrEmpty(config.webSocketUrl))
                {
                    wsUri = config.webSocketUrl;
                }
                Debug.Log($"[SentinelWebSocketClient] Initialized with endpoint: {wsUri}");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            await ConnectAsync();
        }

        private void Update()
        {
            while (mainThreadQueue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }

        public async Task ConnectAsync()
        {
            try
            {
                cts = new CancellationTokenSource();
                webSocket = new ClientWebSocket();
                Uri serverUri = new Uri(wsUri);
                Debug.Log($"[SENTINEL-XR WS] Connecting to WebSocket bus at {wsUri}...");

                await webSocket.ConnectAsync(serverUri, cts.Token);
                Debug.Log("[SENTINEL-XR WS] Connected to live backend event bus!");

                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SENTINEL-XR WS] Connection failed: {ex.Message}. Will retry in 5s.");
                _ = ScheduleReconnectAsync();
            }
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[8192];
            while (webSocket != null && webSocket.State == WebSocketState.Open && !cts.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    mainThreadQueue.Enqueue(() => DispatchMessage(message));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SENTINEL-XR WS] Receive error: {ex.Message}");
                    break;
                }
            }

            _ = ScheduleReconnectAsync();
        }

        private void DispatchMessage(string json)
        {
            if (json.Contains("\"type\":\"SECURITY_EVENT\""))
            {
                OnSecurityEventReceived?.Invoke(json);
            }
            else if (json.Contains("\"type\":\"TOPOLOGY_UPDATE\""))
            {
                OnTopologyUpdated?.Invoke(json);
            }
            else if (json.Contains("\"type\":\"INCIDENT_UPDATE\"") || json.Contains("\"type\":\"INCIDENT_CREATED\""))
            {
                OnIncidentUpdated?.Invoke(json);
            }
        }

        private async Task ScheduleReconnectAsync()
        {
            await Task.Delay(5000);
            if (this != null && gameObject != null)
            {
                await ConnectAsync();
            }
        }

        private async void OnDestroy()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "App Exiting", CancellationToken.None);
                webSocket.Dispose();
            }
        }
    }
}
