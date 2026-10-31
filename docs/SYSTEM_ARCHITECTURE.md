# SENTINEL-XR — System Architecture & Data Contracts

## 1. Architectural Topology

SENTINEL-XR operates on a distributed real-time event pipeline designed for ultra-low latency spatial synchronization between web clients, mobile AR endpoints, and immersive VR headsets.

### Layers

1. **Ingestion & Simulation Layer**:
   - Synthetic network event generator (`simulationEngine.ts`) produces realistic network records (packet rate, bytes per sec, source/dest IPs, failed auth velocity).
   - Generates four attack patterns: DDoS Volumetric, SSH Brute Force, Port Reconnaissance, and Internal East-West Lateral Traversal.

2. **Analytics & ML Inference Layer**:
   - Feature normalization extracts 8-dimensional telemetry vectors.
   - Isolation Forest calculates path-length anomaly scores ($0.0 - 1.0$).
   - Supervised classification attributes attack categories with confidence scores ($80\% - 99\%$).
   - SHAP (SHapley Additive exPlanations) computes local additive feature importance metrics.

3. **Core Authoritative Backend**:
   - Express & FastAPI-compatible service (`server.ts`).
   - Maintains authoritative network topology, node statuses (Healthy, Suspicious, Critical, Isolated), and incident states.
   - Provides REST APIs under `/api/v1` and WebSocket feeds on `/ws`.

4. **Presentation & Spatial Experience**:
   - **Web Dashboard**: React 19 + Three.js WebGL interactive 3D topology with packet flow particles.
   - **Unity AR**: AR Foundation image target tracking over physical server anchors.
   - **Unity VR**: OpenXR 6DoF Virtual Reality Security Operations Center.

---

## 2. Real-Time WebSocket Protocol

WebSocket Endpoint: `ws://<host>:3000/ws`

### Outbound Messages
- `SECURITY_EVENT`: Emitted on each newly ingested or generated network event.
  ```json
  {
    "type": "SECURITY_EVENT",
    "data": {
      "id": "EVT-1002",
      "timestamp": "2026-10-30T06:15:00Z",
      "sourceIp": "198.51.100.88",
      "destIp": "10.0.1.10",
      "destPort": 80,
      "protocol": "TCP",
      "packetRate": 14200,
      "bytesPerSec": 14540800,
      "anomalyScore": 0.92,
      "predictedAttackType": "DDoS",
      "severity": "critical",
      "targetDeviceId": "server-01"
    }
  }
  ```
- `TOPOLOGY_UPDATE`: Emitted when any device is isolated or topology metrics change.
- `INCIDENT_CREATED` & `INCIDENT_UPDATE`: Emitted when incidents progress through the 9-stage lifecycle.
- `XR_UPDATE`: Broadcasts active spatial anchors and headset connection statuses.
