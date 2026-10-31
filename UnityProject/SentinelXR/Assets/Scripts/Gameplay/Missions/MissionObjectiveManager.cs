using System;
using System.Collections.Generic;
using UnityEngine;
using SentinelXR.Gameplay.Environment;
using SentinelXR.Gameplay.Enemies;

namespace SentinelXR.Gameplay.Missions
{
    [System.Serializable]
    public class MissionObjective
    {
        public string id;
        public string title;
        public string description;
        public bool isCompleted;
    }

    public class MissionObjectiveManager : MonoBehaviour
    {
        public static MissionObjectiveManager Instance { get; private set; }

        [Header("Mission 01 Objectives")]
        [SerializeField] private List<MissionObjective> objectives = new List<MissionObjective>();

        [Header("Tracking Metrics")]
        [SerializeField] private int threatsEliminatedCount = 0;
        [SerializeField] private int requiredThreatsWave1 = 5;
        [SerializeField] private float subnetIntegrity = 100.0f;
        [SerializeField] private float missionTimer = 0f;

        private CyberNetworkNode[] networkNodes;
        private BreachCoreBoss breachCore;

        public List<MissionObjective> Objectives => objectives;
        public float SubnetIntegrity => subnetIntegrity;
        public float MissionTime => missionTimer;
        public int ThreatsEliminated => threatsEliminatedCount;

        public event Action<MissionObjective> OnObjectiveCompleted;
        public event Action<float> OnSubnetIntegrityChanged;

        private void Awake()
        {
            Instance = this;
            InitializeMission01Objectives();
        }

        private void Start()
        {
            networkNodes = FindObjectsOfType<CyberNetworkNode>();
            breachCore = FindObjectOfType<BreachCoreBoss>();

            // Initialize Gateway Node into a corrupted breach state requiring operator patch
            if (networkNodes != null)
            {
                foreach (var node in networkNodes)
                {
                    if (node.Role == NetworkNodeRole.EdgeGateway)
                    {
                        node.TakeCorruptionDamage(65f); // Sets integrity to 35%, corruption to 65%
                    }
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.InMission)
            {
                return;
            }

            missionTimer += Time.deltaTime;
            EvaluateSubnetIntegrity();
            CheckObjectiveConditions();
        }

        private void InitializeMission01Objectives()
        {
            objectives.Clear();
            objectives.Add(new MissionObjective
            {
                id = "OBJ-01",
                title = "Trace Anomalous Breach Signal",
                description = "Equip Trace Scanner [1] and scan sector to identify corrupted nodes.",
                isCompleted = false
            });
            objectives.Add(new MissionObjective
            {
                id = "OBJ-02",
                title = "Secure DMZ Gateway Node",
                description = "Navigate to Node_DMZGateway and use Patch Tool [2] to restore integrity to 100%.",
                isCompleted = false
            });
            objectives.Add(new MissionObjective
            {
                id = "OBJ-03",
                title = "Contain Threat Infiltration",
                description = "Defeat 5 incoming malware threat entities (0/5 eliminated).",
                isCompleted = false
            });
            objectives.Add(new MissionObjective
            {
                id = "OBJ-04",
                title = "Neutralize Breach Core Alpha",
                description = "Destroy the anomaly breach core once its protective shield drops.",
                isCompleted = false
            });
        }

        private void EvaluateSubnetIntegrity()
        {
            if (networkNodes == null || networkNodes.Length == 0)
            {
                networkNodes = FindObjectsOfType<CyberNetworkNode>();
                if (networkNodes.Length == 0) return;
            }

            float totalIntegrity = 0f;
            foreach (var node in networkNodes)
            {
                totalIntegrity += node.CurrentIntegrity;
            }

            subnetIntegrity = totalIntegrity / networkNodes.Length;
            OnSubnetIntegrityChanged?.Invoke(subnetIntegrity);

            if (subnetIntegrity <= 0f)
            {
                Debug.LogError("[MissionObjectiveManager] SUBNET INTEGRITY COLLAPSED (0%)! Total System Breach.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
        }

        private void CheckObjectiveConditions()
        {
            // OBJ-01: Scanned sector with Trace Scanner
            if (!objectives[0].isCompleted)
            {
                var scanner = FindObjectOfType<Tools.TraceScanner>();
                if (scanner != null && !string.IsNullOrEmpty(scanner.LastDecodedTelemetry))
                {
                    CompleteObjective("OBJ-01");
                }
            }

            // OBJ-02: Patch DMZ Gateway Node to 100%
            if (objectives[0].isCompleted && !objectives[1].isCompleted)
            {
                foreach (var node in networkNodes)
                {
                    if (node.Role == NetworkNodeRole.EdgeGateway)
                    {
                        objectives[1].description = $"Use Patch Tool [2] to restore DMZ Gateway ({node.CurrentIntegrity:F0}%/100%).";
                        if (node.CurrentIntegrity >= 98f && node.CorruptionLevel <= 2f)
                        {
                            CompleteObjective("OBJ-02");
                            Debug.Log("[MissionObjectiveManager] Gateway secured! Initiating sector threat defense.");
                            break;
                        }
                    }
                }
            }

            // OBJ-03: Threats eliminated (after gateway is secured)
            if (objectives[1].isCompleted && !objectives[2].isCompleted)
            {
                objectives[2].description = $"Defeat 5 incoming malware threat entities ({threatsEliminatedCount}/{requiredThreatsWave1} eliminated).";
                if (threatsEliminatedCount >= requiredThreatsWave1)
                {
                    CompleteObjective("OBJ-03");

                    // Drop shield on Breach Core!
                    if (breachCore != null)
                    {
                        breachCore.SetShieldActive(false);
                        Debug.Log("[MissionObjectiveManager] Threat wave repelled! Breach Core shield DROPPED — Core is VULNERABLE!");
                    }
                }
            }

            // OBJ-04: Breach Core destroyed
            if (objectives[2].isCompleted && !objectives[3].isCompleted)
            {
                if (breachCore == null || breachCore.Health <= 0)
                {
                    CompleteObjective("OBJ-04");
                    OnMissionSuccess();
                }
            }
        }

        public void RecordThreatEliminated(EnemyBase enemy)
        {
            threatsEliminatedCount++;
            Debug.Log($"[MissionObjectiveManager] Threat eliminated! Total: {threatsEliminatedCount}");
        }

        public void CompleteObjective(string objId)
        {
            var obj = objectives.Find(o => o.id == objId);
            if (obj != null && !obj.isCompleted)
            {
                obj.isCompleted = true;
                Debug.Log($"[MissionObjectiveManager] OBJECTIVE COMPLETED: {obj.title}");
                OnObjectiveCompleted?.Invoke(obj);
            }
        }

        private void OnMissionSuccess()
        {
            Debug.Log("[MissionObjectiveManager] *** MISSION 01 ACCOMPLISHED! ALL OBJECTIVES SECURED ***");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordMissionDebrief(threatsEliminatedCount, subnetIntegrity, missionTimer);
            }
        }
    }
}
