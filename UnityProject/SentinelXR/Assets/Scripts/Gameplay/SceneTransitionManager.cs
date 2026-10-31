using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SentinelXR.Gameplay
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        public const string SCENE_BOOTSTRAP = "BootstrapScene";
        public const string SCENE_MAIN_MENU = "MainMenuScene";
        public const string SCENE_HQ = "HQScene";
        public const string SCENE_MISSION_01 = "Mission01_SignalInTheNoise";
        public const string SCENE_VR_COMMAND = "VRCommandCenter";
        public const string SCENE_AR_TRAINING = "ARTraining";

        [SerializeField] private float transitionDuration = 0.5f;
        private bool isTransitioning = false;
        private float loadProgress = 0f;

        public float LoadProgress => loadProgress;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (!isTransitioning)
            {
                StartCoroutine(LoadSceneCoroutine(sceneName));
            }
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            isTransitioning = true;
            loadProgress = 0f;

            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");

            yield return new WaitForSecondsRealtime(transitionDuration * 0.5f);

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                loadProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);

                if (asyncOp.progress >= 0.9f)
                {
                    yield return new WaitForSecondsRealtime(0.2f);
                    asyncOp.allowSceneActivation = true;
                }

                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.2f);
            isTransitioning = false;
            loadProgress = 1f;

            // Update Game State based on loaded scene
            if (GameManager.Instance != null)
            {
                if (sceneName == SCENE_MAIN_MENU)
                    GameManager.Instance.SetState(GameState.MainMenu);
                else if (sceneName == SCENE_HQ)
                    GameManager.Instance.SetState(GameState.InHQ);
                else if (sceneName == SCENE_MISSION_01)
                    GameManager.Instance.SetState(GameState.InMission);
            }
        }

        private void OnGUI()
        {
            if (isTransitioning)
            {
                // Fullscreen fade overlay and loading bar
                GUI.color = new Color(0.02f, 0.04f, 0.08f, 0.9f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

                GUI.color = Color.cyan;
                string loadingText = $"LOADING CYBER SUBNET... {Mathf.RoundToInt(loadProgress * 100)}%";
                GUI.Label(new Rect(Screen.width * 0.5f - 120, Screen.height * 0.5f - 20, 240, 30), loadingText);

                // Progress bar
                GUI.color = new Color(0.1f, 0.2f, 0.3f, 1f);
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f + 15, 300, 8), Texture2D.whiteTexture);

                GUI.color = Color.cyan;
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f + 15, 300 * loadProgress, 8), Texture2D.whiteTexture);
            }
        }
    }
}
