# SENTINEL-XR 2.0 — AR Implementation Guide

## 1. Overview
The AR subsystem enables security analysts to walk through physical data centers and server rooms while viewing real-time cybersecurity telemetry floating directly above physical server racks.

---

## 2. Optical Image Anchor Workflow

1. **Physical Marker Generation**:
   - Navigate to the **XR Center** tab in the desktop application or download the SVG marker files from `Assets/Art/Markers/`.
   - Print markers on standard letter/A4 paper and affix them to server chassis faces.

2. **Tracking Pipeline (`ARMarkerManager.cs`)**:
   - The device camera streams frames to `ARTrackedImageManager`.
   - The subsystem matches visual features against the `XRReferenceImageLibrary`.
   - Upon detection, an `ARTrackedImage` is instantiated with a 3D world pose ($P \in \mathbb{R}^3, R \in SO(3)$).

3. **World-Space HUD (`ARDeviceOverlay.cs`)**:
   - Projects a high-contrast Obsidian Neural panel showing:
     - Bound Device Name & Simulated IP
     - Real-time CPU, RAM, and Packet Throughput
     - Current Anomaly / Risk Index ($0-100$)
     - Quarantined / Online Status
     - Immediate Physical Quarantine Button
