using System;
using UnityEngine;

namespace SentinelXR.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class SentinelPlayerController : MonoBehaviour
    {
        public static SentinelPlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float sprintSpeed = 10.0f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -19.62f;

        [Header("Look Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float lookXLimit = 85.0f;

        [Header("Combat & Integrity")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float currentHealth = 100.0f;
        [SerializeField] private float maxShield = 100.0f;
        [SerializeField] private float currentShield = 100.0f;
        [SerializeField] private float shieldRegenDelay = 4.0f;
        [SerializeField] private float shieldRegenRate = 20.0f;

        [Header("Spawn Protection & Onboarding")]
        [SerializeField] private float spawnProtectionDuration = 8.0f;
        [SerializeField] private bool qaInvulnerability = false;

        private CharacterController characterController;
        private Vector3 velocity;
        private float rotationX = 0f;
        private bool isGrounded;
        private float lastDamageTime = -999f;
        private bool isDead = false;
        private float spawnProtectionEndTime = 0f;
        private bool wasProtectionActive = true;

        // Feedback Events for UI & Audio
        public event Action<float, float> OnDamageTaken; // (healthDmg, shieldDmg)
        public event Action OnShieldBroken;
        public event Action OnLowHealthWarning;
        public event Action OnSpawnProtectionExpired;

        /// <summary>
        /// True after Die() has been invoked. Blocks further damage and death processing.
        /// </summary>
        public bool IsDead => isDead;

        public float Health => currentHealth;
        public float MaxHealth => maxHealth;
        public float Shield => currentShield;
        public float MaxShield => maxShield;
        public Camera PlayerCamera => playerCamera;

        public float SpawnProtectionDuration
        {
            get => spawnProtectionDuration;
            set => spawnProtectionDuration = value;
        }

        public float SpawnProtectionTimeRemaining => Mathf.Max(0f, spawnProtectionEndTime - Time.time);

        public bool IsSpawnProtected
        {
            get
            {
                if (qaInvulnerability && IsQAInvulnerabilityPermitted) return true;
                return Time.time < spawnProtectionEndTime;
            }
        }

        public bool QAInvulnerability
        {
            get => qaInvulnerability;
            set => qaInvulnerability = value;
        }

        public bool IsQAInvulnerabilityPermitted
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public float LastDamageTime => lastDamageTime;

        private void Awake()
        {
            InitializeSingleton();
        }

        public void InitializeSingleton()
        {
            Instance = this;
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            InitializeMission();
        }

        public void InitializeMission(float protectionOverride = -1f)
        {
            InitializeSingleton();
            currentHealth = maxHealth;
            currentShield = maxShield;
            isDead = false;
            lastDamageTime = -999f;
            if (protectionOverride >= 0f)
            {
                spawnProtectionDuration = protectionOverride;
            }
            spawnProtectionEndTime = Time.time + spawnProtectionDuration;
            wasProtectionActive = spawnProtectionDuration > 0f;

            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Player initialized. HP: {currentHealth:F0}/{maxHealth:F0}, SHD: {currentShield:F0}/{maxShield:F0}. Spawn protection active for {spawnProtectionDuration:F1}s (expires at {spawnProtectionEndTime:F2}s). QA Invuln: {qaInvulnerability && IsQAInvulnerabilityPermitted}");
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.InMission)
            {
                return;
            }

            // Monitor spawn protection expiration
            if (wasProtectionActive && !IsSpawnProtected)
            {
                wasProtectionActive = false;
                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Spawn protection EXPIRED. Combat vulnerability engaged.");
                OnSpawnProtectionExpired?.Invoke();
            }

            HandleMovement();
            HandleMouseLook();
            HandleShieldRegeneration();
        }

        private void HandleMovement()
        {
            if (characterController == null) return;

            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? sprintSpeed : walkSpeed;

            Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
            characterController.Move(moveDirection * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }

            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleShieldRegeneration()
        {
            if (Time.time - lastDamageTime >= shieldRegenDelay && currentShield < maxShield && !isDead)
            {
                currentShield = Mathf.MoveTowards(currentShield, maxShield, shieldRegenRate * Time.deltaTime);
            }
        }

        public void TakeDamage(float amount)
        {
            // Block damage if player is already dead
            if (isDead)
            {
                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Attack rejected: Player is already dead.");
                return;
            }

            // Block damage if mission is not active
            if (GameManager.Instance != null && !GameManager.Instance.IsMissionActive)
            {
                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Attack rejected: Mission is not active ({GameManager.Instance.CurrentState}).");
                return;
            }

            // Block damage if spawn protection or QA invulnerability is active
            if (IsSpawnProtected)
            {
                Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Attack rejected: Player under spawn protection / QA invulnerability ({SpawnProtectionTimeRemaining:F2}s remaining).");
                return;
            }

            if (amount <= 0f) return;

            lastDamageTime = Time.time;
            float shieldAbsorbed = 0f;
            float healthDamage = 0f;

            if (currentShield > 0f)
            {
                if (currentShield >= amount)
                {
                    shieldAbsorbed = amount;
                    currentShield -= amount;
                    if (currentShield == 0f)
                    {
                        OnShieldBroken?.Invoke();
                    }
                }
                else
                {
                    shieldAbsorbed = currentShield;
                    healthDamage = amount - currentShield;
                    currentShield = 0f;
                    OnShieldBroken?.Invoke();
                    currentHealth -= healthDamage;
                }
            }
            else
            {
                healthDamage = amount;
                currentHealth -= healthDamage;
            }

            // Clamp health to valid range [0, maxHealth]
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Attack accepted! Incurred: {amount:F1} (Shield absorbed: {shieldAbsorbed:F1}, Health damaged: {healthDamage:F1}) => Status: HP {currentHealth:F0}/{maxHealth:F0}, SHD {currentShield:F0}/{maxShield:F0}");

            OnDamageTaken?.Invoke(healthDamage, shieldAbsorbed);

            if (healthDamage > 0f)
            {
                SentinelAudioSystem.Instance?.PlayPlayerDamage();
            }
            else if (shieldAbsorbed > 0f)
            {
                SentinelAudioSystem.Instance?.PlayShieldHit();
            }

            if (currentHealth > 0f && currentHealth <= maxHealth * 0.25f)
            {
                OnLowHealthWarning?.Invoke();
            }

            if (currentHealth <= 0f && !isDead)
            {
                Die();
            }
        }

        public void SetSpawnProtection(float duration)
        {
            spawnProtectionDuration = duration;
            spawnProtectionEndTime = Time.time + duration;
            wasProtectionActive = duration > 0f;
            Debug.Log($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Spawn protection manually set to {duration:F1}s (expires at {spawnProtectionEndTime:F2}s).");
        }

        private void Die()
        {
            if (isDead) return; // Guard: Die() executes exactly once
            isDead = true;

            Debug.LogWarning($"[COMBAT-DIAG] [{Time.time:F2}s] [SentinelPlayer] Operator Integrity Depleted! System Breach Critical.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}
