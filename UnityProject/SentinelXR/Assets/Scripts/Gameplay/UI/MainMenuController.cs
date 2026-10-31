using UnityEngine;

namespace SentinelXR.Gameplay.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }
        }

        private void OnGUI()
        {
            int screenW = Screen.width;
            int screenH = Screen.height;

            // Background cyber styling
            GUI.color = new Color(0.01f, 0.03f, 0.07f, 0.95f);
            GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

            // Title Banner
            GUI.color = Color.cyan;
            GUI.Label(new Rect(screenW * 0.5f - 250, screenH * 0.12f, 500, 40), "<size=24><b>SENTINEL-XR // BREACH PROTOCOL</b></size>");
            GUI.color = Color.gray;
            GUI.Label(new Rect(screenW * 0.5f - 250, screenH * 0.12f + 35, 500, 25), "NATIVE UNITY 6 SPATIAL CYBER DEFENSE OPERATIONAL PLATFORM");

            // Main Menu Box
            GUI.color = new Color(0.03f, 0.07f, 0.14f, 0.9f);
            int boxW = 420;
            int boxH = 380;
            int boxX = (screenW - boxW) / 2;
            int boxY = (int)(screenH * 0.28f);
            GUI.Box(new Rect(boxX, boxY, boxW, boxH), "");

            int btnW = 340;
            int btnH = 45;
            int btnX = boxX + (boxW - btnW) / 2;
            int curY = boxY + 25;

            // 1. Mission 01 Playable Level
            GUI.color = Color.cyan;
            if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "▶ LAUNCH MISSION 01: SIGNAL IN THE NOISE"))
            {
                SentinelAudioSystem.Instance?.PlayUiClick();
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MISSION_01);
                }
            }
            curY += 55;

            // 2. Tactical HQ
            GUI.color = Color.white;
            if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "TACTICAL HQ & LOADOUT"))
            {
                SentinelAudioSystem.Instance?.PlayUiClick();
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_HQ);
                }
            }
            curY += 55;

            // 3. VR Command Center
            if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "VR COMMAND CENTER [EXPERIMENTAL]"))
            {
                SentinelAudioSystem.Instance?.PlayUiClick();
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_VR_COMMAND);
                }
            }
            curY += 55;

            // 4. AR Holo-Simulation
            if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "AR HOLO-SIMULATION [EXPERIMENTAL]"))
            {
                SentinelAudioSystem.Instance?.PlayUiClick();
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_AR_TRAINING);
                }
            }
            curY += 55;

            // 5. Quit Game
            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "QUIT PROTOCOL"))
            {
                SentinelAudioSystem.Instance?.PlayUiClick();
                Debug.Log("[MainMenu] Exiting application...");
                Application.Quit();
            }

            // Version info footer
            GUI.color = Color.gray;
            GUI.Label(new Rect(20, screenH - 30, 400, 20), "SENTINEL-XR v2.0.0-rc2 | Unity 6000.0.23f1 URP | Aegis Cybernetics");
        }
    }
}
