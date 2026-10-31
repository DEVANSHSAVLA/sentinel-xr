# SENTINEL-XR 2.0 — Unity Architecture Specification

**Engine Target:** Unity 6 LTS (Universal Render Pipeline)  
**Scripting Target:** C# (.NET Standard 2.1 / C# 9.0)  
**Rendering Pipeline:** Universal Render Pipeline (URP 17.x)  
**Input Architecture:** New Unity Input System (`com.unity.inputsystem`)  

---

## 1. Architectural Overview

SENTINEL-XR 2.0 is designed as a modular, service-oriented Unity architecture built to support three target application modes from a single unified codebase:

1. **Mode A — Desktop SOC**:
   - Primary operational environment for security analysts and SOC leads.
   - Built with **Unity UI Toolkit (USS / UXML)** for scalable, resolution-independent HUD rendering.
   - 3D Interactive Topology with orbit, free-pan, and focus camera controllers.
2. **Mode B — Mobile AR**:
   - Powered by **Unity AR Foundation** and **ARCore XR Plugin**.
   - Optical 2D image marker tracking anchors holographic device stats over physical server racks.
3. **Mode C — Immersive VR SOC**:
   - Powered by **OpenXR** and **XR Interaction Toolkit 3.x**.
   - 6DoF room-scale command center with spatial incident playback and grab-and-rotate nodes.

---

## 2. Namespace & Class Decomposition

```
SentinelXR
├── Core
│   ├── AppBootstrapper.cs          # Initialization & dependency injection
│   ├── SentinelEnums.cs            # Status, severity, and lifecycle enums
│   └── ThemeManager.cs             # Midnight Ops vs Light Lab palette swatches
├── Network
│   ├── SentinelApiClient.cs        # UnityWebRequest REST client (/api/v1)
│   └── SentinelWebSocketClient.cs  # Thread-safe WebSocket listener (/ws)
├── Topology
│   ├── NetworkTopologyManager.cs   # Spawns 3D node prefabs & LineRenderer links
│   ├── NetworkNodeObject.cs        # Node interaction & risk emissive materials
│   └── NetworkEdgeRenderer.cs      # Animated packet particles along edges
├── Incidents
│   └── IncidentManager.cs          # 9-stage lifecycle & containment authorization
├── Simulation
│   └── SimulationController.cs     # Scenarios A-D triggers & intensity
├── AI
│   └── AiCopilotPanel.cs           # Gemini API chat interface
├── AR
│   ├── ARMarkerManager.cs          # ARTrackedImageManager event listener
│   └── ARDeviceOverlay.cs          # World-space holographic HUD anchor
└── VR
    ├── VRSocRoomManager.cs         # Holographic table & environmental lighting
    ├── VRNetworkInteraction.cs     # XRGrabInteractable spatial manipulators
    └── VRIncidentReplay.cs         # 3D incident reconstruction & scrubbing
```

---

## 3. Asynchronous Threading & Main-Thread Dispatch

WebSockets receive messages on a background I/O thread. To safely modify Unity GameObjects and materials, `SentinelWebSocketClient.cs` uses a thread-safe `ConcurrentQueue<Action>`:
```csharp
private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

private void Update()
{
    while (mainThreadQueue.TryDequeue(out Action action))
    {
        action?.Invoke();
    }
}
```
This guarantees zero main-thread freezing and eliminates Unity cross-thread exceptions.
