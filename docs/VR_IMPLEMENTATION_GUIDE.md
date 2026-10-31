# SENTINEL-XR 2.0 — VR Implementation & Operations Guide

## 1. Overview
The VR subsystem immerses the security analyst in a virtual 3D Security Operations Center. It features a central holographic network table, floating threat monitors, and an AI investigation station.

---

## 2. Interaction Model

1. **Locomotion**:
   - Continuous smooth locomotion (Left thumbstick) or Teleportation (Right thumbstick ray).
2. **Direct & Ray Interaction**:
   - `XRRayInteractor` for selecting remote UI displays and floating incident cards.
   - `XRDirectInteractor` for grasping and manipulating 3D network nodes on the central holographic table.
3. **3D Forensic Incident Reconstruction (`VRIncidentReplay.cs`)**:
   - Reconstructs incident attacks chronologically using real backend event records.
   - Shows volumetric surge vectors from external source IPs penetrating the DMZ and threatening the internal core.
4. **Desktop Fallback Navigation**:
   - For environments without tethered VR headsets, the application includes a full 6DoF WASD + Mouse look camera controller to explore the exact same virtual room.
