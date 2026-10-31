import { SecurityEvent, SimulationScenarioId, Severity } from '../types';
import { RawTelemetryInput, classifyTelemetry } from './mlMath';

export interface SimulationStepResult {
  event: SecurityEvent;
  telemetry: RawTelemetryInput;
  createdIncident: boolean;
  incidentPayload?: {
    title: string;
    scenario: 'DDoS' | 'Brute-Force' | 'Port-Scan' | 'Lateral Movement';
    severity: Severity;
    targetDeviceId: string;
    affectedNodes: string[];
    sourceIp: string;
    description: string;
    recommendedAction: string;
  };
}

let eventCounter = 1000;

export function generateSyntheticEvent(
  scenario: SimulationScenarioId,
  targetDeviceId: string,
  targetIp: string,
  intensity: 'low' | 'medium' | 'high' | 'extreme'
): SimulationStepResult {
  eventCounter++;
  const id = `EVT-${Date.now().toString().slice(-6)}-${eventCounter}`;
  const timestamp = new Date().toISOString();

  let telemetry: RawTelemetryInput;
  let sourceIp = '198.51.100.45';
  let destPort = 443;
  let protocol = 'HTTPS';
  let rawPayloadSummary = 'TLS Client Hello';

  const intensityMultiplier = 
    intensity === 'low' ? 1 :
    intensity === 'medium' ? 2.5 :
    intensity === 'high' ? 5 : 10;

  switch (scenario) {
    case 'ddos': {
      sourceIp = `198.51.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}`;
      destPort = 80;
      protocol = 'TCP';
      const packetRate = Math.round((2500 + Math.random() * 4000) * intensityMultiplier);
      const uniqueSources = Math.round(80 * intensityMultiplier);
      telemetry = {
        packetRate,
        bytesPerSec: packetRate * 1024,
        uniqueSources,
        failedAuthAttempts: 0,
        destPort,
        protocol,
        sourceInternal: false,
        targetInternal: true,
      };
      rawPayloadSummary = `SYN flood volumetric surge targeting ${targetDeviceId} (${targetIp}:80)`;
      break;
    }
    case 'bruteforce': {
      sourceIp = '203.0.113.89';
      destPort = 22;
      protocol = 'SSH';
      const failedAttempts = Math.round((6 + Math.random() * 14) * (intensity === 'low' ? 1 : 2));
      telemetry = {
        packetRate: 120,
        bytesPerSec: 18000,
        uniqueSources: 1,
        failedAuthAttempts: failedAttempts,
        destPort,
        protocol,
        sourceInternal: false,
        targetInternal: true,
      };
      rawPayloadSummary = `Repeated auth failure (user 'root', 'admin') sequence against SSH daemon`;
      break;
    }
    case 'portscan': {
      sourceIp = '198.18.0.15';
      destPort = Math.floor(Math.random() * 10000) + 1;
      protocol = 'TCP';
      telemetry = {
        packetRate: Math.round(450 * (intensity === 'low' ? 1 : 2.5)),
        bytesPerSec: 32000,
        uniqueSources: 1,
        failedAuthAttempts: 0,
        destPort,
        protocol,
        sourceInternal: false,
        targetInternal: true,
      };
      rawPayloadSummary = `SYN stealth scan probing ports ${destPort}, ${destPort + 1}, ${destPort + 2}`;
      break;
    }
    case 'lateral': {
      sourceIp = '10.0.3.14'; // internal workstation
      destPort = 445; // SMB
      protocol = 'TCP';
      telemetry = {
        packetRate: 350,
        bytesPerSec: 95000,
        uniqueSources: 1,
        failedAuthAttempts: 4,
        destPort,
        protocol,
        sourceInternal: true,
        targetInternal: true,
      };
      rawPayloadSummary = `Suspicious internal SMB session traversal to database node from compromised workstation`;
      break;
    }
  }

  const mlResult = classifyTelemetry(telemetry);

  const event: SecurityEvent = {
    id,
    timestamp,
    sourceIp,
    destIp: targetIp,
    destPort,
    protocol,
    packetRate: telemetry.packetRate,
    bytesPerSec: telemetry.bytesPerSec,
    uniqueSources: telemetry.uniqueSources,
    failedAuthAttempts: telemetry.failedAuthAttempts,
    anomalyScore: mlResult.anomalyScore,
    predictedAttackType: mlResult.predictedType,
    severity: mlResult.severity,
    targetDeviceId,
    rawPayloadSummary,
    shapFactors: mlResult.shapFactors,
  };

  const isSuspicious = mlResult.predictedType !== 'Normal';
  let incidentPayload;

  if (isSuspicious) {
    const scenarioTitleMap = {
      ddos: 'DDoS Volumetric Inundation Attack',
      bruteforce: 'Credential Stuffing & SSH Brute-Force Wave',
      portscan: 'Horizontal Subnet Port Reconnaissance',
      lateral: 'Active Directory Lateral Movement Pivot',
    };

    const actionMap = {
      ddos: `Deploy rate-limiting scrub rule and temporarily isolate external ingress to ${targetDeviceId}`,
      bruteforce: `Blacklist source IP ${sourceIp} at perimeter firewall and force MFA re-auth on target`,
      portscan: `Apply firewall drop rule for ${sourceIp} and silence open port responses`,
      lateral: `Quarantine internal node ${targetDeviceId} and invalidate compromised Kerberos ticket grants`,
    };

    incidentPayload = {
      title: scenarioTitleMap[scenario],
      scenario: (scenario === 'ddos' ? 'DDoS' :
                 scenario === 'bruteforce' ? 'Brute-Force' :
                 scenario === 'portscan' ? 'Port-Scan' : 'Lateral Movement') as any,
      severity: mlResult.severity,
      targetDeviceId,
      affectedNodes: [targetDeviceId],
      sourceIp,
      description: `ML detection engine flagged anomalous traffic profile: ${rawPayloadSummary}. Anomaly score ${mlResult.anomalyScore.toFixed(2)}, confidence ${mlResult.confidence}%.`,
      recommendedAction: actionMap[scenario],
    };
  }

  return {
    event,
    telemetry,
    createdIncident: isSuspicious,
    incidentPayload,
  };
}
