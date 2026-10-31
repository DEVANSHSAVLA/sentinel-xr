# SENTINEL-XR 2.0 — Architecture & System Overview

## 1. System Topology & Dual-Engine Design
SENTINEL-XR 2.0 unifies tactical cybersecurity intelligence across native 3D spatial engines (Unity 6 / OpenXR / ARCore) and high-throughput web operations centers.

```
                           +----------------------------------------+
                           |     Central Backend Service            |
                           |     (server.ts / Node.js Express)      |
                           +-------------------+--------------------+
                                               |
                  +----------------------------+---------------------------+
                  |                                                        |
         REST API (/api/v1)                                       WebSocket Bus (/ws)
  - Telemetry & Devices (/devices)                         - Live Security Events (100Hz)
  - Incident Lifecycle (/incidents)                        - Node Status Changes
  - AI Gemini Proxy (/ai/chat, /ai/investigate)            - Anomaly Alerts & Threat Indices
  - Forensic Dossier Signing (SHA-256)                     - Topology Reconfiguration
                  |                                                        |
         +--------+----------------------------+                           |
         |                                     |                           |
         v                                     v                           v
+-----------------------+           +-----------------------+    +-----------------------+
| Unity 6 Desktop App   |           | Android ARCore Mobile |    | Web SOC Console       |
| (SentinelXR.exe)      |           | (SentinelXR.apk)      |    | (index.html / React)  |
| - 3D Neural Topology  |           | - Real-world Rack AR  |    | - Real-time Dashboard |
| - OpenXR VR Command   |           | - Hardware Diagnostics|    | - Lab Simulation      |
| - URP Shaders         |           | - QR Physical Overlay |    | - Forensic Inspector  |
+-----------------------+           +-----------------------+    +-----------------------+
```

## 2. Nine Network Nodes and Topological Segmentation
The core network model represents an enterprise hybrid enclave partitioned into four zero-trust zones:

1. **`gateway-01` (Internet Gateway)** - Zone: *External* (198.51.100.1). Primary BGP WAN routing and ingress scrubbing.
2. **`firewall-01` (Perimeter Next-Gen Firewall)** - Zone: *Perimeter* (198.51.100.254). Stateful inspection and IDS/IPS packet decoders.
3. **`dmz-proxy` (Reverse Proxy & WAF)** - Zone: *DMZ* (10.0.1.10). Envoy/Nginx edge load balancing and SSL offloading.
4. **`server-01` (Primary Production Application Server)** - Zone: *Internal* (10.0.2.15). Enterprise app hosting (HTTP 443, API 8080).
5. **`server-02` (Secondary Application Server)** - Zone: *Internal* (10.0.2.16). Microservices container cluster.
6. **`db-cluster-01` (Encrypted Database Cluster)** - Zone: *Internal* (10.0.3.50). PostgreSQL multi-master with encryption at rest.
7. **`auth-service` (Identity & SSO Provider)** - Zone: *Internal* (10.0.3.10). OAuth2/OIDC, LDAP, Kerberos authentication.
8. **`workstation-01` (SecOps Engineering Workstation)** - Zone: *Secure Core* (10.0.4.101). Privileged operator terminal.
9. **`workstation-02` (Financial Audit Terminal)** - Zone: *Secure Core* (10.0.4.102). Regulated transaction client.

## 3. Five Network Status States
Each node evaluates real-time anomaly scores and telemetry vectors to enter one of 5 visual and operational states:
- **`healthy` (Emerald Hologram, Risk 0-25):** Nominal packet rates, standard CPU/memory loads, zero active threat indicators.
- **`suspicious` (Amber Glow, Risk 26-60):** Elevated port probing, intermittent handshake failures, or anomalous login spikes.
- **`critical` (Crimson Pulse, Risk 61-99):** Confirmed active attack (e.g. SYN flood > 5,000 pkts/s, brute-force lockout, ransomware exfiltration).
- **`isolated` (Cyan Stasis Shield, Risk Locked):** Network interfaces electronically severed via automated or manual containment.
- **`offline` (Muted Steel Slate):** Unreachable, powered down, or decommissioned.

## 4. Machine Learning & Anomaly Detection Architecture
- **Isolation Forest Scorer:** Normalizes multi-dimensional telemetry (packetRate, errorRate, connectionCount, byteVolume) into a bounded anomaly score [0.0 - 1.0].
- **Attribution (SHAP):** Calculates Shapley additive explanations for top feature contributors (e.g., Packet Rate Surge: +0.48, Protocol Anomaly: +0.32).
- **Automated Incident Creation:** When anomaly score exceeds 0.85 and severity is critical, an automated incident ticket (`INC-XXXX`) is minted with cryptographic timeline recording.
- **AI Copilot (Gemini 2.5 Flash):** Server-side proxy evaluates threat parameters against MITRE ATT&CK tactics, recommending containment and forensic steps.

## 5. Forensic Integrity & Cryptographic Dossier
All resolved or contained incidents produce an immutable forensic dossier sealed with SHA-256:
- Canonical JSON structure containing incident metadata, timeline events, SHAP attribution vectors, and actor signatures.
- SHA-256 digest computed across canonical string: `SHA-256(canonicalPayload)`.
- Prevents post-facto tampering in legal and regulatory compliance reviews.
