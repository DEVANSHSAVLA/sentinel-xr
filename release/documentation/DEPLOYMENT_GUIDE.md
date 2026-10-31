# SENTINEL-XR 2.0 — Enterprise Deployment Guide

## 1. System Architecture Summary
SENTINEL-XR 2.0 operates as a dual-runtime enterprise cybersecurity operations system:
- **Central Intelligence & Telemetry Core (`server.ts`):** High-throughput Express + WebSocket streaming core running on Node.js/Bun. Connects to SIEM/EDR streams, calculates Isolation Forest anomaly scores, generates SHAP attributions, manages incident state lifecycles, and securely brokers queries to Google Gemini 2.5 Flash without exposing API keys to edge endpoints.
- **Native Spatial SOC Frontend (`UnityProject/SentinelXR`):** Unity 6 (6000.0.23f1) Universal Render Pipeline client delivering a 3D topological graph, neural status shaders, immersive VR SOC command spaces, and AR mobile overlays for data-center hardware maintenance.
- **Web Simulation SOC Client (`index.html` + `src/`):** High-performance desktop browser SOC workstation offering 1:1 functional equivalence for remote incident responders, monitoring rooms, and headless test suites.

## 2. Infrastructure Requirements
| Component | Minimum Specification | Recommended Production |
|---|---|---|
| **Core Server OS** | Ubuntu 22.04 LTS / Debian 12 | Red Hat Enterprise Linux 9.2 |
| **Server CPU / RAM** | 4 vCPU, 8 GB RAM | 8 vCPU, 16 GB RAM |
| **Server Runtime** | Node.js 20+ or Bun 1.1+ | Node.js 22 LTS with PM2 / Docker |
| **Unity Desktop Client** | Windows 10/11 64-bit, Intel i5, GTX 1660 | Windows 11 64-bit, Intel i7/Ryzen 7, RTX 4070 |
| **VR Headset** | Meta Quest 2 via Link (Oculus Runtime) | Meta Quest 3 / Valve Index (OpenXR 1.0) |
| **AR Mobile Client** | Android 8.0 (API 26), ARCore 1.30+ | Android 13+ (Pixel 7/8/9, Galaxy S23/S24) |

## 3. Server Deployment Instructions

### Option A: Direct Node.js / PM2 Deployment
```bash
# 1. Install dependencies
npm ci --production

# 2. Configure Environment Secrets
cp .env.example .env
# Edit .env with your actual enterprise GEMINI_API_KEY and PORT=3000

# 3. Compile backend
npm run build

# 4. Start backend service
node dist/server.js
# Or with PM2 process manager:
pm2 start dist/server.js --name sentinel-core -i max
```

### Option B: Docker Container Deployment
```bash
# Build Docker image
docker build -t sentinel-core:2.0.0-rc1 .

# Run with environment variables
docker run -d \
  --name sentinel-core \
  -p 3000:3000 \
  -e GEMINI_API_KEY="AIzaSy..." \
  -e NODE_ENV="production" \
  --restart unless-stopped \
  sentinel-core:2.0.0-rc1
```

## 4. Unity Client Deployment

### Building Windows Desktop Client
1. Open terminal on Windows workstation with Unity 6000.0.23f1 installed.
2. Execute `release/windows/build_windows.bat`.
3. Standalone executable `SentinelXR.exe` and its `SentinelXR_Data` directory are output into `release/windows/`.
4. Copy `SentinelXR_Config.json` adjacent to `SentinelXR.exe` and update `apiBaseUrl` with your server IP/domain.
5. Launch `SentinelXR.exe`.

### Deploying Android AR APK
1. Connect target device with USB debugging enabled.
2. Run `adb install -r release/android/SentinelXR_v2.0.0-rc1_arcore.apk`.
3. Ensure physical server racks have registered ArUco or QR markers matching device IDs (`server-01`, `db-cluster-01`, etc.).

### Deploying OpenXR VR Experience
1. Connect VR headset to PC via USB-C or AirLink.
2. Set default OpenXR runtime in Oculus App / SteamVR.
3. Launch `release/vr/SentinelXR_VR.exe`.
