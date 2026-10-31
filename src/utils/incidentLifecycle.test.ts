import test from 'node:test';
import assert from 'node:assert/strict';
import { advanceIncidentStatus, generateForensicReport } from './incidentLifecycle';
import { Incident } from '../types';

const mockIncident: Incident = {
  id: 'INC-2026-001',
  title: 'DDoS Volumetric Inundation',
  severity: 'critical',
  status: 'detected',
  scenario: 'DDoS',
  targetDeviceId: 'server-01',
  affectedNodes: ['server-01'],
  sourceIp: '198.51.100.88',
  timestamp: new Date().toISOString(),
  description: 'Anomalous traffic surge detected',
  confidenceScore: 94,
  shapFactors: [
    {
      feature: 'Packet Rate',
      value: '12,500 pkts/s',
      impact: 'Severe',
      contributionScore: 0.55,
    },
  ],
  recommendedAction: 'Isolate server-01 from DMZ ingress',
  containmentApproved: false,
  timeline: [
    {
      time: new Date().toISOString(),
      stage: 'detected',
      description: 'Incident detected by ML engine',
      actor: 'AI Detector',
    },
  ],
};

test('advanceIncidentStatus updates status and records timeline', () => {
  const contained = advanceIncidentStatus(
    mockIncident,
    'contained',
    'Security Analyst',
    'Applied simulated quarantine to server-01'
  );
  assert.equal(contained.status, 'contained');
  assert.equal(contained.containmentApproved, true);
  assert.ok(contained.containedAt);
  assert.equal(contained.timeline.length, 2);
  assert.equal(contained.timeline[1].actor, 'Security Analyst');
});

test('generateForensicReport formats complete audit dossier', () => {
  const report = generateForensicReport(mockIncident);
  assert.ok(report.includes('INC-2026-001'));
  assert.ok(report.includes('SHAP ATTRIBUTION FACTORS'));
  assert.ok(report.includes('server-01'));
});
