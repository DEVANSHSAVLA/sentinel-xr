using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SentinelXR.Network;

namespace SentinelXR.AR
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ARMarkerManager : MonoBehaviour
    {
        [Header("AR Configuration")]
        [SerializeField] private GameObject deviceOverlayPrefab;
        private ARTrackedImageManager trackedImageManager;
        private readonly Dictionary<string, GameObject> activeOverlays = new Dictionary<string, GameObject>();

        private void Awake()
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        private void OnEnable()
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        private void OnDisable()
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                CreateOrUpdateOverlay(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    CreateOrUpdateOverlay(trackedImage);
                }
                else if (trackedImage.trackingState == TrackingState.None)
                {
                    string markerName = trackedImage.referenceImage.name;
                    if (activeOverlays.TryGetValue(markerName, out GameObject overlay))
                    {
                        overlay.SetActive(false);
                    }
                }
            }

            foreach (ARTrackedImage trackedImage in eventArgs.removed)
            {
                string markerName = trackedImage.referenceImage.name;
                if (activeOverlays.TryGetValue(markerName, out GameObject overlay))
                {
                    Destroy(overlay);
                    activeOverlays.Remove(markerName);
                }
            }
        }

        private void CreateOrUpdateOverlay(ARTrackedImage trackedImage)
        {
            string markerName = trackedImage.referenceImage.name; // e.g. "server-01"
            if (!activeOverlays.TryGetValue(markerName, out GameObject overlay))
            {
                if (deviceOverlayPrefab != null)
                {
                    overlay = Instantiate(deviceOverlayPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
                    activeOverlays[markerName] = overlay;
                }
            }

            if (overlay != null)
            {
                overlay.SetActive(true);
                overlay.transform.position = trackedImage.transform.position + Vector3.up * 0.15f;
                overlay.transform.rotation = trackedImage.transform.rotation;

                ARDeviceOverlay overlayComponent = overlay.GetComponent<ARDeviceOverlay>();
                if (overlayComponent != null)
                {
                    overlayComponent.Initialize(markerName);
                }
            }
        }
    }
}
