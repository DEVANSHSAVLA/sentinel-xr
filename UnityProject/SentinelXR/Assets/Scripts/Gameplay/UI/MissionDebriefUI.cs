using UnityEngine;

namespace SentinelXR.Gameplay.UI
{
    public class MissionDebriefUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.MissionSuccess)
            {
                return;
            }

            int screenW = Screen.width;
            int screenH = Screen.height;

            // Fullscreen backdrop
            GUI.color = new Color(0.01f, 0.04f, 0.08f, 0.94f);
            GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

            // Debrief Card
            GUI.color = Color.cyan;
            GUI.Box(new Rect(screenW * 0.5f - 260, screenH * 0.5f - 210, 520, 420), "MISSION ACCOMPLISHED // FORENSIC DEBRIEF");

            GUI.color = Color.white;
            int y = (int)(screenH * 0.5f - 160);

            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 25), "SECTOR: DMZ Gateway Subnet Alpha");
            y += 28;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 25), $"THREATS NEUTRALIZED: {GameManager.Instance.LastThreatsEliminated} Entities");
            y += 28;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 25), $"SUBNET INTEGRITY PRESERVED: {GameManager.Instance.LastSubnetIntegrityPreserved:F1}%");
            y += 28;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 25), $"MISSION ELAPSED TIME: {GameManager.Instance.LastMissionTimeElapsed:F1} seconds");
            y += 28;
            GUI.color = Color.green;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 25), $"BOUNTY REWARD: +750 CREDITS (Balance: {GameManager.Instance.PlayerCredits})");
            y += 35;

            // Forensic SHA-256 seal
            GUI.color = Color.yellow;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 20), "CRYPTOGRAPHIC EVIDENCE SEAL [SHA-256]:");
            y += 22;
            GUI.color = Color.gray;
            GUI.Label(new Rect(screenW * 0.5f - 220, y, 440, 35), GameManager.Instance.LastForensicSha256);
            y += 45;

            // Action Buttons
            GUI.color = Color.cyan;
            if (GUI.Button(new Rect(screenW * 0.5f - 200, y, 400, 45), "CONTINUE TO TACTICAL HQ"))
            {
                SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_HQ);
            }
            y += 55;
            if (GUI.Button(new Rect(screenW * 0.5f - 200, y, 400, 35), "RETURN TO MAIN MENU"))
            {
                SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
            }
        }
    }
}
