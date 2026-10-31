using System.Collections;
using UnityEngine;

namespace SentinelXR.Gameplay
{
    public class BootstrapController : MonoBehaviour
    {
        [Header("Bootstrap Timing")]
        [SerializeField] private float splashDuration = 1.5f;

        private IEnumerator Start()
        {
            Debug.Log("[BootstrapController] Starting SENTINEL-XR 2.0 system bootloader...");

            // Ensure singletons are alive
            if (GameManager.Instance == null)
            {
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }

            if (SceneTransitionManager.Instance == null)
            {
                GameObject stm = new GameObject("SceneTransitionManager");
                stm.AddComponent<SceneTransitionManager>();
            }

            if (SentinelAudioSystem.Instance == null)
            {
                GameObject audioSys = new GameObject("SentinelAudioSystem");
                audioSys.AddComponent<SentinelAudioSystem>();
            }

            // Yield for splash duration
            yield return new WaitForSeconds(splashDuration);

            // Automatically transition to MainMenuScene
            Debug.Log("[BootstrapController] Transitioning to MainMenuScene...");
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(SceneTransitionManager.SCENE_MAIN_MENU);
            }
        }

        private void OnGUI()
        {
            int screenW = Screen.width;
            int screenH = Screen.height;

            GUI.color = new Color(0.01f, 0.02f, 0.05f, 1.0f);
            GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

            GUI.color = Color.cyan;
            GUI.Label(new Rect(screenW * 0.5f - 200, screenH * 0.45f, 400, 30),
                "<size=22><b>SENTINEL-XR // BREACH PROTOCOL</b></size>");

            GUI.color = Color.gray;
            GUI.Label(new Rect(screenW * 0.5f - 150, screenH * 0.52f, 300, 20),
                "INITIALIZING NEURAL DEFENSE KERNEL...");
        }
    }
}
