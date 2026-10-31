import { NetworkNode, NetworkConnection, SecurityEvent, Incident, UserProfile, XrSessionStatus, SimulationState } from '../types';

export class SentinelDatabase {
  public nodes: Map<string, NetworkNode> = new Map();
  public connections: NetworkConnection[] = [];
  public events: SecurityEvent[] = [];
  public incidents: Map<string, Incident> = new Map();
  public xrStatus: XrSessionStatus = {
    arConnected: false,
    vrConnected: false,
    lastSyncTimestamp: new Date().toISOString(),
    activeIncidentId: null,
    trackingMode: 'ARCore Plane / Marker Tracking',
    activeSpatialAnchor: 'ANCHOR_SENTINEL_SOC_ORIGIN',
    fpsAverage: 72,
  };
  public simulation: SimulationState = {
    isActive: false,
    isPaused: false,
    currentScenario: null,
    targetDeviceId: null,
    intensity: 'medium',
    elapsedSeconds: 0,
    totalEventsGenerated: 0,
    activeIncidentsGenerated: 0,
  };
  public users: UserProfile[] = [
    {
      id: 'USR-ADMIN',
      username: 'admin',
      name: 'Sarah Chen (Lead SecOps Architect)',
      role: 'Administrator',
      token: 'tok-admin-sentinel-2026',
    },
    {
      id: 'USR-ANALYST',
      username: 'analyst',
      name: 'Alex Rivera (Senior Cyber Threat Analyst)',
      role: 'Security Analyst',
      token: 'tok-analyst-sentinel-2026',
    },
    {
      id: 'USR-VIEWER',
      username: 'viewer',
      name: 'Marcus Vance (Executive Oversight)',
      role: 'Viewer',
      token: 'tok-viewer-sentinel-2026',
    },
  ];

  constructor() {
    this.seedTopology();
    this.seedSampleIncidents();
  }

  private seedTopology() {
    const initialNodes: NetworkNode[] = [
      {
        id: 'gateway-01',
        name: 'Internet Gateway',
        type: 'gateway',
        ip: '198.51.100.1',
        status: 'healthy',
        riskScore: 12,
        zone: 'external',
        position: [-6, 2, 0],
        services: ['BGP', 'WAN Ingress', 'DDoS Scrubbing'],
        cpuUsage: 24,
        memoryUsage: 35,
        trafficRate: 850,
      },
      {
        id: 'firewall-01',
        name: 'Perimeter NextGen Firewall',
        type: 'firewall',
        ip: '10.0.0.1',
        status: 'healthy',
        riskScore: 18,
        zone: 'perimeter',
        position: [-3, 1, 0],
        services: ['Stateful Inspection', 'IPS/IDS', 'TLS Decryption'],
        cpuUsage: 42,
        memoryUsage: 55,
        trafficRate: 820,
      },
      {
        id: 'router-core',
        name: 'Core Distribution Router',
        type: 'router',
        ip: '10.0.0.2',
        status: 'healthy',
        riskScore: 15,
        zone: 'internal',
        position: [0, 0, 0],
        services: ['OSPF', 'VLAN Trunking', 'QoS'],
        cpuUsage: 31,
        memoryUsage: 40,
        trafficRate: 1450,
      },
      {
        id: 'server-01',
        name: 'DMZ Web App Cluster',
        type: 'server',
        ip: '10.0.1.10',
        status: 'healthy',
        riskScore: 22,
        zone: 'dmz',
        position: [3, 2, -2],
        services: ['HTTPS (443)', 'Nginx 1.24', 'React Frontend SSR'],
        cpuUsage: 38,
        memoryUsage: 62,
        trafficRate: 410,
      },
      {
        id: 'server-02',
        name: 'DMZ API Gateway',
        type: 'api_gateway',
        ip: '10.0.1.15',
        status: 'healthy',
        riskScore: 25,
        zone: 'dmz',
        position: [3, 0, -2],
        services: ['REST API', 'GraphQL', 'OAuth Service'],
        cpuUsage: 45,
        memoryUsage: 58,
        trafficRate: 530,
      },
      {
        id: 'db-cluster-01',
        name: 'Secure Vault Database Cluster',
        type: 'database',
        ip: '10.0.2.20',
        status: 'healthy',
        riskScore: 10,
        zone: 'secure_core',
        position: [6, 1, -1],
        services: ['PostgreSQL 16 High-Avail', 'Redis Cache', 'AES-256 Storage'],
        cpuUsage: 28,
        memoryUsage: 71,
        trafficRate: 290,
      },
      {
        id: 'workstation-01',
        name: 'SOC Analyst Workstation',
        type: 'workstation',
        ip: '10.0.3.14',
        status: 'healthy',
        riskScore: 14,
        zone: 'internal',
        position: [1, -2, 3],
        services: ['EDR Sentinel Agent', 'XR Mirror Client', 'SSH Client'],
        cpuUsage: 21,
        memoryUsage: 46,
        trafficRate: 65,
      },
      {
        id: 'workstation-02',
        name: 'DevOps Engineering Terminal',
        type: 'workstation',
        ip: '10.0.3.18',
        status: 'healthy',
        riskScore: 16,
        zone: 'internal',
        position: [-2, -2, 3],
        services: ['Docker Daemon', 'Kubectl', 'CI Runner'],
        cpuUsage: 33,
        memoryUsage: 50,
        trafficRate: 90,
      },
      {
        id: 'client-pool',
        name: 'Simulated Client Subnet',
        type: 'client',
        ip: '10.0.4.0/24',
        status: 'healthy',
        riskScore: 8,
        zone: 'internal',
        position: [-4, -1, 4],
        services: ['DHCP Client', 'Browser Telemetry', 'VPN Pool'],
        cpuUsage: 15,
        memoryUsage: 30,
        trafficRate: 180,
      },
    ];

    for (const node of initialNodes) {
      this.nodes.set(node.id, node);
    }

    this.connections = [
      { id: 'conn-1', source: 'gateway-01', target: 'firewall-01', protocol: 'HTTPS', port: 443, status: 'normal', bandwidthKbps: 4500 },
      { id: 'conn-2', source: 'firewall-01', target: 'router-core', protocol: 'TCP', port: 80, status: 'normal', bandwidthKbps: 4200 },
      { id: 'conn-3', source: 'router-core', target: 'server-01', protocol: 'HTTPS', port: 443, status: 'normal', bandwidthKbps: 2100 },
      { id: 'conn-4', source: 'router-core', target: 'server-02', protocol: 'HTTP', port: 8080, status: 'normal', bandwidthKbps: 2800 },
      { id: 'conn-5', source: 'server-01', target: 'db-cluster-01', protocol: 'TCP', port: 5432, status: 'normal', bandwidthKbps: 1200 },
      { id: 'conn-6', source: 'server-02', target: 'db-cluster-01', protocol: 'TCP', port: 5432, status: 'normal', bandwidthKbps: 1800 },
      { id: 'conn-7', source: 'router-core', target: 'workstation-01', protocol: 'SSH', port: 22, status: 'normal', bandwidthKbps: 350 },
      { id: 'conn-8', source: 'router-core', target: 'workstation-02', protocol: 'TCP', port: 2376, status: 'normal', bandwidthKbps: 480 },
      { id: 'conn-9', source: 'router-core', target: 'client-pool', protocol: 'HTTP', port: 80, status: 'normal', bandwidthKbps: 950 },
    ];
  }

  private seedSampleIncidents() {
    const initialIncident: Incident = {
      id: 'INC-7041',
      title: 'Credential Stuffing & SSH Wave against DMZ API',
      severity: 'high',
      status: 'pending_approval',
      scenario: 'Brute-Force',
      targetDeviceId: 'server-02',
      affectedNodes: ['server-02'],
      sourceIp: '203.0.113.89',
      timestamp: new Date(Date.now() - 1000 * 60 * 18).toISOString(),
      description: 'Distributed automated SSH credential dictionary attack originating from external proxy IP targeting API authentication endpoint.',
      confidenceScore: 93,
      shapFactors: [
        { feature: 'Failed Auth Velocity', value: '28 failures/min', impact: 'Severe', contributionScore: 0.54 },
        { feature: 'Target Port Focus', value: 'Port 22 (SSH)', impact: 'High', contributionScore: 0.29 },
        { feature: 'Client User Agent Anomaly', value: 'libssh / paramiko fingerprint', impact: 'Moderate', contributionScore: 0.17 },
      ],
      recommendedAction: 'Blacklist source IP 203.0.113.89 at perimeter firewall and force MFA re-authentication for all active API administrator tokens.',
      containmentApproved: false,
      timeline: [
        {
          time: new Date(Date.now() - 1000 * 60 * 18).toISOString(),
          stage: 'detected',
          description: 'Anomaly score spike (0.88) triggered alert on server-02 auth daemon.',
          actor: 'AI Detector',
        },
        {
          time: new Date(Date.now() - 1000 * 60 * 15).toISOString(),
          stage: 'investigating',
          description: 'Automated correlation verified 84 failed root and service account login attempts within 3 minutes.',
          actor: 'Simulation Engine',
        },
        {
          time: new Date(Date.now() - 1000 * 60 * 10).toISOString(),
          stage: 'response_recommended',
          description: 'AI Copilot generated recommended containment rule: Perimeter firewall drop rule.',
          actor: 'AI Detector',
        },
      ],
    };

    this.incidents.set(initialIncident.id, initialIncident);
  }

  public isolateDevice(id: string, isolated: boolean): boolean {
    const node = this.nodes.get(id);
    if (!node) return false;

    node.status = isolated ? 'isolated' : 'healthy';
    node.riskScore = isolated ? 15 : node.riskScore;

    // Update connected edges
    for (const conn of this.connections) {
      if (conn.source === id || conn.target === id) {
        conn.status = isolated ? 'suspicious' : 'normal';
      }
    }
    return true;
  }
}

export const db = new SentinelDatabase();
