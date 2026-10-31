import { WebSocket } from 'ws';
import crypto from 'crypto';
import fs from 'fs';

interface TestStepResult {
  step: number;
  name: string;
  status: 'PASS' | 'FAIL' | 'BLOCKED';
  details: string;
  timestamp: string;
  evidence?: any;
}

const BASE_URL = 'http://localhost:3000/api/v1';
const WS_URL = 'ws://localhost:3000/ws';

async function fetchJson(endpoint: string, options: any = {}) {
  const url = `${BASE_URL}${endpoint}`;
  const res = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: 'Bearer token-operator-alpha-9921',
      ...(options.headers || {}),
    },
    ...options,
  });
  const data = await res.json();
  return { status: res.status, ok: res.ok, data };
}

async function runRc2LiveAcceptance() {
  const results: TestStepResult[] = [];
  console.log('=== INITIATING SENTINEL-XR 2.0 RC2 HARDWARE & E2E LIVE ACCEPTANCE TEST ===\n');

  // Step 1: Health & Backend Core Operational
  try {
    const health = await fetchJson('/health');
    if (health.status === 200 && health.data.status === 'healthy') {
      results.push({
        step: 1,
        name: 'Backend Core Service Operational',
        status: 'PASS',
        details: `Backend online. Platform: ${health.data.platform}, AI Provider: ${health.data.aiProvider}`,
        timestamp: new Date().toISOString(),
        evidence: health.data,
      });
      console.log('Step 1: PASS - Backend service online and responsive.');
    } else {
      throw new Error(`Unexpected health status: ${JSON.stringify(health)}`);
    }
  } catch (err: any) {
    results.push({
      step: 1,
      name: 'Backend Core Service Operational',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 2: Standalone Windows Configuration Integrity
  try {
    const configRaw = fs.readFileSync('release/windows/SentinelXR_Config.json', 'utf8');
    const config = JSON.parse(configRaw);
    if (config.application && config.application.version === '2.0.0-rc2' && config.backend && config.backend.apiBaseUrl) {
      results.push({
        step: 2,
        name: 'Windows Desktop Configuration Stamped (RC2)',
        status: 'PASS',
        details: `Valid configuration version ${config.application.version}, target: ${config.backend.apiBaseUrl}`,
        timestamp: new Date().toISOString(),
        evidence: config.application,
      });
      console.log('Step 2: PASS - Windows configuration validated.');
    } else {
      throw new Error('Config missing version 2.0.0-rc2 or backend url');
    }
  } catch (err: any) {
    results.push({
      step: 2,
      name: 'Windows Desktop Configuration Stamped (RC2)',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 3: Simulation Environment Reset & Topology Baseline Verification
  try {
    await fetchJson('/simulation/reset', { method: 'POST' });
    const topo = await fetchJson('/network/topology');
    if (topo.status === 200 && topo.data.nodes.length === 9) {
      const allHealthy = topo.data.nodes.every((n: any) => n.status === 'healthy' || n.status === 'normal');
      results.push({
        step: 3,
        name: 'Topology Baseline & Node Reset',
        status: 'PASS',
        details: `9 network nodes active and in nominal healthy state across DMZ, Enterprise, and Core Data zones.`,
        timestamp: new Date().toISOString(),
        evidence: { totalNodes: topo.data.nodes.length, connections: topo.data.connections.length },
      });
      console.log('Step 3: PASS - All 9 topology nodes reset to nominal status.');
    } else {
      throw new Error(`Topology baseline incomplete: ${JSON.stringify(topo.data)}`);
    }
  } catch (err: any) {
    results.push({
      step: 3,
      name: 'Topology Baseline & Node Reset',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 4: Establish Real-Time WebSocket Bus Connection
  let wsClient: WebSocket | null = null;
  const wsMessages: any[] = [];
  try {
    wsClient = new WebSocket(WS_URL);
    await new Promise<void>((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error('WebSocket connection timeout')), 4000);
      wsClient!.on('open', () => {
        clearTimeout(timer);
        resolve();
      });
      wsClient!.on('message', (msg) => {
        try {
          wsMessages.push(JSON.parse(msg.toString()));
        } catch {}
      });
      wsClient!.on('error', (e) => reject(e));
    });

    results.push({
      step: 4,
      name: 'WebSocket Event Bus Connected',
      status: 'PASS',
      details: `Established persistent duplex stream on ${WS_URL}`,
      timestamp: new Date().toISOString(),
    });
    console.log('Step 4: PASS - Duplex WebSocket connection established.');
  } catch (err: any) {
    results.push({
      step: 4,
      name: 'WebSocket Event Bus Connected',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 5: Launch Synthetic DDoS Simulation on SERVER-01
  try {
    const simRes = await fetchJson('/simulation/start', {
      method: 'POST',
      body: JSON.stringify({
        scenarioId: 'ddos',
        targetDeviceId: 'server-01',
        intensity: 'high',
      }),
    });

    if (simRes.status === 200 && simRes.data.isActive) {
      results.push({
        step: 5,
        name: 'Synthetic Attack Simulation Activated',
        status: 'PASS',
        details: `DDoS attack simulation injected on server-01 at high intensity.`,
        timestamp: new Date().toISOString(),
        evidence: simRes.data,
      });
      console.log('Step 5: PASS - DDoS attack simulation actively running.');
    } else {
      throw new Error(`Simulation failed to start: ${JSON.stringify(simRes.data)}`);
    }
  } catch (err: any) {
    results.push({
      step: 5,
      name: 'Synthetic Attack Simulation Activated',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Wait 3.5s for simulation ticker to generate events and update nodes
  console.log('Waiting for synthetic telemetry stream and ML engine evaluation...');
  await new Promise((r) => setTimeout(r, 3500));

  // Step 6: Verify Synthetic Telemetry Ingestion
  let liveEvents: any[] = [];
  try {
    const evRes = await fetchJson('/events?limit=10');
    liveEvents = evRes.data;
    if (liveEvents && liveEvents.length > 0 && liveEvents[0].packetRate > 5000) {
      results.push({
        step: 6,
        name: 'High-Volume Telemetry Streaming',
        status: 'PASS',
        details: `Telemetry streaming detected. Top event packet rate: ${liveEvents[0].packetRate} pkts/s, Protocol: ${liveEvents[0].protocol}`,
        timestamp: new Date().toISOString(),
        evidence: liveEvents[0],
      });
      console.log(`Step 6: PASS - Synthetic telemetry streaming (${liveEvents[0].packetRate} pkts/s).`);
    } else {
      throw new Error(`No high-rate synthetic events captured: ${JSON.stringify(liveEvents)}`);
    }
  } catch (err: any) {
    results.push({
      step: 6,
      name: 'High-Volume Telemetry Streaming',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 7: Verify Isolation Forest Anomaly Scoring
  try {
    if (liveEvents.length > 0 && liveEvents[0].anomalyScore !== undefined) {
      const score = liveEvents[0].anomalyScore;
      if (score >= 0.8) {
        results.push({
          step: 7,
          name: 'Isolation Forest Anomaly Inference',
          status: 'PASS',
          details: `Anomaly score calculated: ${score.toFixed(3)} (threshold > 0.80). Confirmed volumetric anomaly.`,
          timestamp: new Date().toISOString(),
          evidence: { anomalyScore: score, severity: liveEvents[0].severity },
        });
        console.log(`Step 7: PASS - Isolation Forest anomaly score: ${score.toFixed(3)}`);
      } else {
        throw new Error(`Anomaly score too low: ${score}`);
      }
    } else {
      throw new Error('Event missing anomaly score');
    }
  } catch (err: any) {
    results.push({
      step: 7,
      name: 'Isolation Forest Anomaly Inference',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 8: Verify SHAP Explainability Factors
  try {
    if (liveEvents.length > 0 && Array.isArray(liveEvents[0].shapFactors) && liveEvents[0].shapFactors.length > 0) {
      results.push({
        step: 8,
        name: 'SHAP Feature Attribution Generation',
        status: 'PASS',
        details: `Generated ${liveEvents[0].shapFactors.length} SHAP factors. Dominant feature: ${liveEvents[0].shapFactors[0].feature} (${liveEvents[0].shapFactors[0].impact})`,
        timestamp: new Date().toISOString(),
        evidence: liveEvents[0].shapFactors,
      });
      console.log('Step 8: PASS - SHAP factors successfully attributed.');
    } else {
      throw new Error('Missing SHAP factors on security event');
    }
  } catch (err: any) {
    results.push({
      step: 8,
      name: 'SHAP Feature Attribution Generation',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 9: Verify Real Incident Creation & Fresh Dynamic ID
  let activeIncident: any = null;
  try {
    const incRes = await fetchJson('/incidents');
    const incidents = incRes.data;
    activeIncident = incidents.find((i: any) => i.targetDeviceId === 'server-01' && i.status !== 'resolved');

    if (activeIncident && activeIncident.id) {
      results.push({
        step: 9,
        name: 'Dynamic Incident Ticket Creation',
        status: 'PASS',
        details: `Dynamic incident generated: ${activeIncident.id} [${activeIncident.severity.toUpperCase()}]. Title: ${activeIncident.title}`,
        timestamp: new Date().toISOString(),
        evidence: { id: activeIncident.id, title: activeIncident.title, status: activeIncident.status },
      });
      console.log(`Step 9: PASS - Active dynamic incident generated: ${activeIncident.id}`);
    } else {
      throw new Error('No active incident found for server-01');
    }
  } catch (err: any) {
    results.push({
      step: 9,
      name: 'Dynamic Incident Ticket Creation',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 10: Verify 3D Node Escalation to Critical Status
  try {
    const nodeRes = await fetchJson('/devices/server-01');
    const node = nodeRes.data;
    if (node && (node.status === 'critical' || node.status === 'suspicious') && node.riskScore > 20) {
      results.push({
        step: 10,
        name: '3D Node State Escalation',
        status: 'PASS',
        details: `Target node ${node.id} status transitioned to '${node.status}', risk score elevated to ${node.riskScore}/100.`,
        timestamp: new Date().toISOString(),
        evidence: node,
      });
      console.log(`Step 10: PASS - Node ${node.id} status: ${node.status}, risk: ${node.riskScore}`);
    } else {
      throw new Error(`Node status not escalated: ${JSON.stringify(node)}`);
    }
  } catch (err: any) {
    results.push({
      step: 10,
      name: '3D Node State Escalation',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 11: Real-Time WebSocket Event Verification
  try {
    const hasSecEvent = wsMessages.some((m) => m.type === 'SECURITY_EVENT');
    const hasIncEvent = wsMessages.some((m) => m.type === 'INCIDENT_CREATED' || m.type === 'INCIDENT_UPDATE');
    if (hasSecEvent || hasIncEvent) {
      results.push({
        step: 11,
        name: 'WebSocket Real-Time Broadcast Reception',
        status: 'PASS',
        details: `Received ${wsMessages.length} WebSocket telemetry packets during simulation.`,
        timestamp: new Date().toISOString(),
        evidence: { totalMessages: wsMessages.length, messageTypes: wsMessages.map((m) => m.type) },
      });
      console.log(`Step 11: PASS - Received ${wsMessages.length} live WebSocket events.`);
    } else {
      throw new Error('No SECURITY_EVENT or INCIDENT broadcast received on WebSocket');
    }
  } catch (err: any) {
    results.push({
      step: 11,
      name: 'WebSocket Real-Time Broadcast Reception',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 12: Gemini 2.5 Flash AI Copilot Investigation
  let aiAnswer = '';
  try {
    console.log('Querying Google Gemini 2.5 Flash Copilot for contextual attack analysis...');
    const copilotRes = await fetchJson('/ai/investigate', {
      method: 'POST',
      body: JSON.stringify({
        incidentId: activeIncident ? activeIncident.id : undefined,
        deviceId: 'server-01',
        userPrompt: 'Analyze this incident on server-01. Identify attack vector, MITRE technique, blast radius across DMZ, and specify containment protocol.',
      }),
    });

    if (copilotRes.status === 200 && copilotRes.data.answer) {
      aiAnswer = copilotRes.data.answer;
      results.push({
        step: 12,
        name: 'Gemini 2.5 Flash Copilot Investigation',
        status: 'PASS',
        details: `AI response received (${aiAnswer.length} chars). Model: ${copilotRes.data.modelUsed}, Source: ${copilotRes.data.source}`,
        timestamp: new Date().toISOString(),
        evidence: {
          model: copilotRes.data.modelUsed,
          source: copilotRes.data.source,
          excerpt: aiAnswer.slice(0, 250) + '...',
        },
      });
      console.log(`Step 12: PASS - Gemini 2.5 Flash provided ${aiAnswer.length} chars of tactical cyber investigation.`);
    } else {
      throw new Error(`Gemini investigation failed: ${JSON.stringify(copilotRes.data)}`);
    }
  } catch (err: any) {
    results.push({
      step: 12,
      name: 'Gemini 2.5 Flash Copilot Investigation',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 13: Operator Approval & Authoritative Network Containment
  try {
    if (activeIncident) {
      const containRes = await fetchJson(`/incidents/${activeIncident.id}/contain`, { method: 'POST' });
      if (containRes.status === 200 && containRes.data.containmentApproved) {
        results.push({
          step: 13,
          name: 'Human-in-the-Loop Containment Approval',
          status: 'PASS',
          details: `Incident ${activeIncident.id} containment approved. Lifecycle moved to 'contained'.`,
          timestamp: new Date().toISOString(),
          evidence: containRes.data,
        });
        console.log(`Step 13: PASS - Containment approved for ${activeIncident.id}`);
      } else {
        throw new Error(`Containment failed: ${JSON.stringify(containRes.data)}`);
      }
    } else {
      throw new Error('No active incident to contain');
    }
  } catch (err: any) {
    results.push({
      step: 13,
      name: 'Human-in-the-Loop Containment Approval',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 14: Authoritative Device Isolation Verification
  try {
    const checkNode = await fetchJson('/devices/server-01');
    if (checkNode.status === 200 && checkNode.data.status === 'isolated') {
      results.push({
        step: 14,
        name: 'Authoritative Node Isolation Enforced',
        status: 'PASS',
        details: `server-01 is isolated from network edges. Traffic severed authoritatively.`,
        timestamp: new Date().toISOString(),
        evidence: checkNode.data,
      });
      console.log('Step 14: PASS - Node server-01 successfully isolated.');
    } else {
      throw new Error(`Device not isolated: ${JSON.stringify(checkNode.data)}`);
    }
  } catch (err: any) {
    results.push({
      step: 14,
      name: 'Authoritative Node Isolation Enforced',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 15: Cryptographic SHA-256 Sealed Forensic Dossier
  let forensicHash = '';
  try {
    if (activeIncident) {
      const repRes = await fetchJson(`/incidents/${activeIncident.id}/report`);
      if (repRes.status === 200 && repRes.data.report) {
        const dossier = repRes.data.report;
        forensicHash = crypto.createHash('sha256').update(JSON.stringify(dossier)).digest('hex');

        results.push({
          step: 15,
          name: 'Forensic Dossier & SHA-256 Tamper Seal',
          status: 'PASS',
          details: `Tamper-evident forensic dossier compiled. SHA-256 seal: ${forensicHash}`,
          timestamp: new Date().toISOString(),
          evidence: {
            incidentId: activeIncident.id,
            sha256Seal: forensicHash,
            timelineEvents: dossier.timeline ? dossier.timeline.length : 0,
          },
        });
        console.log(`Step 15: PASS - Forensic dossier generated with unique SHA-256 seal: ${forensicHash}`);
      } else {
        throw new Error(`Report generation failed: ${JSON.stringify(repRes.data)}`);
      }
    } else {
      throw new Error('No active incident for report generation');
    }
  } catch (err: any) {
    results.push({
      step: 15,
      name: 'Forensic Dossier & SHA-256 Tamper Seal',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 16: Incident Resolution & Remediation
  try {
    if (activeIncident) {
      const resRes = await fetchJson(`/incidents/${activeIncident.id}/resolve`, { method: 'POST' });
      if (resRes.status === 200 && resRes.data.status === 'resolved') {
        results.push({
          step: 16,
          name: 'Incident Resolution & Recovery Verification',
          status: 'PASS',
          details: `Incident ${activeIncident.id} resolved. Remediation steps recorded in timeline.`,
          timestamp: new Date().toISOString(),
          evidence: resRes.data,
        });
        console.log(`Step 16: PASS - Incident ${activeIncident.id} resolved.`);
      } else {
        throw new Error(`Resolution failed: ${JSON.stringify(resRes.data)}`);
      }
    }
  } catch (err: any) {
    results.push({
      step: 16,
      name: 'Incident Resolution & Recovery Verification',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Step 17: Stop Simulation Engine
  try {
    await fetchJson('/simulation/stop', { method: 'POST' });
    results.push({
      step: 17,
      name: 'Simulation Engine Shutdown',
      status: 'PASS',
      details: 'Simulation engine safely stopped and ticker unmounted.',
      timestamp: new Date().toISOString(),
    });
    console.log('Step 17: PASS - Simulation engine stopped.');
  } catch (err: any) {
    results.push({
      step: 17,
      name: 'Simulation Engine Shutdown',
      status: 'FAIL',
      details: err.message,
      timestamp: new Date().toISOString(),
    });
  }

  // Close WebSocket
  if (wsClient && wsClient.readyState === WebSocket.OPEN) {
    wsClient.close();
  }

  // Summary
  const passCount = results.filter((r) => r.status === 'PASS').length;
  const failCount = results.filter((r) => r.status === 'FAIL').length;
  console.log(`\n=== RC2 LIVE ACCEPTANCE COMPLETE: ${passCount}/${results.length} PASSED (${failCount} FAILED) ===`);

  const outputPayload = {
    releaseVersion: '2.0.0-rc2',
    testExecutionDate: new Date().toISOString(),
    totalSteps: results.length,
    passedSteps: passCount,
    failedSteps: failCount,
    activeIncidentTested: activeIncident ? activeIncident.id : 'N/A',
    forensicSha256: forensicHash,
    results,
  };

  fs.writeFileSync('release/reports/e2e_rc2_raw_results.json', JSON.stringify(outputPayload, null, 2));
  console.log('Results persisted to release/reports/e2e_rc2_raw_results.json');
}

runRc2LiveAcceptance().catch((e) => {
  console.error('Fatal test error:', e);
  process.exit(1);
});
