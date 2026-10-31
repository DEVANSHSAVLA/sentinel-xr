export type Severity = 'low' | 'medium' | 'high' | 'critical';

export type IncidentStatus = 
  | 'detected' 
  | 'investigating' 
  | 'assessing_risk' 
  | 'response_recommended' 
  | 'pending_approval' 
  | 'contained' 
  | 'verifying' 
  | 'resolved';

export type NodeType = 
  | 'gateway' 
  | 'firewall' 
  | 'router' 
  | 'server' 
  | 'database' 
  | 'workstation' 
  | 'api_gateway' 
  | 'client';

export type NodeStatus = 'healthy' | 'suspicious' | 'critical' | 'isolated';

export interface NetworkNode {
  id: string;
  name: string;
  type: NodeType;
  ip: string;
  status: NodeStatus;
  riskScore: number; // 0 - 100
  zone: 'external' | 'perimeter' | 'dmz' | 'internal' | 'secure_core';
  position: [number, number, number];
  services: string[];
  cpuUsage: number; // %
  memoryUsage: number; // %
  trafficRate: number; // packets/sec
}

export interface NetworkConnection {
  id: string;
  source: string;
  target: string;
  protocol: 'TCP' | 'UDP' | 'HTTP' | 'HTTPS' | 'SSH' | 'DNS' | 'ICMP';
  port: number;
  status: 'normal' | 'suspicious' | 'under_attack';
  bandwidthKbps: number;
}

export interface SecurityEvent {
  id: string;
  timestamp: string;
  sourceIp: string;
  destIp: string;
  destPort: number;
  protocol: string;
  packetRate: number;
  bytesPerSec: number;
  uniqueSources: number;
  failedAuthAttempts: number;
  anomalyScore: number; // 0.0 to 1.0
  predictedAttackType: 'Normal' | 'DDoS' | 'Brute-Force' | 'Port-Scan' | 'Lateral Movement';
  severity: Severity;
  targetDeviceId?: string;
  rawPayloadSummary: string;
  shapFactors?: ShapFactor[];
}

export interface ShapFactor {
  feature: string;
  value: string;
  impact: 'High' | 'Moderate' | 'Severe' | 'Elevated';
  contributionScore: number; // e.g. +0.42
}

export interface IncidentTimelineEvent {
  time: string;
  stage: string;
  description: string;
  actor: 'AI Detector' | 'Security Analyst' | 'Automated Containment' | 'Simulation Engine';
}

export interface Incident {
  id: string;
  title: string;
  severity: Severity;
  status: IncidentStatus;
  scenario: 'DDoS' | 'Brute-Force' | 'Port-Scan' | 'Lateral Movement';
  targetDeviceId: string;
  affectedNodes: string[];
  sourceIp: string;
  timestamp: string;
  description: string;
  confidenceScore: number; // 0 - 100
  shapFactors: ShapFactor[];
  recommendedAction: string;
  containmentApproved: boolean;
  containedAt?: string;
  resolvedAt?: string;
  timeline: IncidentTimelineEvent[];
  forensicNotes?: string;
}

export type SimulationScenarioId = 'ddos' | 'bruteforce' | 'portscan' | 'lateral';

export interface SimulationConfig {
  scenarioId: SimulationScenarioId;
  targetDeviceId: string;
  intensity: 'low' | 'medium' | 'high' | 'extreme';
  durationSeconds: number;
}

export interface SimulationState {
  isActive: boolean;
  isPaused: boolean;
  currentScenario: SimulationScenarioId | null;
  targetDeviceId: string | null;
  intensity: 'low' | 'medium' | 'high' | 'extreme';
  elapsedSeconds: number;
  totalEventsGenerated: number;
  activeIncidentsGenerated: number;
}

export interface UserProfile {
  id: string;
  username: string;
  name: string;
  role: 'Administrator' | 'Security Analyst' | 'Viewer';
  token: string;
}

export interface XrSessionStatus {
  arConnected: boolean;
  vrConnected: boolean;
  lastSyncTimestamp: string;
  activeIncidentId: string | null;
  trackingMode: 'ARCore Plane / Marker Tracking' | 'OpenXR 6DoF Spatial Room';
  activeSpatialAnchor: string;
  fpsAverage: number;
}

export interface SystemAnalytics {
  totalDevices: number;
  onlineDevices: number;
  isolatedDevices: number;
  activeIncidents: number;
  criticalIncidents: number;
  networkRiskIndex: number; // 0 - 100
  trafficPacketsPerSec: number;
  detectionAccuracy: number;
  avgResolutionTimeMinutes: number;
  threatDistribution: {
    name: string;
    count: number;
    color: string;
  }[];
}
