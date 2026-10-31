# SENTINEL-XR 2.0 — Operator & Analyst User Manual

## 1. Quick Navigation
- **Dashboard**: High-level command center displaying Total Devices (9), Network Risk Index ($0-100$), Active Incidents, Traffic Ingestion rate, and Live 3D Spatial Topology.
- **3D Topology**: Full-screen 3D view. Left-click and drag to rotate, scroll wheel to zoom, click on any node to open the Node Inspector and view CPU/RAM/traffic metrics or trigger quarantine.
- **Devices**: Inventory view of all 9 nodes (Gateway, Firewall, Core Router, DMZ Web Server, API Gateway, Vault Database, Workstations, Client Subnet) with zone filtering and direct isolation toggles.
- **Threat Intel**: Stream of raw and normalized packet events with anomaly scores and raw payload inspect modal.
- **Incidents**: Complete 9-stage lifecycle management view. Inspect SHAP attribution factors, review AI recommended remediation, and click **Approve & Execute Simulated Containment** to quarantine infected systems.
- **Sim Lab**: Choose between 4 attack scenarios (DDoS, Brute-Force, Port-Scan, Lateral Movement), select target node and packet generation intensity, then click **Launch Attack Simulation**.
- **AI Copilot**: Natural language investigation assistant powered by Google Gemini 2.5 Flash with grounded incident context.
- **XR Center**: Download printable physical AR QR image markers and review Unity VR SOC setup guidelines.
- **Reports**: Export complete incident logs to CSV format or generate text forensic dossiers.
