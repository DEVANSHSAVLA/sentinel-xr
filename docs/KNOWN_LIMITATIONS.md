# SENTINEL-XR 2.0 — Known Limitations & Hardware Dependencies

In accordance with Section 3.4 (Evidence-Based Delivery) of the master execution prompt, this document explicitly records verified features, hardware boundaries, and environmental constraints.

---

## 1. Hardware Dependencies & Status

| Platform / Capability | Execution Status | Reason / Technical Detail |
|:---|:---:|:---|
| **Full-Stack Desktop SOC (Port 3000)** | **PASS** | Fully operational and verified with live WebSockets, REST APIs, and 3D WebGL runtime. |
| **Unity C# Project & Manifests** | **PASS** | Complete C# source tree, URP shaders, USS styles, and UXML layouts verified in `UnityProject/SentinelXR/`. |
| **ML & AI Copilot Pipeline** | **PASS** | Isolation Forest, supervised classifier, SHAP attribution, and Gemini API integration verified. |
| **Physical Android ARCore Tracking** | **BLOCKED** | The cloud Linux development container has no physical Android smartphone or optical camera attached. C# scripts are implemented in `ARMarkerManager.cs` and exportable SVG markers are provided for physical deployment. |
| **Physical OpenXR VR Headset 6DoF** | **BLOCKED** | The cloud environment has no physical VR headset (Meta Quest / HTC Vive) tethered to the container GPU. The application provides an interactive 3D desktop demonstration mode as a verified fallback. |

---

## 2. Cybersecurity Simulation Isolation Boundary

- All network events, traffic spikes, and incident records are **100% synthetic**.
- Containment actions (node isolation, IP blacklisting, rate limiting) apply **strictly to the simulated application state**.
- The platform never executes live external port scans, launches real denial-of-service packets, or alters physical host firewall tables.
