import test from 'node:test';
import assert from 'node:assert/strict';
import { generateSyntheticEvent } from './simulationEngine';

test('generateSyntheticEvent creates valid DDoS event and triggers incident payload', () => {
  const result = generateSyntheticEvent('ddos', 'server-01', '10.0.1.10', 'high');
  assert.equal(result.event.predictedAttackType, 'DDoS');
  assert.equal(result.event.severity, 'critical');
  assert.equal(result.createdIncident, true);
  assert.ok(result.incidentPayload);
  assert.equal(result.incidentPayload?.targetDeviceId, 'server-01');
  assert.ok(result.event.packetRate > 10000);
});

test('generateSyntheticEvent handles brute force and lateral movement scenarios', () => {
  const bfResult = generateSyntheticEvent('bruteforce', 'auth-01', '10.0.1.15', 'medium');
  assert.equal(bfResult.event.predictedAttackType, 'Brute-Force');
  assert.ok(bfResult.event.failedAuthAttempts >= 5);

  const lateralResult = generateSyntheticEvent('lateral', 'db-cluster-01', '10.0.2.20', 'low');
  assert.equal(lateralResult.event.predictedAttackType, 'Lateral Movement');
  assert.equal(lateralResult.event.sourceIp, '10.0.3.14');
});
