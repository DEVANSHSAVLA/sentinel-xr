using System;
using System.IO;
using UnityEngine;

namespace SentinelXR.Core
{
    [System.Serializable]
    public class RuntimeConfigData
    {
        public string environment = "local-dev";
        public string apiBaseUrl = "http://127.0.0.1:3000/api/v1";
        public string webSocketUrl = "ws://127.0.0.1:3000/ws";
        public int timeoutMs = 8000;
        public int reconnectIntervalMs = 3000;
        public bool authRequired = true;
        public string defaultOperator = "operator-01";
        public string bearerToken = "token-operator-alpha-9921";
    }

    /// <summary>
    /// SentinelRuntimeConfig dynamically resolves backend endpoints on startup.
    /// Priority order:
    /// 1. Command-line arguments (--api-url <url>, --ws-url <url>, --env <mode>)
    /// 2. Environment variables (SENTINEL_API_URL, SENTINEL_WS_URL, SENTINEL_ENV)
    /// 3. Configuration file (SentinelXR_Config.json in application directory or StreamingAssets)
    /// 4. Default fallback configuration (Local Development)
    /// </summary>
    public static class SentinelRuntimeConfig
    {
        private static RuntimeConfigData activeConfig;

        public static RuntimeConfigData Current
        {
            get
            {
                if (activeConfig == null)
                {
                    LoadConfiguration();
                }
                return activeConfig;
            }
        }

        public static void LoadConfiguration()
        {
            activeConfig = new RuntimeConfigData();

            // 1. Try loading from external JSON file
            string[] possibleConfigPaths = new string[]
            {
                Path.Combine(Application.dataPath, "../SentinelXR_Config.json"),
                Path.Combine(Application.dataPath, "SentinelXR_Config.json"),
                Path.Combine(Application.streamingAssetsPath, "SentinelXR_Config.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SentinelXR_Config.json"),
                "release/windows/SentinelXR_Config.json"
            };

            foreach (string path in possibleConfigPaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        Debug.Log($"[SentinelRuntimeConfig] Loaded config from: {path}");
                        
                        // Parse JSON fields if present
                        ParseConfigJson(json, activeConfig);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SentinelRuntimeConfig] Could not parse config at {path}: {ex.Message}");
                }
            }

            // 2. Override from Environment Variables
            string envApiUrl = Environment.GetEnvironmentVariable("SENTINEL_API_URL");
            if (!string.IsNullOrEmpty(envApiUrl))
            {
                activeConfig.apiBaseUrl = envApiUrl.TrimEnd('/');
                Debug.Log($"[SentinelRuntimeConfig] Applied SENTINEL_API_URL: {activeConfig.apiBaseUrl}");
            }

            string envWsUrl = Environment.GetEnvironmentVariable("SENTINEL_WS_URL");
            if (!string.IsNullOrEmpty(envWsUrl))
            {
                activeConfig.webSocketUrl = envWsUrl;
                Debug.Log($"[SentinelRuntimeConfig] Applied SENTINEL_WS_URL: {activeConfig.webSocketUrl}");
            }

            // 3. Override from Command-Line Arguments
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--api-url" || args[i] == "-api") && i + 1 < args.Length)
                {
                    activeConfig.apiBaseUrl = args[i + 1].TrimEnd('/');
                    Debug.Log($"[SentinelRuntimeConfig] CLI arg override API: {activeConfig.apiBaseUrl}");
                }
                else if ((args[i] == "--ws-url" || args[i] == "-ws") && i + 1 < args.Length)
                {
                    activeConfig.webSocketUrl = args[i + 1];
                    Debug.Log($"[SentinelRuntimeConfig] CLI arg override WS: {activeConfig.webSocketUrl}");
                }
                else if (args[i] == "--lan" || args[i] == "-lan")
                {
                    activeConfig.environment = "lan";
                }
            }

            Debug.Log($"[SentinelRuntimeConfig] Final Active Configuration: Env={activeConfig.environment}, API={activeConfig.apiBaseUrl}, WS={activeConfig.webSocketUrl}");
        }

        private static void ParseConfigJson(string json, RuntimeConfigData target)
        {
            // Simple parsing to handle flat or nested backend config
            if (json.Contains("\"apiBaseUrl\""))
            {
                int start = json.IndexOf("\"apiBaseUrl\"") + 12;
                int colon = json.IndexOf(':', start);
                int quote1 = json.IndexOf('"', colon);
                int quote2 = json.IndexOf('"', quote1 + 1);
                if (quote1 > 0 && quote2 > quote1)
                {
                    target.apiBaseUrl = json.Substring(quote1 + 1, quote2 - quote1 - 1);
                }
            }
            if (json.Contains("\"webSocketUrl\""))
            {
                int start = json.IndexOf("\"webSocketUrl\"") + 14;
                int colon = json.IndexOf(':', start);
                int quote1 = json.IndexOf('"', colon);
                int quote2 = json.IndexOf('"', quote1 + 1);
                if (quote1 > 0 && quote2 > quote1)
                {
                    target.webSocketUrl = json.Substring(quote1 + 1, quote2 - quote1 - 1);
                }
            }
        }
    }
}
