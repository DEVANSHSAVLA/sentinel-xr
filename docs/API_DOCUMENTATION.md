# SENTINEL-XR — REST API Specification (OpenAPI /v1)

Base URL: `http://localhost:3000/api/v1`

## Endpoints Summary

### Authentication
- `GET /api/v1/auth/users`: Returns list of configured security personnel profiles (Admin, Analyst, Viewer).
- `POST /api/v1/auth/login`: Authenticates user role and issues session bearer token.

### Devices & Topology
- `GET /api/v1/devices`: Returns all 9 simulated network nodes with live telemetry (CPU, RAM, traffic, risk score).
- `GET /api/v1/devices/:id`: Detailed profile for single node.
- `POST /api/v1/devices/:id/isolate`: Quarantines or reconnects node to/from simulated network. Body: `{"isolate": boolean}`.
- `GET /api/v1/network/topology`: Returns complete spatial 3D node coordinates, zones, and connections.

### Threat Intelligence & Events
- `GET /api/v1/events`: Ingested security telemetry feed. Supports query params `limit` and `severity`.
- `GET /api/v1/threats`: Active threat feed categorized by attack vector.

### Incidents Lifecycle
- `GET /api/v1/incidents`: All active and historical incidents.
- `GET /api/v1/incidents/:id`: Incident details including SHAP attribution factors.
- `POST /api/v1/incidents/:id/contain`: Human supervisor approves containment quarantine.
- `POST /api/v1/incidents/:id/resolve`: Marks incident verified and resolved.
- `GET /api/v1/incidents/:id/report`: Generates complete text forensic dossier.

### Simulation Laboratory
- `GET /api/v1/simulation/status`: Current simulation engine state.
- `POST /api/v1/simulation/start`: Initiates synthetic generator. Body: `{"scenarioId": "ddos"|"bruteforce"|"portscan"|"lateral", "targetDeviceId": string, "intensity": string}`.
- `POST /api/v1/simulation/pause`: Toggles pause on generator.
- `POST /api/v1/simulation/stop`: Stops simulation.
- `POST /api/v1/simulation/reset`: Resets all device risk scores and clears simulation state.

### AI Investigation Copilot
- `POST /api/v1/ai/investigate`: Queries Gemini API assistant with grounded telemetry and SHAP factors. Body: `{"incidentId": string, "deviceId": string, "userPrompt": string}`.

### Extended Reality (XR)
- `GET /api/v1/xr/status`: Session status for AR and VR endpoints.
- `POST /api/v1/xr/sync`: Bidirectional handshake synchronizing spatial anchors.

### Analytics & Reports
- `GET /api/v1/analytics/overview`: High-level security posture KPIs, risk index, and threat distributions.
- `GET /api/v1/reports/export?format=csv`: Exports full incident database to CSV file.
- `GET /api/v1/health`: System health and operational readiness check.
