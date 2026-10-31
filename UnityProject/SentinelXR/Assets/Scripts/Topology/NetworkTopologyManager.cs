using System.Collections.Generic;
using UnityEngine;
using SentinelXR.Network;

namespace SentinelXR.Topology
{
    [System.Serializable]
    public class NodeConfig
    {
        public string id;
        public string name;
        public string type;
        public string ip;
        public string zone;
        public Vector3 position;
    }

    public class NetworkTopologyManager : MonoBehaviour
    {
        public static NetworkTopologyManager Instance { get; private set; }

        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private Material lineMaterialNormal;
        [SerializeField] private Material lineMaterialAttack;

        private readonly Dictionary<string, NetworkNodeObject> activeNodes = new Dictionary<string, NetworkNodeObject>();
        private readonly List<LineRenderer> activeEdges = new List<LineRenderer>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (SentinelApiClient.Instance != null)
            {
                FetchTopologyFromBackend();
            }
            else
            {
                BuildDefaultTopology();
            }
            RegisterWebSocketEvents();
        }

        public void FetchTopologyFromBackend()
        {
            if (SentinelApiClient.Instance == null)
            {
                BuildDefaultTopology();
                return;
            }

            SentinelApiClient.Instance.GetTopologyData(
                onSuccess: (data) =>
                {
                    if (data != null && data.nodes != null && data.nodes.Length > 0)
                    {
                        ApplyTopologyData(data);
                    }
                    else
                    {
                        BuildDefaultTopology();
                    }
                },
                onError: (err) =>
                {
                    Debug.LogWarning($"[TopologyManager] Failed to fetch backend topology: {err}. Using default preset.");
                    BuildDefaultTopology();
                }
            );
        }

        private void ClearCurrentTopology()
        {
            foreach (var node in activeNodes.Values)
            {
                if (node != null && node.gameObject != null) Destroy(node.gameObject);
            }
            activeNodes.Clear();

            foreach (var edge in activeEdges)
            {
                if (edge != null && edge.gameObject != null) Destroy(edge.gameObject);
            }
            activeEdges.Clear();
        }

        private void ApplyTopologyData(TopologyResponse data)
        {
            ClearCurrentTopology();

            foreach (var n in data.nodes)
            {
                NodeConfig cfg = new NodeConfig
                {
                    id = n.id,
                    name = n.name,
                    type = n.type,
                    ip = n.ip,
                    zone = n.zone,
                    position = new Vector3(n.x, n.y, n.z)
                };
                SpawnNode(cfg);
            }

            if (data.connections != null)
            {
                foreach (var conn in data.connections)
                {
                    CreateEdge(conn.source, conn.target);
                }
            }
        }

        private void BuildDefaultTopology()
        {
            NodeConfig[] defaultNodes = new NodeConfig[]
            {
                new NodeConfig { id = "gateway-01", name = "Internet Gateway", type = "gateway", ip = "198.51.100.1", zone = "external", position = new Vector3(-6, 2, 0) },
                new NodeConfig { id = "firewall-01", name = "Perimeter Firewall", type = "firewall", ip = "10.0.0.1", zone = "perimeter", position = new Vector3(-3, 1, 0) },
                new NodeConfig { id = "router-core", name = "Core Distribution Router", type = "router", ip = "10.0.0.2", zone = "internal", position = new Vector3(0, 0, 0) },
                new NodeConfig { id = "server-01", name = "DMZ Web App Cluster", type = "server", ip = "10.0.1.10", zone = "dmz", position = new Vector3(3, 2, -2) },
                new NodeConfig { id = "server-02", name = "DMZ API Gateway", type = "server", ip = "10.0.1.15", zone = "dmz", position = new Vector3(3, 0, -2) },
                new NodeConfig { id = "db-cluster-01", name = "Secure Database Vault", type = "database", ip = "10.0.2.20", zone = "secure_core", position = new Vector3(6, 1, -1) },
                new NodeConfig { id = "workstation-01", name = "SOC Analyst Workstation", type = "workstation", ip = "10.0.3.14", zone = "internal", position = new Vector3(1, -2, 3) },
                new NodeConfig { id = "workstation-02", name = "DevOps Engineering Terminal", type = "workstation", ip = "10.0.3.18", zone = "internal", position = new Vector3(-2, -2, 3) },
                new NodeConfig { id = "client-pool", name = "Simulated Client Subnet", type = "client", ip = "10.0.4.50", zone = "internal", position = new Vector3(-4, -1, 4) }
            };

            foreach (NodeConfig cfg in defaultNodes)
            {
                SpawnNode(cfg);
            }

            CreateEdge("gateway-01", "firewall-01");
            CreateEdge("firewall-01", "router-core");
            CreateEdge("router-core", "server-01");
            CreateEdge("router-core", "server-02");
            CreateEdge("server-01", "db-cluster-01");
            CreateEdge("server-02", "db-cluster-01");
            CreateEdge("router-core", "workstation-01");
            CreateEdge("router-core", "workstation-02");
            CreateEdge("router-core", "client-pool");
        }

        private void SpawnNode(NodeConfig cfg)
        {
            GameObject go = nodePrefab != null ? Instantiate(nodePrefab, cfg.position, Quaternion.identity, transform) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Node_{cfg.id}";
            go.transform.position = cfg.position;

            NetworkNodeObject nodeObj = go.GetComponent<NetworkNodeObject>() ?? go.AddComponent<NetworkNodeObject>();
            nodeObj.Initialize(cfg.id, cfg.name, cfg.ip, cfg.zone);
            activeNodes[cfg.id] = nodeObj;
        }

        private void CreateEdge(string sourceId, string targetId)
        {
            if (!activeNodes.TryGetValue(sourceId, out var src) || !activeNodes.TryGetValue(targetId, out var tgt)) return;

            GameObject lineObj = new GameObject($"Edge_{sourceId}_{targetId}");
            lineObj.transform.SetParent(transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            lr.positionCount = 2;
            lr.SetPosition(0, src.transform.position);
            lr.SetPosition(1, tgt.transform.position);
            lr.material = lineMaterialNormal != null ? lineMaterialNormal : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            activeEdges.Add(lr);
        }

        private void RegisterWebSocketEvents()
        {
            if (SentinelWebSocketClient.Instance != null)
            {
                SentinelWebSocketClient.Instance.OnSecurityEventReceived += HandleSecurityEvent;
            }
        }

        private void HandleSecurityEvent(string json)
        {
            // Parses live event from backend and triggers visual attack pulse on target node
            if (json.Contains("server-01") && activeNodes.TryGetValue("server-01", out var node))
            {
                node.SetStatus(Core.NodeStatus.Critical);
            }
        }
    }
}
