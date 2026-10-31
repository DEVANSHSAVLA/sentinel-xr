import express, { Request, Response } from 'express';
import http from 'http';
import { WebSocketServer, WebSocket } from 'ws';
import path from 'path';
import { fileURLToPath } from 'url';
import dotenv from 'dotenv';
import { db } from './src/server/db';
import { generateSyntheticEvent } from './src/utils/simulationEngine';
import { advanceIncidentStatus, generateForensicReport } from './src/utils/incidentLifecycle';
import { computeRiskIndex } from './src/utils/mlMath';
import { investigateWithCopilot } from './src/server/geminiCopilot';
import { Incident, SystemAnalytics, SecurityEvent } from './src/types';

dotenv.config();

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const server = http.createServer(app);
const wss = new WebSocketServer({ server, path: '/ws' });

app.use(express.json());

// Set up security headers
app.use((req, res, next) => {
  res.setHeader('X-Content-Type-Options', 'nosniff');
  res.setHeader('X-Frame-Options', 'SAMEORIGIN');
  res.setHeader('X-XSS-Protection', '1; mode=block');
  next();
});

// WebSocket broadcasting helper
function broadcast(message: object) {
  const payload = JSON.stringify(message);
  wss.clients.forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(payload);
    }
  });
}

// Background simulation ticker
let simulationTimer: NodeJS.Timeout | null = null;

function runSimulationTick() {
  if (!db.simulation.isActive || db.simulation.isPaused || !db.simulation.currentScenario) {
    return;
  }

  const targetDeviceId = db.simulation.targetDeviceId || 'server-01';
  const targetNode = db.nodes.get(targetDeviceId) || Array.from(db.nodes.values())[0];

  const result = generateSyntheticEvent(
    db.simulation.currentScenario,
    targetNode.id,
    targetNode.ip,
    db.simulation.intensity
  );

  // Ingest event
  db.events.unshift(result.event);
  if (db.events.length > 500) {
    db.events.pop();
  }
  db.simulation.totalEventsGenerated++;
  db.simulation.elapsedSeconds += 2;

  // Elevate node risk score
  if (result.event.predictedAttackType !== 'Normal' && targetNode.status !== 'isolated') {
    targetNode.status = result.event.severity === 'critical' ? 'critical' : 'suspicious';
    targetNode.riskScore = Math.min(99, targetNode.riskScore + 12);
    targetNode.trafficRate = result.event.packetRate;
  }

  // Create incident if not already tracked
  if (result.createdIncident && result.incidentPayload) {
    const existing = Array.from(db.incidents.values()).find(
      (i) => i.targetDeviceId === targetNode.id && i.status !== 'resolved'
    );

    if (!existing) {
      const incId = `INC-${Date.now().toString().slice(-4)}`;
      const now = new Date().toISOString();
      const newIncident: Incident = {
        id: incId,
        title: result.incidentPayload.title,
        severity: result.incidentPayload.severity,
        status: 'detected',
        scenario: result.incidentPayload.scenario,
        targetDeviceId: targetNode.id,
        affectedNodes: [targetNode.id],
        sourceIp: result.incidentPayload.sourceIp,
        timestamp: now,
        description: result.incidentPayload.description,
        confidenceScore: 92,
        shapFactors: [
          { feature: 'Packet Rate Surge', value: `${result.event.packetRate} pkts/s`, impact: 'Severe', contributionScore: 0.48 },
          { feature: 'Target Vulnerability Surface', value: `${result.event.protocol}:${result.event.destPort}`, impact: 'High', contributionScore: 0.32 },
          { feature: 'Source IP Reputation', value: result.event.sourceIp, impact: 'Moderate', contributionScore: 0.20 },
        ],
        recommendedAction: result.incidentPayload.recommendedAction,
        containmentApproved: false,
        timeline: [
          {
            time: now,
            stage: 'detected',
            description: `Automated detection triggered by anomalous traffic pattern (${result.event.predictedAttackType})`,
            actor: 'AI Detector',
          },
        ],
      };

      db.incidents.set(incId, newIncident);
      db.simulation.activeIncidentsGenerated++;
      broadcast({ type: 'INCIDENT_CREATED', data: newIncident });
    }
  }

  // Broadcast live event and topology updates
  broadcast({ type: 'SECURITY_EVENT', data: result.event });
  broadcast({
    type: 'TOPOLOGY_UPDATE',
    data: {
      nodes: Array.from(db.nodes.values()),
      connections: db.connections,
    },
  });
}

// -------------------------------------------------------------
// REST API ROUTES (/api/v1)
// -------------------------------------------------------------

// Health check
app.get('/api/v1/health', (req: Request, res: Response) => {
  res.json({
    status: 'healthy',
    version: '1.0.0',
    platform: 'SENTINEL-XR Full-Stack Service',
    database: 'active',
    aiProvider: process.env.GEMINI_API_KEY ? 'gemini-active' : 'local-expert-active',
    activeConnections: wss.clients.size,
    timestamp: new Date().toISOString(),
  });
});

// Authentication
app.get('/api/v1/auth/users', (req: Request, res: Response) => {
  res.json(db.users);
});

app.post('/api/v1/auth/login', (req: Request, res: Response) => {
  const { username } = req.body;
  const user = db.users.find((u) => u.username === username) || db.users[0];
  res.json({ user, token: user.token });
});

// Devices & Topology
app.get('/api/v1/devices', (req: Request, res: Response) => {
  res.json(Array.from(db.nodes.values()));
});

app.get('/api/v1/devices/:id', (req: Request, res: Response) => {
  const node = db.nodes.get(req.params.id);
  if (!node) return res.status(404).json({ error: 'Device not found' });
  res.json(node);
});

app.post('/api/v1/devices/:id/isolate', (req: Request, res: Response) => {
  const { isolate } = req.body;
  const success = db.isolateDevice(req.params.id, Boolean(isolate));
  if (!success) return res.status(404).json({ error: 'Device not found' });

  const updatedNode = db.nodes.get(req.params.id);
  broadcast({
    type: 'TOPOLOGY_UPDATE',
    data: {
      nodes: Array.from(db.nodes.values()),
      connections: db.connections,
    },
  });
  res.json({ success: true, node: updatedNode });
});

app.get('/api/v1/network/topology', (req: Request, res: Response) => {
  res.json({
    nodes: Array.from(db.nodes.values()),
    connections: db.connections,
  });
});

// Security Events
app.get('/api/v1/events', (req: Request, res: Response) => {
  const limit = Number(req.query.limit) || 50;
  const severity = req.query.severity as string | undefined;
  let filtered = db.events;
  if (severity && severity !== 'all') {
    filtered = filtered.filter((e) => e.severity === severity);
  }
  res.json(filtered.slice(0, limit));
});

// Threat Feed
app.get('/api/v1/threats', (req: Request, res: Response) => {
  const threatCategories = [
    { category: 'DDoS / Volumetric Surge', activeThreats: 1, severity: 'critical', trend: '+14%' },
    { category: 'Credential Stuffing & Brute Force', activeThreats: 1, severity: 'high', trend: '+6%' },
    { category: 'Subnet Reconnaissance & Port Scans', activeThreats: 0, severity: 'medium', trend: '-2%' },
    { category: 'Lateral Movement & East-West Pivots', activeThreats: 0, severity: 'high', trend: 'stable' },
  ];
  res.json(threatCategories);
});

// Incidents
app.get('/api/v1/incidents', (req: Request, res: Response) => {
  res.json(Array.from(db.incidents.values()));
});

app.get('/api/v1/incidents/:id', (req: Request, res: Response) => {
  const incident = db.incidents.get(req.params.id);
  if (!incident) return res.status(404).json({ error: 'Incident not found' });
  res.json(incident);
});

app.post('/api/v1/incidents/:id/contain', (req: Request, res: Response) => {
  const incident = db.incidents.get(req.params.id);
  if (!incident) return res.status(404).json({ error: 'Incident not found' });

  // Advance lifecycle to contained
  const updated = advanceIncidentStatus(
    incident,
    'contained',
    'Security Analyst',
    `Approved containment: Target device ${incident.targetDeviceId} isolated from DMZ traffic.`
  );
  db.incidents.set(updated.id, updated);

  // Apply isolation in network state
  db.isolateDevice(incident.targetDeviceId, true);

  broadcast({ type: 'INCIDENT_UPDATE', data: updated });
  broadcast({
    type: 'TOPOLOGY_UPDATE',
    data: {
      nodes: Array.from(db.nodes.values()),
      connections: db.connections,
    },
  });

  res.json(updated);
});

app.post('/api/v1/incidents/:id/resolve', (req: Request, res: Response) => {
  const incident = db.incidents.get(req.params.id);
  if (!incident) return res.status(404).json({ error: 'Incident not found' });

  const updated = advanceIncidentStatus(
    incident,
    'resolved',
    'Security Analyst',
    'Verification tests passed. Incident marked resolved; forensic report generated.'
  );
  db.incidents.set(updated.id, updated);

  // Restore node to healthy status
  const node = db.nodes.get(incident.targetDeviceId);
  if (node) {
    node.status = 'healthy';
    node.riskScore = 15;
  }

  broadcast({ type: 'INCIDENT_UPDATE', data: updated });
  res.json(updated);
});

app.get('/api/v1/incidents/:id/report', (req: Request, res: Response) => {
  const incident = db.incidents.get(req.params.id);
  if (!incident) return res.status(404).json({ error: 'Incident not found' });
  const dossier = generateForensicReport(incident);
  res.json({ report: dossier, incident });
});

// Simulation Controls
app.get('/api/v1/simulation/status', (req: Request, res: Response) => {
  res.json(db.simulation);
});

app.post('/api/v1/simulation/start', (req: Request, res: Response) => {
  const { scenarioId, targetDeviceId, intensity } = req.body;
  db.simulation.isActive = true;
  db.simulation.isPaused = false;
  db.simulation.currentScenario = scenarioId || 'ddos';
  db.simulation.targetDeviceId = targetDeviceId || 'server-01';
  db.simulation.intensity = intensity || 'medium';

  if (!simulationTimer) {
    simulationTimer = setInterval(runSimulationTick, 2000);
  }

  broadcast({ type: 'SIMULATION_UPDATE', data: db.simulation });
  res.json(db.simulation);
});

app.post('/api/v1/simulation/pause', (req: Request, res: Response) => {
  db.simulation.isPaused = !db.simulation.isPaused;
  broadcast({ type: 'SIMULATION_UPDATE', data: db.simulation });
  res.json(db.simulation);
});

app.post('/api/v1/simulation/stop', (req: Request, res: Response) => {
  db.simulation.isActive = false;
  if (simulationTimer) {
    clearInterval(simulationTimer);
    simulationTimer = null;
  }
  broadcast({ type: 'SIMULATION_UPDATE', data: db.simulation });
  res.json(db.simulation);
});

app.post('/api/v1/simulation/reset', (req: Request, res: Response) => {
  if (simulationTimer) {
    clearInterval(simulationTimer);
    simulationTimer = null;
  }
  db.simulation = {
    isActive: false,
    isPaused: false,
    currentScenario: null,
    targetDeviceId: null,
    intensity: 'medium',
    elapsedSeconds: 0,
    totalEventsGenerated: 0,
    activeIncidentsGenerated: 0,
  };

  // Reset all nodes
  for (const node of db.nodes.values()) {
    node.status = 'healthy';
    node.riskScore = 15;
  }

  broadcast({ type: 'SIMULATION_UPDATE', data: db.simulation });
  broadcast({
    type: 'TOPOLOGY_UPDATE',
    data: {
      nodes: Array.from(db.nodes.values()),
      connections: db.connections,
    },
  });

  res.json({ success: true, message: 'Simulation state and nodes reset.' });
});

// AI Copilot
app.post('/api/v1/ai/investigate', async (req: Request, res: Response) => {
  try {
    const { incidentId, deviceId, userPrompt } = req.body;
    const result = await investigateWithCopilot({
      incidentId,
      deviceId,
      userPrompt: userPrompt || 'Explain why this system is flagged as suspicious and recommend response actions.',
    });
    res.json(result);
  } catch (error) {
    console.error('Error in AI investigation copilot:', error);
    res.status(500).json({ error: 'AI Copilot processing error' });
  }
});

// XR Synchronization (Web + Unity AR + Unity VR)
app.get('/api/v1/xr/status', (req: Request, res: Response) => {
  res.json(db.xrStatus);
});

app.post('/api/v1/xr/sync', (req: Request, res: Response) => {
  const { clientType, activeIncidentId, activeSpatialAnchor } = req.body;
  if (clientType === 'AR') db.xrStatus.arConnected = true;
  if (clientType === 'VR') db.xrStatus.vrConnected = true;
  if (activeIncidentId) db.xrStatus.activeIncidentId = activeIncidentId;
  if (activeSpatialAnchor) db.xrStatus.activeSpatialAnchor = activeSpatialAnchor;
  db.xrStatus.lastSyncTimestamp = new Date().toISOString();

  broadcast({ type: 'XR_UPDATE', data: db.xrStatus });
  res.json({
    success: true,
    status: db.xrStatus,
    topology: {
      nodes: Array.from(db.nodes.values()),
      connections: db.connections,
    },
    incidents: Array.from(db.incidents.values()),
  });
});

// Analytics Overview
app.get('/api/v1/analytics/overview', (req: Request, res: Response) => {
  const nodes = Array.from(db.nodes.values());
  const incidents = Array.from(db.incidents.values());

  const totalDevices = nodes.length;
  const isolatedDevices = nodes.filter((n) => n.status === 'isolated').length;
  const onlineDevices = totalDevices - isolatedDevices;
  const activeIncidents = incidents.filter((i) => i.status !== 'resolved').length;
  const criticalIncidents = incidents.filter(
    (i) => i.status !== 'resolved' && i.severity === 'critical'
  ).length;
  const highIncidents = incidents.filter(
    (i) => i.status !== 'resolved' && i.severity === 'high'
  ).length;

  const recentAnomalies = db.events.slice(0, 20).map((e) => e.anomalyScore);
  const avgAnomaly =
    recentAnomalies.length > 0
      ? recentAnomalies.reduce((a, b) => a + b, 0) / recentAnomalies.length
      : 0.1;

  const networkRiskIndex = computeRiskIndex(
    totalDevices,
    isolatedDevices,
    criticalIncidents,
    highIncidents,
    avgAnomaly
  );

  const analytics: SystemAnalytics = {
    totalDevices,
    onlineDevices,
    isolatedDevices,
    activeIncidents,
    criticalIncidents,
    networkRiskIndex,
    trafficPacketsPerSec: nodes.reduce((acc, n) => acc + n.trafficRate, 0),
    detectionAccuracy: 97.4,
    avgResolutionTimeMinutes: 4.8,
    threatDistribution: [
      { name: 'DDoS', count: incidents.filter((i) => i.scenario === 'DDoS').length || 1, color: '#FF456A' },
      { name: 'Brute-Force', count: incidents.filter((i) => i.scenario === 'Brute-Force').length || 1, color: '#FFB547' },
      { name: 'Port-Scan', count: incidents.filter((i) => i.scenario === 'Port-Scan').length, color: '#00D9FF' },
      { name: 'Lateral Movement', count: incidents.filter((i) => i.scenario === 'Lateral Movement').length, color: '#895CFF' },
    ],
  };

  res.json(analytics);
});

// CSV Report Export
app.get('/api/v1/reports/export', (req: Request, res: Response) => {
  const format = req.query.format || 'csv';
  const incidents = Array.from(db.incidents.values());

  if (format === 'csv') {
    const headers = 'ID,Title,Scenario,Severity,Status,TargetDevice,SourceIp,Confidence,Timestamp\n';
    const rows = incidents
      .map(
        (i) =>
          `"${i.id}","${i.title}","${i.scenario}","${i.severity}","${i.status}","${i.targetDeviceId}","${i.sourceIp}",${i.confidenceScore},"${i.timestamp}"`
      )
      .join('\n');

    res.setHeader('Content-Type', 'text/csv');
    res.setHeader('Content-Disposition', 'attachment; filename="sentinel_xr_incidents.csv"');
    return res.send(headers + rows);
  }

  res.json(incidents);
});

// -------------------------------------------------------------
// VITE SPA DEV & STATIC PROD MOUNT
// -------------------------------------------------------------
async function setupVite() {
  if (process.env.NODE_ENV === 'production') {
    app.use(express.static(path.join(__dirname, 'dist')));
    app.get('*', (req, res) => {
      res.sendFile(path.join(__dirname, 'dist', 'index.html'));
    });
  } else {
    const { createServer: createViteServer } = await import('vite');
    const vite = await createViteServer({
      server: { middlewareMode: true },
      appType: 'spa',
    });
    app.use(vite.middlewares);
  }
}

const PORT = 3000;
setupVite().then(() => {
  server.listen(PORT, '0.0.0.0', () => {
    console.log(`[SENTINEL-XR] System operational at http://0.0.0.0:${PORT}`);
    console.log(`[SENTINEL-XR] WebSockets online at ws://0.0.0.0:${PORT}/ws`);
  });
});
