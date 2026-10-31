# SENTINEL-XR 2.0 — Operator Manual

## 1. Quick Start
1. **Launch the Application:** Launch `SentinelXR.exe` on desktop or open the browser console at `http://localhost:3000`.
2. **Authenticate:** Select user profile (e.g. `operator-01` [Lead SOC Analyst] or `analyst-02` [Incident Responder]).
3. **Inspect the 3D Network Topology:**
   - Left-click/drag to rotate the camera around the topological cluster.
   - Right-click/drag or scroll to zoom and pan.
   - Click any node (e.g., `server-01`) to view live telemetry, CPU/Memory utilization, and open services.

## 2. Managing Cyber Incidents
When an anomaly is detected:
1. The affected node pulses crimson (`critical` state) in the 3D topology.
2. The Incident Management banner sounds an alert and lists the active incident (`INC-xxxx`).
3. Click the incident card to inspect:
   - **Severity & Scenario:** (e.g., SYN-Flood DDoS, Credential Stuffing, Lateral Movement).
   - **SHAP Anomaly Factors:** Why the ML model classified the traffic as malicious.
   - **Affected Infrastructure:** Specific hostnames and IP addresses.
4. **Consult AI Copilot:**
   - Click "Ask Gemini Copilot" or type custom forensic inquiries in the Copilot prompt.
   - Gemini analyzes the incident context and returns MITRE ATT&CK mappings, blast radius estimates, and remediation commands.
5. **Execute Containment:**
   - Click "Approve & Execute Containment" or use the manual "Isolate Host" switch.
   - The backend severs virtual routing to the compromised node, transitioning its status to `isolated` (cyan stasis field).
   - Incident timeline automatically logs the operator's containment action.
6. **Generate & Export Forensic Dossier:**
   - Click "Export Forensic Dossier".
   - The system produces a cryptographic report stamped with a verifiable SHA-256 hash.

## 3. Running Simulation Laboratory
To conduct tabletop exercises or validate defensive controls:
1. Navigate to the **Simulation Lab** tab.
2. Choose a threat scenario:
   - *Distributed Denial of Service (DDoS)*
   - *Credential Stuffing / Brute-Force*
   - *Internal Lateral Movement*
   - *Data Exfiltration / Ransomware*
3. Select the target node (e.g. `server-01` or `auth-service`).
4. Set intensity (1x to 10x).
5. Click **Launch Simulation**.
6. Observe live synthetic packet generation, telemetry spike, anomaly detection, and incident ticket creation.
7. Click **Reset Simulation** to clear synthetic vectors and restore nominal state.
