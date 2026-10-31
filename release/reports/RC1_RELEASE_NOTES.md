# SENTINEL-XR 2.0 — Release Candidate 1 (RC1) Release Notes

**Version:** 2.0.0-rc1  
**Build Date:** October 30, 2026  
**Target Environments:** Windows 64-bit Standalone, Android AR (ARCore), OpenXR VR (Meta Quest/SteamVR), Modern Browser / WebGL SOC  

---

## 1. Release Overview
SENTINEL-XR 2.0 represents a generational evolution from 2D flat dashboard security monitoring to an immersive, spatial cybersecurity command center. Powered by a high-throughput Node.js/Bun streaming core, Unity 6 Universal Render Pipeline, and Google Gemini 2.5 Flash AI, SENTINEL-XR gives SecOps teams real-time visibility, automated anomaly classification, explainable AI, and instant threat containment.

---

## 2. Key Features in Release Candidate 1

### 2.1 Spatial 3D Network Topology
- High-fidelity 3D spatial representation of 9 enterprise network nodes partitioned into 4 Zero-Trust zones (External, Perimeter, DMZ, Internal, Secure Core).
- 5 dynamic network status states (`healthy`, `suspicious`, `critical`, `isolated`, `offline`) rendered via the custom `ObsidianHologram` shader.
- Full orbital navigation, click selection, telemetry inspection, and raycast interactions.

### 2.2 Machine Learning & Explainable AI (SHAP)
- In-memory Isolation Forest anomaly scoring engine evaluating packet rates, byte surges, port surfaces, and authentication patterns in real time.
- Integrated SHAP (Shapley Additive Explanations) breaking down exactly which factors triggered the anomaly alert.
- Automated incident ticket generation (`INC-xxxx`) when severity reaches critical thresholds.

### 2.3 Gemini 2.5 Flash Cybersecurity Copilot
- High-speed natural language copilot connected via a secure server-side proxy.
- Real-time incident context ingestion: analyzes attack vectors against the MITRE ATT&CK matrix, calculates blast radius, and provides step-by-step remediation commands.
- Absolute server-side isolation: API keys are never packaged into client applications or transmitted to end devices.

### 2.4 One-Click Quarantine & Electronic Containment
- Immediate host isolation capability severing all virtual routing connections to compromised devices.
- Visual state update instantly renders the node in a stasis cyan shield in both 3D spatial environments and 2D SOC dashboards.

### 2.5 Cryptographically Sealed Forensic Dossiers
- Automated compilation of legal forensic dossiers upon incident resolution.
- Tamper-evident SHA-256 seal computed and stored with canonical timeline records.

---

## 3. Platform Distribution Packages
- **`release/windows/`**: Batchmode build scripts, configuration files, and package manifests for Windows Standalone desktop deployment.
- **`release/android/`**: AndroidManifest.xml, ARCore configurations, and build scripts for mobile AR data-center overlay operations.
- **`release/vr/`**: OpenXR settings, 6DoF interaction profiles, and VR build scripts for room-scale tactical SOC commanding.
- **`release/configuration/`**: Pre-configured environment profiles for local dev, LAN testing, and production deployments.
- **`release/documentation/`**: Comprehensive deployment guides, architectural specifications, and operator manuals.
