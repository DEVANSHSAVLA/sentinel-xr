import { Incident, IncidentStatus, IncidentTimelineEvent } from '../types';

export function advanceIncidentStatus(
  incident: Incident,
  targetStatus: IncidentStatus,
  actor: IncidentTimelineEvent['actor'],
  description: string
): Incident {
  const now = new Date().toISOString();
  const updatedTimeline = [
    ...incident.timeline,
    {
      time: now,
      stage: targetStatus,
      description,
      actor,
    },
  ];

  const updated: Incident = {
    ...incident,
    status: targetStatus,
    timeline: updatedTimeline,
  };

  if (targetStatus === 'contained') {
    updated.containmentApproved = true;
    updated.containedAt = now;
  }

  if (targetStatus === 'resolved') {
    updated.resolvedAt = now;
  }

  return updated;
}

export function generateForensicReport(incident: Incident): string {
  return [
    `============================================================`,
    `SENTINEL-XR CYBER INCIDENT FORENSIC DOSSIER`,
    `============================================================`,
    `Incident ID:        ${incident.id}`,
    `Classification:     ${incident.scenario} [Severity: ${incident.severity.toUpperCase()}]`,
    `Detection Time:     ${incident.timestamp}`,
    `Status:             ${incident.status.toUpperCase()}`,
    `Target Device:      ${incident.targetDeviceId}`,
    `Offending Source:   ${incident.sourceIp}`,
    `Model Confidence:   ${incident.confidenceScore}%`,
    `Containment Time:   ${incident.containedAt || 'N/A (Active/Pending)'}`,
    `Resolution Time:    ${incident.resolvedAt || 'N/A (Unresolved)'}`,
    ``,
    `INCIDENT SUMMARY:`,
    incident.description,
    ``,
    `SHAP ATTRIBUTION FACTORS (EXPLAINABLE AI):`,
    ...incident.shapFactors.map(
      (f) => `  - ${f.feature}: ${f.value} [${f.impact} Impact | SHAP delta: ${f.contributionScore > 0 ? '+' : ''}${f.contributionScore}]`
    ),
    ``,
    `RECOMMENDED REMEDIATION:`,
    incident.recommendedAction,
    ``,
    `FORENSIC TIMELINE & AUDIT TRAIL:`,
    ...incident.timeline.map(
      (t) => `  [${t.time}] [${t.stage.toUpperCase()}] by ${t.actor}: ${t.description}`
    ),
    `============================================================`,
  ].join('\n');
}
