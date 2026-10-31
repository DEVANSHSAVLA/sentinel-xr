using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SentinelXR.Gameplay
{
    public enum GameState
    {
        Bootstrapping,
        MainMenu,
        InHQ,
        InMission,
        Paused,
        MissionSuccess,
        MissionFailed
    }

    /// <summary>
    /// Central game manager maintaining global state, player stats, audio settings,
    /// and offline/online single-player mission progression.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Bootstrapping;
        public GameState CurrentState => currentState;

        [Header("Player Persistence")]
        public int PlayerCredits = 1500;
        public int SentinelLevel = 1;
        public int MissionsCompleted = 0;
        public float MasterVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public bool IsOfflineMode = true;

        [Header("Mission Debrief Data")]
        public int LastThreatsEliminated = 0;
        public float LastSubnetIntegrityPreserved = 100f;
        public float LastMissionTimeElapsed = 0f;
        public string LastForensicSha256 = "";

        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            InitializeSingleton();
        }

        public void InitializeSingleton()
        {
            if (Instance == null || Instance == this)
            {
                Instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                InitializeGame();
            }
            else if (Instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeGame()
        {
            Debug.Log("[GameManager] Initializing SENTINEL-XR: BREACH PROTOCOL engine...");
            Application.targetFrameRate = 60;
            Time.timeScale = 1.0f;
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] Game state changed to: {currentState}");

            if (currentState == GameState.Paused)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (currentState == GameState.InMission)
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnGameStateChanged?.Invoke(currentState);
        }

        public void RecordMissionDebrief(int threatsEliminated, float integrityPreserved, float timeElapsed)
        {
            LastThreatsEliminated = threatsEliminated;
            LastSubnetIntegrityPreserved = Mathf.Clamp(integrityPreserved, 0f, 100f);
            LastMissionTimeElapsed = timeElapsed;
            MissionsCompleted++;
            PlayerCredits += 500 + Mathf.RoundToInt(integrityPreserved * 5);

            // Compute deterministic SHA-256 evidence seal for mission debrief
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                string payload = $"MISSION-01|{threatsEliminated}|{integrityPreserved:F1}|{timeElapsed:F1}|{DateTime.UtcNow:s}";
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                LastForensicSha256 = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            SentinelAudioSystem.Instance?.PlayMissionSuccess();
            SetState(GameState.MissionSuccess);
        }

        /// <summary>
        /// Idempotent game-over trigger. Only transitions to MissionFailed once.
        /// Subsequent calls are safely ignored.
        /// </summary>
        public void TriggerGameOver()
        {
            if (currentState == GameState.MissionFailed)
            {
                return; // Already failed — prevent duplicate transitions
            }

            Debug.Log("[GameManager] TriggerGameOver invoked. Transitioning to MissionFailed.");
            SentinelAudioSystem.Instance?.PlayMissionFailed();
            SetState(GameState.MissionFailed);
        }

        /// <summary>
        /// Returns true when the game is actively in a mission (combat/spawning allowed).
        /// </summary>
        public bool IsMissionActive => currentState == GameState.InMission;
    }
}
