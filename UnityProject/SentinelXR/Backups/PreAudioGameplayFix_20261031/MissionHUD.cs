using System;
using System.Collections.Generic;
using UnityEngine;
using SentinelXR.Gameplay.Player;
using SentinelXR.Gameplay.Missions;
using SentinelXR.Gameplay.Enemies;

namespace SentinelXR.Gameplay.UI
{
    /// <summary>
    /// Premium OBSIDIAN NEURAL cyber-defense heads-up display.
    /// Provides real-time telemetry, operator health & shields, defensive tool energy,
    /// dynamic 4-stage mission objectives, boss health presentation, and combat feedback.
    /// </summary>
    public class MissionHUD : MonoBehaviour
    {
        private SentinelPlayerController player;
        private ToolInventory inventory;
        private PlayerInteraction interaction;
        private MissionObjectiveManager objectives;
        private Tools.TraceScanner scanner;
        private BreachCoreBoss breachCore;

        // Feedback & Toast Notifications
        private float hitFlashTimer = 0f;
        private Color hitFlashColor = Color.clear;
        private const float HIT_FLASH_DURATION = 0.35f;

        private string toastMessage = "";
        private float toastTimer = 0f;
        private Color toastColor = Color.cyan;

        // Palette Constants
        private static readonly Color ColorBgGlass = new Color(0.027f, 0.043f, 0.086f, 0.88f);  // #070B16
        private static readonly Color ColorBorderCyan = new Color(0.000f, 0.851f, 1.000f, 0.90f); // #00D9FF
        private static readonly Color ColorElectricBlue = new Color(0.278f, 0.482f, 1.000f, 1f);  // #477BFF
        private static readonly Color ColorGreen = new Color(0.145f, 0.902f, 0.647f, 1f);        // #25E6A5
        private static readonly Color ColorCriticalRed = new Color(1.000f, 0.212f, 0.373f, 1f);  // #FF365F
        private static readonly Color ColorWarningAmber = new Color(1.000f, 0.650f, 0.100f, 1f);

        // GUI Styles
        private static GUIStyle styleTitle;
        private static GUIStyle styleSubtitle;
        private static GUIStyle styleLabel;
        private static GUIStyle styleWarning;
        private static GUIStyle styleObjectiveActive;
        private static GUIStyle styleObjectiveDone;
        private static GUIStyle styleCrosshair;
        private static Texture2D texWhite;

        private void Start()
        {
            AcquireReferences();
            BindEvents();
        }

        private void OnEnable()
        {
            AcquireReferences();
            BindEvents();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void AcquireReferences()
        {
            if (player == null) player = SentinelPlayerController.Instance;
            if (inventory == null) inventory = ToolInventory.Instance ?? FindObjectOfType<ToolInventory>();
            if (interaction == null) interaction = FindObjectOfType<PlayerInteraction>();
            if (objectives == null) objectives = MissionObjectiveManager.Instance;
            if (scanner == null) scanner = FindObjectOfType<Tools.TraceScanner>();
            if (breachCore == null) breachCore = FindObjectOfType<BreachCoreBoss>();
        }

        private void BindEvents()
        {
            if (player != null)
            {
                player.OnDamageTaken -= HandleDamageTaken;
                player.OnDamageTaken += HandleDamageTaken;
            }

            if (objectives != null)
            {
                objectives.OnObjectiveCompleted -= HandleObjectiveCompleted;
                objectives.OnObjectiveCompleted += HandleObjectiveCompleted;
            }

            if (inventory != null)
            {
                inventory.OnToolSelected -= HandleToolSelected;
                inventory.OnToolSelected += HandleToolSelected;
            }
        }

        private void UnbindEvents()
        {
            if (player != null)
            {
                player.OnDamageTaken -= HandleDamageTaken;
            }
            if (objectives != null)
            {
                objectives.OnObjectiveCompleted -= HandleObjectiveCompleted;
            }
            if (inventory != null)
            {
                inventory.OnToolSelected -= HandleToolSelected;
            }
        }

        private void HandleDamageTaken(float healthDmg, float shieldDmg)
        {
            hitFlashTimer = HIT_FLASH_DURATION;
            if (healthDmg > 0f)
            {
                hitFlashColor = new Color(1f, 0.15f, 0.25f, 0.50f);
            }
            else
            {
                hitFlashColor = new Color(0f, 0.85f, 1f, 0.40f);
            }
        }

        private void HandleObjectiveCompleted(MissionObjective obj)
        {
            ShowToast($"OBJECTIVE ACCOMPLISHED: {obj.title.ToUpper()}", ColorGreen, 4.0f);
        }

        private void HandleToolSelected(CyberToolType tool)
        {
            ShowToast($"TOOL ENGAGED: [{tool.ToString().ToUpper()}]", ColorBorderCyan, 2.0f);
        }

        public void ShowToast(string message, Color col, float duration = 3.0f)
        {
            toastMessage = message;
            toastColor = col;
            toastTimer = duration;
        }

        private void Update()
        {
            if (player == null || inventory == null || breachCore == null)
            {
                AcquireReferences();
                BindEvents();
            }

            if (hitFlashTimer > 0f) hitFlashTimer -= Time.deltaTime;
            if (toastTimer > 0f) toastTimer -= Time.deltaTime;
        }

        private void InitStyles()
        {
            if (texWhite == null)
            {
                texWhite = new Texture2D(1, 1);
                texWhite.SetPixel(0, 0, Color.white);
                texWhite.Apply();
            }

            if (styleTitle == null)
            {
                styleTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                styleTitle.normal.textColor = ColorBorderCyan;

                styleSubtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };
                styleSubtitle.normal.textColor = new Color(0.75f, 0.85f, 1f, 0.9f);

                styleLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };
                styleLabel.normal.textColor = Color.white;

                styleWarning = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                styleWarning.normal.textColor = ColorCriticalRed;

                styleObjectiveActive = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                styleObjectiveActive.normal.textColor = ColorBorderCyan;

                styleObjectiveDone = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };
                styleObjectiveDone.normal.textColor = ColorGreen;

                styleCrosshair = new GUIStyle(GUI.skin.box);
            }
        }

        private void OnGUI()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.InMission)
            {
                return;
            }

            InitStyles();

            int screenW = Screen.width;
            int screenH = Screen.height;

            // 0. Screen-Space Damage & Shield Impact Border Flash
            DrawScreenDamageFlash(screenW, screenH);

            // 1. TOP-LEFT: Identity, Mission Name & Timer
            DrawTopLeftPanel();

            // 2. TOP-RIGHT: Wave Status, Active Threats & Subnet Integrity
            DrawTopRightPanel(screenW);

            // 3. BOTTOM-LEFT: Health, Shield & Spawn Protection
            DrawBottomLeftPanel(screenH);

            // 4. BOTTOM-RIGHT: Tool Card, Energy & Shortcut Dock
            DrawBottomRightPanel(screenW, screenH);

            // 5. CENTER: Reticle, Enemy Targeting, Interaction Prompt
            DrawCenterReticle(screenW, screenH);

            // 6. TOP-CENTER: Boss Health Bar (when Boss Active) or Notification Toast
            DrawTopCenterBossAndToasts(screenW);
        }

        private void DrawScreenDamageFlash(int w, int h)
        {
            if (hitFlashTimer > 0f)
            {
                float alpha = (hitFlashTimer / HIT_FLASH_DURATION);
                Color c = new Color(hitFlashColor.r, hitFlashColor.g, hitFlashColor.b, hitFlashColor.a * alpha);
                GUI.color = c;
                int thick = 20;
                GUI.DrawTexture(new Rect(0, 0, w, thick), texWhite);
                GUI.DrawTexture(new Rect(0, h - thick, w, thick), texWhite);
                GUI.DrawTexture(new Rect(0, 0, thick, h), texWhite);
                GUI.DrawTexture(new Rect(w - thick, 0, thick, h), texWhite);
            }
        }

        private void DrawGlassBox(Rect rect, Color borderColor)
        {
            // Background fill
            GUI.color = ColorBgGlass;
            GUI.DrawTexture(rect, texWhite);

            // Border outline
            GUI.color = borderColor;
            int b = 1;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, b), texWhite);
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - b, rect.width, b), texWhite);
            GUI.DrawTexture(new Rect(rect.x, rect.y, b, rect.height), texWhite);
            GUI.DrawTexture(new Rect(rect.x + rect.width - b, rect.y, b, rect.height), texWhite);
        }

        private void DrawBar(Rect rect, float pct, Color fillColor, Color bgColor)
        {
            pct = Mathf.Clamp01(pct);
            GUI.color = bgColor;
            GUI.DrawTexture(rect, texWhite);
            if (pct > 0f)
            {
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * pct, rect.height), texWhite);
            }
        }

        private void DrawTopLeftPanel()
        {
            Rect r = new Rect(20, 20, 310, 80);
            DrawGlassBox(r, ColorBorderCyan);

            GUI.Label(new Rect(32, 26, 280, 18), "SENTINEL-XR // BREACH PROTOCOL", styleTitle);
            GUI.Label(new Rect(32, 44, 280, 16), "SECTOR: MISSION 01 — SIGNAL IN THE NOISE", styleSubtitle);

            float missionTime = objectives != null ? objectives.MissionTime : 0f;
            int mins = (int)(missionTime / 60f);
            float secs = missionTime % 60f;
            string timeStr = $"SYS-TIME: {mins:00}:{secs:04.1f} | STAT: ACTIVE";
            GUI.Label(new Rect(32, 62, 280, 18), timeStr, styleSubtitle);
        }

        private void DrawTopRightPanel(int screenW)
        {
            Rect r = new Rect(screenW - 390, 20, 370, 165);
            DrawGlassBox(r, ColorBorderCyan);

            // Header
            GUI.Label(new Rect(screenW - 378, 26, 350, 18), "NETWORK TELEMETRY & OBJECTIVES", styleTitle);

            // Subnet Integrity
            float subnet = objectives != null ? Mathf.Clamp01(objectives.SubnetIntegrity / 100f) : 1f;
            Color netCol = subnet > 0.5f ? ColorBorderCyan : (subnet > 0.25f ? ColorWarningAmber : ColorCriticalRed);
            GUI.Label(new Rect(screenW - 378, 46, 200, 16), $"SUBNET INTEGRITY: {Mathf.RoundToInt(subnet * 100)}%", styleSubtitle);
            DrawBar(new Rect(screenW - 378, 64, 346, 8), subnet, netCol, new Color(0.1f, 0.15f, 0.25f, 0.8f));

            // Threat count & Wave
            int enemiesAlive = FindObjectsOfType<EnemyBase>().Length;
            int threatsDefeated = objectives != null ? objectives.ThreatsEliminated : 0;
            string threatStr = $"ACTIVE HOSTILES: {enemiesAlive} | PURGED: {threatsDefeated}";
            GUI.Label(new Rect(screenW - 378, 76, 350, 16), threatStr, styleSubtitle);

            // Objective list
            if (objectives != null && objectives.Objectives != null)
            {
                int y = 96;
                for (int i = 0; i < objectives.Objectives.Count; i++)
                {
                    var obj = objectives.Objectives[i];
                    string prefix = obj.isCompleted ? "[✓] " : $"[{i + 1}] ";
                    GUIStyle s = obj.isCompleted ? styleObjectiveDone : styleObjectiveActive;
                    GUI.Label(new Rect(screenW - 378, y, 350, 16), $"{prefix}{obj.title}", s);
                    y += 16;
                }
            }
        }

        private void DrawBottomLeftPanel(int screenH)
        {
            Rect r = new Rect(20, screenH - 145, 330, 125);
            DrawGlassBox(r, ColorBorderCyan);

            float hp = player != null ? player.Health : 100f;
            float maxHp = player != null ? player.MaxHealth : 100f;
            float hpPct = Mathf.Clamp01(hp / maxHp);

            float shd = player != null ? player.Shield : 100f;
            float maxShd = player != null ? player.MaxShield : 100f;
            float shdPct = Mathf.Clamp01(shd / maxShd);

            // Health
            Color hpCol = hpPct > 0.35f ? ColorGreen : ColorCriticalRed;
            GUI.Label(new Rect(32, screenH - 138, 200, 16), $"OPERATOR INTEGRITY: {Mathf.RoundToInt(hp)}/{Mathf.RoundToInt(maxHp)}", styleSubtitle);
            DrawBar(new Rect(32, screenH - 120, 260, 12), hpPct, hpCol, new Color(0.15f, 0.05f, 0.05f, 0.8f));

            // Shield
            GUI.Label(new Rect(32, screenH - 104, 200, 16), $"KINETIC SHIELD: {Mathf.RoundToInt(shd)}/{Mathf.RoundToInt(maxShd)}", styleSubtitle);
            DrawBar(new Rect(32, screenH - 86, 260, 10), shdPct, ColorBorderCyan, new Color(0.05f, 0.15f, 0.25f, 0.8f));

            // Spawn Protection or Low Health Status
            if (player != null && player.IsSpawnProtected)
            {
                float rem = player.SpawnProtectionTimeRemaining;
                GUI.color = ColorBorderCyan;
                GUI.Label(new Rect(32, screenH - 68, 290, 18), $"[SPAWN SHIELD ENGAGED: {rem:F1}s]", styleSubtitle);
            }
            else if (hpPct < 0.25f)
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                GUI.color = Color.Lerp(ColorWarningAmber, ColorCriticalRed, pulse);
                GUI.Label(new Rect(32, screenH - 68, 290, 18), "⚠ CRITICAL COMPROMISE: HP < 25%", styleSubtitle);
            }
            else
            {
                GUI.color = Color.gray;
                GUI.Label(new Rect(32, screenH - 68, 290, 18), "VULNERABILITY MATRIX: ENGAGED", styleSubtitle);
            }

            GUI.color = Color.white;
        }

        private void DrawBottomRightPanel(int screenW, int screenH)
        {
            Rect r = new Rect(screenW - 400, screenH - 145, 380, 125);
            DrawGlassBox(r, ColorBorderCyan);

            CyberToolType active = inventory != null ? inventory.ActiveTool : CyberToolType.TraceScanner;
            float energy = inventory != null ? inventory.ToolEnergy : 100f;
            float maxEnergy = inventory != null ? inventory.MaxToolEnergy : 100f;
            float energyPct = Mathf.Clamp01(energy / maxEnergy);

            // Active Tool header
            GUI.Label(new Rect(screenW - 388, screenH - 138, 260, 18), $"DEPLOYED: [{active.ToString().ToUpper()}]", styleTitle);

            // Energy Gauge
            GUI.Label(new Rect(screenW - 388, screenH - 118, 200, 16), $"TOOL POWER: {Mathf.RoundToInt(energy)}/{Mathf.RoundToInt(maxEnergy)}", styleSubtitle);
            DrawBar(new Rect(screenW - 388, screenH - 102, 356, 8), energyPct, ColorBorderCyan, new Color(0.05f, 0.15f, 0.25f, 0.8f));

            // Tool Shortcuts dock
            string[] toolKeys = { "[1] SCANNER", "[2] PATCH", "[3] PULSE", "[4] FIREWALL" };
            int btnW = 86;
            for (int i = 0; i < 4; i++)
            {
                Rect btnR = new Rect(screenW - 388 + (i * 90), screenH - 88, btnW, 24);
                bool isActive = (int)active == i;
                Color frameCol = isActive ? ColorBorderCyan : Color.gray;
                DrawGlassBox(btnR, frameCol);
                GUI.color = isActive ? ColorBorderCyan : Color.gray;
                GUI.Label(new Rect(btnR.x + 4, btnR.y + 4, btnR.width - 8, 16), toolKeys[i], styleSubtitle);
            }

            // Controls footnote
            GUI.color = Color.gray;
            GUI.Label(new Rect(screenW - 388, screenH - 58, 356, 16), "[LMB] Fire Tool | [E] Interact | [Shift] Sprint | [Space] Jump", styleSubtitle);
            GUI.color = Color.white;
        }

        private void DrawCenterReticle(int screenW, int screenH)
        {
            float cx = screenW * 0.5f;
            float cy = screenH * 0.5f;

            // Reticle brackets
            GUI.color = ColorBorderCyan;
            int reticleSize = 10;
            int tickThick = 2;

            // Dot
            GUI.DrawTexture(new Rect(cx - 1, cy - 1, 2, 2), texWhite);

            // 4 brackets
            GUI.DrawTexture(new Rect(cx - reticleSize - 4, cy - 1, reticleSize, tickThick), texWhite);
            GUI.DrawTexture(new Rect(cx + 4, cy - 1, reticleSize, tickThick), texWhite);
            GUI.DrawTexture(new Rect(cx - 1, cy - reticleSize - 4, tickThick, reticleSize), texWhite);
            GUI.DrawTexture(new Rect(cx - 1, cy + 4, tickThick, reticleSize), texWhite);

            // Interaction Prompt
            if (interaction != null && interaction.HasTarget && !string.IsNullOrEmpty(interaction.CurrentPrompt))
            {
                Rect promptR = new Rect(cx - 160, cy + 35, 320, 26);
                DrawGlassBox(promptR, ColorWarningAmber);
                GUI.color = ColorWarningAmber;
                GUI.Label(new Rect(promptR.x, promptR.y + 4, promptR.width, 18), interaction.CurrentPrompt, styleSubtitle);
            }

            // Decoded Scanner Telemetry
            if (scanner != null && !string.IsNullOrEmpty(scanner.LastDecodedTelemetry))
            {
                Rect scanR = new Rect(cx - 220, cy + 68, 440, 24);
                DrawGlassBox(scanR, ColorBorderCyan);
                GUI.color = ColorBorderCyan;
                GUI.Label(new Rect(scanR.x + 6, scanR.y + 3, scanR.width - 12, 18), scanner.LastDecodedTelemetry, styleSubtitle);
            }
        }

        private void DrawTopCenterBossAndToasts(int screenW)
        {
            float cx = screenW * 0.5f;

            // 1. Boss Health Bar (When BreachCoreBoss exists and either vulnerable or mission active)
            if (breachCore != null && breachCore.Health > 0)
            {
                Rect bossR = new Rect(cx - 240, 20, 480, 52);
                Color bossBorder = breachCore.IsShielded ? ColorWarningAmber : ColorCriticalRed;
                DrawGlassBox(bossR, bossBorder);

                string statusText = breachCore.IsShielded ? "[ANOMALY BARRIER ENGAGED // IMMUNE]" : "[SHIELD COLLAPSED // VULNERABLE]";
                GUI.Label(new Rect(bossR.x + 12, bossR.y + 6, 456, 18), $"BREACH CORE ALPHA // {statusText}", styleTitle);

                float bossPct = Mathf.Clamp01(breachCore.Health / breachCore.MaxHealth);
                Color barCol = breachCore.IsShielded ? ColorWarningAmber : ColorCriticalRed;
                DrawBar(new Rect(bossR.x + 12, bossR.y + 28, 456, 12), bossPct, barCol, new Color(0.2f, 0.05f, 0.05f, 0.8f));
            }

            // 2. Notification Toast Banner
            if (toastTimer > 0f && !string.IsNullOrEmpty(toastMessage))
            {
                float toastW = 460;
                float toastH = 32;
                float yPos = (breachCore != null && breachCore.Health > 0) ? 80 : 25;
                Rect tRect = new Rect(cx - toastW * 0.5f, yPos, toastW, toastH);
                DrawGlassBox(tRect, toastColor);
                GUI.color = toastColor;
                GUI.Label(new Rect(tRect.x + 10, tRect.y + 6, tRect.width - 20, 20), toastMessage, styleTitle);
            }
        }
    }
}
