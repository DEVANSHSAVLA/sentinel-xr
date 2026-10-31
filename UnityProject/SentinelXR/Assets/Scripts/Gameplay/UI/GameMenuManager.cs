using UnityEngine;
using UnityEngine.SceneManagement;

namespace SentinelXR.Gameplay.UI
{
    public class GameMenuManager : MonoBehaviour
    {
        private bool isPaused = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.CurrentState == GameState.InMission)
                    {
                        PauseGame();
                    }
                    else if (GameManager.Instance.CurrentState == GameState.Paused)
                    {
                        ResumeGame();
                    }
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.InMission);
            }
        }

        private void OnGUI()
        {
            if (GameManager.Instance == null) return;

            int screenW = Screen.width;
            int screenH = Screen.height;

            // 1. Pause Menu
            if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GUI.color = new Color(0.01f, 0.02f, 0.05f, 0.92f);
                GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

                GUI.color = Color.cyan;
                GUI.Box(new Rect(screenW * 0.5f - 180, screenH * 0.5f - 180, 360, 340), "TACTICAL PAUSE // SENTINEL-XR");

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f - 120, 280, 40), "RESUME MISSION"))
                {
                    ResumeGame();
                }

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f - 65, 280, 40), "RESTART MISSION 01"))
                {
                    Time.timeScale = 1f;
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MISSION_01);
                }

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f - 10, 280, 40), "ABORT TO TACTICAL HQ"))
                {
                    Time.timeScale = 1f;
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_HQ);
                }

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f + 45, 280, 40), "RETURN TO MAIN MENU"))
                {
                    Time.timeScale = 1f;
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
                }
            }

            // 2. Game Over Screen
            if (GameManager.Instance.CurrentState == GameState.MissionFailed)
            {
                GUI.color = new Color(0.15f, 0.02f, 0.02f, 0.95f);
                GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

                GUI.color = Color.red;
                GUI.Box(new Rect(screenW * 0.5f - 220, screenH * 0.5f - 150, 440, 280), "SYSTEM BREACH // MISSION FAILED");

                GUI.color = Color.white;
                GUI.Label(new Rect(screenW * 0.5f - 190, screenH * 0.5f - 100, 380, 50),
                    "Subnet integrity collapsed or operator system integrity depleted.\nThe breach entity compromised core network services.");

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f - 30, 280, 45), "RETRY MISSION 01"))
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MISSION_01);
                }

                if (GUI.Button(new Rect(screenW * 0.5f - 140, screenH * 0.5f + 30, 280, 45), "RETURN TO MAIN MENU"))
                {
                    SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
                }
            }
        }
    }
}
