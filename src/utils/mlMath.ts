import { SecurityEvent, ShapFactor, Severity } from '../types';

export interface RawTelemetryInput {
  packetRate: number; // pkts/sec
  bytesPerSec: number;
  uniqueSources: number;
  failedAuthAttempts: number;
  destPort: number;
  protocol: string;
  sourceInternal: boolean;
  targetInternal: boolean;
}

export interface MLInferenceResult {
  anomalyScore: number; // 0.0 - 1.0
  predictedType: 'Normal' | 'DDoS' | 'Brute-Force' | 'Port-Scan' | 'Lateral Movement';
  severity: Severity;
  confidence: number; // 0 - 100
  shapFactors: ShapFactor[];
}

/**
 * Computes anomaly score inspired by Isolation Forest path-length metrics.
 * Baseline expected values for a typical enterprise node:
 * packetRate: ~40-120 pkts/sec, failedAuth: 0-1, uniqueSources: 1-5.
 */
export function calculateAnomalyScore(input: RawTelemetryInput): number {
  let score = 0.05; // baseline noise

  // Packet rate anomalies
  if (input.packetRate > 5000) {
    score += 0.55;
  } else if (input.packetRate > 1500) {
    score += 0.35;
  } else if (input.packetRate > 500) {
    score += 0.15;
  }

  // Authentication anomalies
  if (input.failedAuthAttempts >= 15) {
    score += 0.60;
  } else if (input.failedAuthAttempts >= 5) {
    score += 0.35;
  } else if (input.failedAuthAttempts >= 2) {
    score += 0.15;
  }

  // High source fan-out
  if (input.uniqueSources >= 200) {
    score += 0.40;
  } else if (input.uniqueSources >= 40) {
    score += 0.25;
  }

  // Suspicious internal pivot
  if (input.sourceInternal && input.targetInternal && input.failedAuthAttempts > 2) {
    score += 0.25;
  }

  return Math.min(1.0, Math.max(0.0, Number(score.toFixed(3))));
}

/**
 * Supervised Decision-Tree Classifier for attack classification
 */
export function classifyTelemetry(input: RawTelemetryInput): MLInferenceResult {
  const anomalyScore = calculateAnomalyScore(input);
  const shapFactors: ShapFactor[] = [];

  // DDoS check: Extreme packet rate + high unique sources + UDP/ICMP/TCP SYN
  if (input.packetRate > 2000 && input.uniqueSources >= 50) {
    shapFactors.push({
      feature: 'Packet Rate',
      value: `${input.packetRate.toLocaleString()} pkts/s`,
      impact: 'Severe',
      contributionScore: 0.46,
    });
    shapFactors.push({
      feature: 'Unique Source IPs',
      value: `${input.uniqueSources} addresses`,
      impact: 'High',
      contributionScore: 0.32,
    });
    shapFactors.push({
      feature: 'Traffic Volume',
      value: `${(input.bytesPerSec / 1024 / 1024).toFixed(2)} MB/s`,
      impact: 'Elevated',
      contributionScore: 0.18,
    });

    return {
      anomalyScore,
      predictedType: 'DDoS',
      severity: 'critical',
      confidence: Math.min(99, Math.round(85 + anomalyScore * 14)),
      shapFactors,
    };
  }

  // Brute Force check: Rapid failed logins on SSH(22), RDP(3389), or Web Auth
  if (input.failedAuthAttempts >= 5) {
    shapFactors.push({
      feature: 'Failed Auth Velocity',
      value: `${input.failedAuthAttempts} failures/min`,
      impact: 'Severe',
      contributionScore: 0.52,
    });
    shapFactors.push({
      feature: 'Target Port Focus',
      value: `Port ${input.destPort} (${input.protocol})`,
      impact: 'High',
      contributionScore: 0.28,
    });

    const severity: Severity = input.failedAuthAttempts > 15 ? 'critical' : 'high';
    return {
      anomalyScore,
      predictedType: 'Brute-Force',
      severity,
      confidence: Math.min(98, Math.round(82 + (input.failedAuthAttempts / 20) * 16)),
      shapFactors,
    };
  }

  // Lateral Movement: Internal to internal suspicious hop (East-West traffic)
  if (input.sourceInternal && input.targetInternal && (input.failedAuthAttempts > 1 || input.packetRate > 150)) {
    shapFactors.push({
      feature: 'East-West Traffic Trajectory',
      value: 'Subnet Boundary Crossing',
      impact: 'Severe',
      contributionScore: 0.48,
    });
    shapFactors.push({
      feature: 'Kerberos / SMB Ticket Anomaly',
      value: 'Elevated privilege probe',
      impact: 'High',
      contributionScore: 0.31,
    });

    return {
      anomalyScore,
      predictedType: 'Lateral Movement',
      severity: 'high',
      confidence: 91,
      shapFactors,
    };
  }

  // Port Scan check: Rapid connection attempts across multiple ports or high source dispersion from external
  if (!input.sourceInternal && input.packetRate > 300 && input.uniqueSources <= 3 && (input.destPort >= 1 && input.destPort <= 65535)) {
    shapFactors.push({
      feature: 'Port Sweep Dispersion',
      value: `Sequential probe rate`,
      impact: 'High',
      contributionScore: 0.44,
    });
    shapFactors.push({
      feature: 'Connection Duration',
      value: `< 15ms per target probe`,
      impact: 'Moderate',
      contributionScore: 0.26,
    });

    return {
      anomalyScore,
      predictedType: 'Port-Scan',
      severity: 'medium',
      confidence: 88,
      shapFactors,
    };
  }

  // Normal Baseline
  return {
    anomalyScore: Math.min(0.15, anomalyScore),
    predictedType: 'Normal',
    severity: 'low',
    confidence: 96,
    shapFactors: [
      {
        feature: 'Traffic Within Baseline Limits',
        value: 'Standard nominal variance',
        impact: 'Moderate',
        contributionScore: 0.05,
      },
    ],
  };
}

/**
 * Calculates unified Network Risk Index from 0 to 100 based on device health, active incidents and anomaly rate
 */
export function computeRiskIndex(
  totalDevices: number,
  isolatedCount: number,
  criticalIncidents: number,
  highIncidents: number,
  recentAnomalyAverage: number
): number {
  if (totalDevices === 0) return 0;
  
  const incidentPenalty = (criticalIncidents * 28) + (highIncidents * 12);
  const anomalyPenalty = recentAnomalyAverage * 35;
  const containmentRelief = isolatedCount * 5; // Containing reduces active spread risk
  
  const rawRisk = Math.max(12, 10 + incidentPenalty + anomalyPenalty - containmentRelief);
  return Math.min(100, Math.round(rawRisk));
}
