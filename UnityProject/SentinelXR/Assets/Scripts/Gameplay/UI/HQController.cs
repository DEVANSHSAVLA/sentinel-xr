using UnityEngine;

namespace SentinelXR.Gameplay.UI
{
    public class HQController : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.InHQ);
            }
        }

        private void OnGUI()
        {
            int screenW = Screen.width;
            int screenH = Screen.height;

            // Background
            GUI.color = new Color(0.01f, 0.03f, 0.06f, 0.95f);
            GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

            // Header
            GUI.color = Color.cyan;
            GUI.Label(new Rect(40, 30, 500, 35), "<size=20><b>AEGIS CYBERNETICS // TACTICAL HQ COMMAND</b></size>");
            GUI.color = Color.gray;
            GUI.Label(new Rect(40, 65, 500, 20), "OPERATOR OPERATIONS & THREAT RESPONSE CENTER");

            // Left Panel: Operator Dossier
            GUI.color = new Color(0.03f, 0.08f, 0.15f, 0.9f);
            GUI.Box(new Rect(40, 100, 340, 360), "OPERATOR PROFILE");

            GUI.color = Color.white;
            int credits = GameManager.Instance != null ? GameManager.Instance.PlayerCredits : 1500;
            int lvl = GameManager.Instance != null ? GameManager.Instance.SentinelLevel : 1;
            int missions = GameManager.Instance != null ? GameManager.Instance.MissionsCompleted : 0;

            GUI.Label(new Rect(60, 140, 300, 25), $"OPERATOR CALLSIGN: Sentinel-01");
            GUI.Label(new Rect(60, 170, 300, 25), $"SECURITY CLEARANCE: LEVEL {lvl}");
            GUI.Label(new Rect(60, 200, 300, 25), $"BOUNTY CREDITS: {credits} CR");
            GUI.Label(new Rect(60, 230, 300, 25), $"MISSIONS SECURED: {missions}");

            GUI.color = Color.cyan;
            GUI.Label(new Rect(60, 275, 300, 25), "ACTIVE CYBER LOADOUT:");
            GUI.color = Color.gray;
            GUI.Label(new Rect(60, 305, 300, 20), "• [Slot 1] High-Frequency Trace Scanner");
            GUI.Label(new Rect(60, 325, 300, 20), "• [Slot 2] Quantum Dynamic Patch Tool");
            GUI.Label(new Rect(60, 345, 300, 20), "• [Slot 3] Subnet Quarantine EMP Pulse");
            GUI.Label(new Rect(60, 365, 300, 20), "• [Slot 4] Stateful Directional Firewall Shield");

            // Right Panel: Mission Operations
            GUI.color = new Color(0.03f, 0.08f, 0.15f, 0.9f);
            GUI.Box(new Rect(420, 100, 500, 360), "MISSION TERMINAL // ACTIVE CONTRACTS");

            GUI.color = Color.yellow;
            GUI.Label(new Rect(440, 140, 460, 25), "MISSION 01: SIGNAL IN THE NOISE [ACTIVE THREAT]");
            GUI.color = Color.white;
            GUI.Label(new Rect(440, 170, 460, 80),
                "A catastrophic volumetric breach has compromised DMZ Gateway Alpha.\n" +
                "Malware Bit-Worms and DDoS Flooders are overwhelming edge nodes.\n" +
                "Neutralize hostile infiltration, patch compromised nodes, and defeat Breach Core Alpha.");

            GUI.color = Color.cyan;
            if (GUI.Button(new Rect(440, 280, 460, 50), "▶ DEPLOY TO MISSION 01 (SIGNAL IN THE NOISE)"))
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MISSION_01);
                }
            }

            GUI.color = Color.white;
            if (GUI.Button(new Rect(440, 350, 460, 40), "RETURN TO MAIN MENU"))
            {
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
                }
            }
        }
    }
}
