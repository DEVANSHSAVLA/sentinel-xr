using UnityEngine;

namespace SentinelXR.Gameplay.Player
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(GameObject player);
    }

    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float interactRange = 4.0f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        private Camera playerCamera;
        private IInteractable currentTarget;
        private string currentPrompt = "";

        public string CurrentPrompt => currentPrompt;
        public bool HasTarget => currentTarget != null;

        private void Start()
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null && SentinelPlayerController.Instance != null)
            {
                playerCamera = SentinelPlayerController.Instance.PlayerCamera;
            }
        }

        private void Update()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentTarget = interactable;
                    currentPrompt = interactable.GetInteractionPrompt();

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact(gameObject);
                    }
                    return;
                }
            }

            currentTarget = null;
            currentPrompt = "";
        }
    }
}
