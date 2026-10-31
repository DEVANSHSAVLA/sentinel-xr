using System.Collections.Generic;
using UnityEngine;
using SentinelXR.Network;

namespace SentinelXR.VR
{
    public class VRIncidentReplay : MonoBehaviour
    {
        [Header("Replay Configuration")]
        [SerializeField] private float playbackSpeed = 1.0f;
        [SerializeField] private LineRenderer attackPathRenderer;

        private bool isPlaying = false;
        private float currentTimelineProgress = 0.0f;
        private IncidentData currentIncident;

        public void LoadIncident(IncidentData incident)
        {
            currentIncident = incident;
            currentTimelineProgress = 0.0f;
            Debug.Log($"[SENTINEL-XR VR] Loaded Incident {incident.id} for 3D spatial reconstruction.");
        }

        public void Play()
        {
            isPlaying = true;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void ResetReplay()
        {
            currentTimelineProgress = 0.0f;
            isPlaying = false;
        }

        private void Update()
        {
            if (!isPlaying || currentIncident == null) return;

            currentTimelineProgress += Time.deltaTime * playbackSpeed * 0.1f;
            if (currentTimelineProgress > 1.0f)
            {
                currentTimelineProgress = 1.0f;
                isPlaying = false;
                Debug.Log("[SENTINEL-XR VR] Incident replay sequence completed.");
            }
        }
    }
}
