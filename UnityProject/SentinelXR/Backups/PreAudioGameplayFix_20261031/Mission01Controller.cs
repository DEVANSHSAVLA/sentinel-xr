using System.Collections;
using UnityEngine;
using SentinelXR.Gameplay.Enemies;

namespace SentinelXR.Gameplay.Missions
{
    public class Mission01Controller : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float initialSpawnDelay = 3.0f;
        [SerializeField] private float waveInterval = 8.0f;
        [SerializeField] private int maxSimultaneousEnemies = 4;

        private int enemiesSpawned = 0;
        private int totalEnemiesToSpawn = 10;
        private bool isSpawning = false;
        private Coroutine spawnCoroutine;

        public float InitialSpawnDelay { get => initialSpawnDelay; set => initialSpawnDelay = value; }
        public float WaveInterval { get => waveInterval; set => waveInterval = value; }
        public int MaxSimultaneousEnemies { get => maxSimultaneousEnemies; set => maxSimultaneousEnemies = value; }
        public int EnemiesSpawned => enemiesSpawned;
        public int TotalEnemiesToSpawn { get => totalEnemiesToSpawn; set => totalEnemiesToSpawn = value; }
        public bool IsSpawning => isSpawning;

        private void Start()
        {
            Environment.Mission01EnvironmentBuilder.BuildFacilityIfMissing();
            spawnCoroutine = StartCoroutine(SpawnSequence());
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        /// <summary>
        /// Listens for state changes. Cancels spawning on MissionFailed or MissionSuccess.
        /// </summary>
        private void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.MissionFailed || newState == GameState.MissionSuccess)
            {
                StopSpawning();
            }
        }

        /// <summary>
        /// Immediately halts spawning. Cancels the active coroutine.
        /// </summary>
        private void StopSpawning()
        {
            if (isSpawning)
            {
                isSpawning = false;
                if (spawnCoroutine != null)
                {
                    StopCoroutine(spawnCoroutine);
                    spawnCoroutine = null;
                }
                Debug.Log("[Mission01Controller] Spawn sequence HALTED — mission ended.");
            }
        }

        private IEnumerator SpawnSequence()
        {
            isSpawning = true;
            yield return new WaitForSeconds(initialSpawnDelay);

            while (enemiesSpawned < totalEnemiesToSpawn && isSpawning)
            {
                // Check game state before spawning
                if (GameManager.Instance != null && !GameManager.Instance.IsMissionActive)
                {
                    StopSpawning();
                    yield break;
                }

                int currentEnemies = FindObjectsOfType<EnemyBase>().Length;
                if (currentEnemies < maxSimultaneousEnemies)
                {
                    SpawnNextEnemy();
                }
                yield return new WaitForSeconds(waveInterval);
            }

            isSpawning = false;
            spawnCoroutine = null;
        }

        private void SpawnNextEnemy()
        {
            Vector3 spawnPos = Vector3.zero;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            }
            else
            {
                // Fallback radius
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                spawnPos = new Vector3(Mathf.Cos(angle) * 15f, 0.5f, Mathf.Sin(angle) * 15f);
            }

            GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyObj.transform.position = spawnPos;

            // Graduated onboarding threat progression
            int threatType;
            if (enemiesSpawned == 0 || enemiesSpawned == 1)
            {
                // Wave 1 & 2: Introductory Malware Worms
                threatType = 0;
            }
            else if (enemiesSpawned == 2)
            {
                // Wave 3: Introductory Trojan Infiltrator (demonstrates node defense)
                threatType = 2;
            }
            else if (enemiesSpawned == 3)
            {
                // Wave 4: Introductory DDoS Flooder (demonstrates ranged defense & firewall)
                threatType = 1;
            }
            else
            {
                // Waves 5+: Dynamic escalating mix
                threatType = enemiesSpawned % 3;
            }

            if (threatType == 0)
            {
                enemyObj.name = $"MalwareWorm_{enemiesSpawned}";
                enemyObj.transform.localScale = new Vector3(0.8f, 0.8f, 1.4f);
                var rend = enemyObj.GetComponent<Renderer>();
                if (rend != null) rend.material.color = Color.magenta;
                enemyObj.AddComponent<EnemyMalwareWorm>();
            }
            else if (threatType == 1)
            {
                enemyObj.name = $"DDoSFlooder_{enemiesSpawned}";
                enemyObj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                var rend = enemyObj.GetComponent<Renderer>();
                if (rend != null) rend.material.color = Color.red;
                enemyObj.AddComponent<EnemyDDoSFlooder>();
            }
            else
            {
                enemyObj.name = $"TrojanInfiltrator_{enemiesSpawned}";
                enemyObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                var rend = enemyObj.GetComponent<Renderer>();
                if (rend != null) rend.material.color = new Color(0.54f, 0.36f, 1.0f); // Cyber Violet
                enemyObj.AddComponent<EnemyTrojanInfiltrator>();
            }

            // Ensure collider is active
            var boxCol = enemyObj.GetComponent<BoxCollider>();
            if (boxCol != null) boxCol.isTrigger = false;

            var rb = enemyObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            enemiesSpawned++;
            Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [Mission01Controller] Spawned threat entity #{enemiesSpawned}: {enemyObj.name} at {spawnPos}");
        }
    }
}
