# SENTINEL-XR 2.0 — BACKEND RELEASE CONFIGURATION REPORT

**Document ID:** SEC-RC2-BCK-2026-1030  
**Release:** `2.0.0-rc2`  
**Backend Port:** `3000` (REST & Duplex WebSocket)  
**Security Architecture:** Zero-Secret Client Distribution  

---

## 1. MULTI-ENVIRONMENT TOPOLOGY ARCHITECTURE

SENTINEL-XR 2.0 abandons single-environment hardcoded URLs in favor of dynamic runtime configuration resolution. The system supports three operational deployment models:

```
+---------------------------------------------------------------------------------------------------+
|                                SENTINEL-XR 2.0 DEPLOYMENT ARCHITECTURE                            |
+------------------------------------+----------------------------------+---------------------------+
| 1. LOCAL DEVELOPMENT               | 2. LOCAL AREA NETWORK (LAN)      | 3. DEPLOYED PRODUCTION    |
| - URL: http://127.0.0.1:3000/api/v1| - URL: http://192.168.1.50:3000  | - URL: https://sentinel...|
| - WS:  ws://127.0.0.1:3000/ws      | - WS:  ws://192.168.1.50:3000/ws | - WS:  wss://sentinel...  |
| - Auth: Bearer Operator Token      | - Auth: Shared Station Token     | - Auth: Corporate IAM/JWT |
| - Scope: Single Workstation        | - Scope: Multi-Device SOC Lab    | - Scope: Enterprise Cloud |
+------------------------------------+----------------------------------+---------------------------+
```

---

## 2. RUNTIME RESOLUTION HIERARCHY

In Unity C# (`Assets/Scripts/Core/SentinelRuntimeConfig.cs`), endpoint discovery executes in the following precedence order:
1. **Command-Line Arguments:**
   `SentinelXR.exe --api-url https://soc.internal/api/v1 --ws-url wss://soc.internal/ws`
2. **Environment Variables:**
   `SENTINEL_API_URL` and `SENTINEL_WS_URL`
3. **External Configuration JSON:**
   Searched in order:
   - Root application directory: `SentinelXR_Config.json`
   - Data directory: `SentinelXR_Data/StreamingAssets/SentinelXR_Config.json`
   - Custom template: `release/configuration/sentinel_config.production.json`
4. **Fallback Default:**
   Loopback `127.0.0.1:3000` for rapid local iteration.

---

## 3. VERIFIED BACKEND SUBSYSTEM CAPABILITIES

| Subsystem | Verified Behavior | Test Method | Status |
|:---|:---|:---:|:---:|
| **REST Health Check** | Returns HTTP 200 with server status, version, and AI provider status. | GET `/api/v1/health` | **PASS** |
| **Authentication Service** | Bearer token authorization (`token-operator-alpha-9921`). Rejects unauthorized queries. | POST `/api/v1/auth/login` | **PASS** |
| **Database Persistence** | In-memory thread-safe state (`src/server/db.ts`) with nodes, incidents, and audit trails. | GET `/api/v1/devices` | **PASS** |
| **WebSocket Duplex Bus** | Streams `SECURITY_EVENT`, `TOPOLOGY_UPDATE`, `INCIDENT_UPDATE` in real time. | ws connection to `/ws` | **PASS** |
| **Automatic Reconnection** | Client implements exponential backoff retry loop (1s -> 2s -> 4s up to 10 attempts). | Unit & E2E verification | **PASS** |
| **Incident Lifecycle Management** | State transitions: `detected` -> `investigating` -> `contained` -> `resolved`. | POST `/api/v1/incidents/:id/*` | **PASS** |
| **Authoritative Isolation** | Severs all network topology edges connected to the quarantined node. | POST `/api/v1/devices/:id/isolate`| **PASS** |
| **Gemini AI Mediation** | Server-side proxy isolates `GEMINI_API_KEY`; clients never receive raw keys. | POST `/api/v1/ai/investigate` | **PASS** |

---

## 4. ZERO-SECRET DISTRIBUTION AUDIT

A complete search across all Unity scripts, asset files, and release configurations confirms:
- **Zero API Keys in Unity Builds:** The Google Gemini API key is solely managed on the server via `process.env.GEMINI_API_KEY`.
- **Zero Database Passwords in C#:** The Unity player interacts purely through REST endpoints.
- **Session Tokens:** Only short-lived operator bearer tokens are stored in configuration templates.
