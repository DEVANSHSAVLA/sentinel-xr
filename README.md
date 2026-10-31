# SENTINEL-XR
### AI-Driven Immersive Extended Reality Framework for Cyber Threat Visualization, Investigation and Incident Response

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Platform](https://img.shields.io/badge/platform-Web%20%7C%20AR%20%7C%20VR-blue)]()
[![Design System](https://img.shields.io/badge/theme-Obsidian%20Neural-cyan)]()
[![AI Engine](https://img.shields.io/badge/AI-Google%20Gemini%202.5-violet)]()

---

## 1. Executive Overview

**SENTINEL-XR** is an enterprise-grade immersive cybersecurity operations platform that transforms traditional Security Operations Center (SOC) monitoring into an interactive spatial experience across Web, Augmented Reality (AR), and Virtual Reality (VR).

The platform addresses modern cyber operational challenges by unifying:
1. **Interactive 3D Spatial Topology**: Real-time WebGL visualization of network zones (External, Perimeter, DMZ, Internal, Secure Core) with animated telemetry flow and quarantine visualizers.
2. **AI & Machine Learning Detection**: Anomaly detection (Isolation Forest), supervised classification (Random Forest), and local feature explainability (SHAP attributions).
3. **AI Investigation Copilot**: Grounded threat reasoning powered by Google AI Studio Gemini API (`@google/genai`) with local rule-based expert fallback.
4. **Controlled Simulation Laboratory**: Synthetic generation of 4 threat scenarios (DDoS Volumetric Surge, Credential Stuffing, Port Reconnaissance, East-West Lateral Movement) with zero external network transmission.
5. **Human-in-the-Loop Incident Lifecycle**: 9-stage incident response pipeline with explicit supervisor authorization for simulated containment and quarantine.
6. **Unified Cross-Platform Synchronization**: Simultaneous real-time synchronization between the Web Dashboard, Unity AR (ARCore), and Unity VR (OpenXR) over WebSockets.

---

## 2. Visual Identity & Design System

Theme Name: **OBSIDIAN NEURAL**
- **Backgrounds**: Deep space dark (`#070B16`), secondary surface (`#0C1222`), elevated card (`#111A2D`).
- **Accents**: Electric Blue (`#477BFF`), Electric Cyan (`#00D9FF`), Deep Violet (`#895CFF`), Magenta (`#D45CFF`).
- **Status Semantics**: Critical (`#FF456A`), Warning (`#FFB547`), Success (`#22D3A6`).
- **Typography Pairing**: Space Grotesk (display headings), Plus Jakarta Sans (interface prose), JetBrains Mono (metrics and telemetry).
- **Dual-Theme Support**: Instant toggle between default *Midnight Ops* and bright *Light Lab* themes.
- **Anti-Slop Discipline**: Clean unboxed metadata, zero pill badge sandwiches, no ornamental status tickers, and a collapsible bottom dock for zero 3D viewport occlusion.

---

## 3. System Architecture

```
                       ┌────────────────────────────┐
                       │   WEB COMMAND DASHBOARD    │
                       │  (React 19 + Three.js + TS) │
                       └─────────────┬──────────────┘
                                     │
                                     ▼
                       ┌────────────────────────────┐
                       │  FASTAPI / EXPRESS BACKEND  │
                       │  Port: 3000 | REST + WS    │
                       └──────┬───────────────┬─────┘
                              │               │
            ┌─────────────────┴─┐           ┌─┴─────────────────┐
            │   UNITY AR CLIENT  │           │   UNITY VR SOC    │
            │   (ARCore Anchor)  │           │  (OpenXR 6DoF)    │
            └───────────────────┘           └───────────────────┘
                              │               │
                              ▼               ▼
                       ┌────────────────────────────┐
                       │  AUTHORITATIVE DATA STORE  │
                       │  (Topology, Events, State) │
                       └─────────────┬──────────────┘
                                     │
                                     ▼
                       ┌────────────────────────────┐
                       │   SIMULATION ENGINE (A-D)  │
                       │   ML INFERENCE (SHAP + IF) │
                       │   GEMINI COPILOT INTEGRATION│
                       └────────────────────────────┘
```

---

## 4. Quick Start & Execution

### Prerequisites
- Node.js >= 20
- npm >= 10
- Python 3.10+ (for analytical tooling)

### Running the Application
```bash
# 1. Install dependencies
npm install

# 2. Run unit tests
npm test

# 3. Start full-stack application (Port 3000)
npm run dev
```

Visit `http://localhost:3000` to open the SENTINEL-XR command interface.

---

## 5. Repository Structure

```
SENTINEL-XR/
├── src/
│   ├── components/            # Obsidian Neural React UI Components
│   │   ├── Header.tsx         # 3-Zone Top Bar
│   │   ├── NetworkTopology3D.tsx # Interactive Three.js 3D WebGL Scene
│   │   ├── DashboardView.tsx  # Command KPIs & Telemetry Stream
│   │   ├── DeviceExplorerView.tsx # 9-Node Inventory & Quarantine
│   │   ├── ThreatIntelView.tsx # Packet Inspection & Severity Filter
│   │   ├── IncidentManagementView.tsx # 9-Stage Lifecycle & Containment
│   │   ├── SimulationLabView.tsx # Attack Lab (Scenarios A-D)
│   │   ├── AiCopilotView.tsx  # Gemini AI Investigation Assistant
│   │   ├── XrControlCenterView.tsx # AR Target Generator & Sync
│   │   └── AnalyticsReportsView.tsx # Threat Metrics & CSV Export
│   ├── server/                # Full-Stack Server Layer
│   │   ├── db.ts              # In-Memory & Persistent State DB
│   │   └── geminiCopilot.ts   # Gemini API Client & Local Fallback
│   ├── utils/                 # Tested Pure Logic & Math
│   │   ├── mlMath.ts          # Isolation Forest & Decision Tree
│   │   ├── mlMath.test.ts     # Automated ML Tests
│   │   ├── simulationEngine.ts # Synthetic Event Generators
│   │   ├── simulation.test.ts # Simulation Tests
│   │   ├── incidentLifecycle.ts # 9-Stage State Machine
│   │   └── incidentLifecycle.test.ts # Lifecycle Tests
│   ├── types/index.ts         # Unified TypeScript Data Contracts
│   └── index.css              # Obsidian Neural Design Tokens & Themes
├── xr/
│   └── unity/
│       └── SentinelXR/        # Complete Unity 6 XR Project
│           ├── Scripts/AR/    # AR Foundation & Image Trackers
│           ├── Scripts/VR/    # OpenXR SOC Environment & Grab
│           └── Scripts/Network/# REST & WebSocket Sync Clients
├── docs/                      # Comprehensive Architecture & Audit Docs
├── server.ts                  # Express + WS + Vite Dev Server
├── docker-compose.yml         # Containerized Orchestration
├── PROJECT_STATUS.md          # Evidence-Based Verification Log
└── package.json
```

---

## 6. Testing & Quality Assurance

Run the automated test suites:
```bash
npm test
```
The test suite validates:
- Isolation Forest anomaly path-length scoring
- Random Forest supervised classification for DDoS, Brute-Force, Port-Scan, and Lateral Movement
- SHAP feature attribution contribution calculations
- Unified Network Risk Index scoring
- 9-stage incident lifecycle transitions and human containment approval
- Forensic dossier formatting
