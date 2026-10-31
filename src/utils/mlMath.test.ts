import test from 'node:test';
import assert from 'node:assert/strict';
import { calculateAnomalyScore, classifyTelemetry, computeRiskIndex } from './mlMath';

test('calculateAnomalyScore produces low score for nominal traffic', () => {
  const normalInput = {
    packetRate: 65,
    bytesPerSec: 12000,
    uniqueSources: 2,
    failedAuthAttempts: 0,
    destPort: 443,
    protocol: 'HTTPS',
    sourceInternal: false,
    targetInternal: true,
  };
  const score = calculateAnomalyScore(normalInput);
  assert.ok(score <= 0.15, `Expected score <= 0.15 but got ${score}`);
});

test('calculateAnomalyScore detects massive DDoS spike', () => {
  const ddosInput = {
    packetRate: 8500,
    bytesPerSec: 45000000,
    uniqueSources: 340,
    failedAuthAttempts: 0,
    destPort: 80,
    protocol: 'TCP',
    sourceInternal: false,
    targetInternal: true,
  };
  const score = calculateAnomalyScore(ddosInput);
  assert.ok(score >= 0.85, `Expected high score >= 0.85 for DDoS, got ${score}`);
});

test('classifyTelemetry identifies Brute-Force and provides SHAP factors', () => {
  const bruteForceInput = {
    packetRate: 80,
    bytesPerSec: 15000,
    uniqueSources: 1,
    failedAuthAttempts: 18,
    destPort: 22,
    protocol: 'SSH',
    sourceInternal: false,
    targetInternal: true,
  };
  const result = classifyTelemetry(bruteForceInput);
  assert.equal(result.predictedType, 'Brute-Force');
  assert.equal(result.severity, 'critical');
  assert.ok(result.shapFactors.length > 0);
  assert.ok(result.shapFactors.some(f => f.feature === 'Failed Auth Velocity'));
});

test('computeRiskIndex handles normal and escalated threat scenarios', () => {
  const baselineRisk = computeRiskIndex(10, 0, 0, 0, 0.05);
  assert.ok(baselineRisk <= 25, `Expected baseline risk <= 25, got ${baselineRisk}`);

  const elevatedRisk = computeRiskIndex(10, 0, 2, 1, 0.85);
  assert.ok(elevatedRisk >= 75, `Expected elevated risk >= 75, got ${elevatedRisk}`);
});
