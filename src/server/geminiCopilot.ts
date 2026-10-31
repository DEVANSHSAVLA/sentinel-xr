import { GoogleGenAI } from '@google/genai';
import { db } from './db';
import { Incident, NetworkNode } from '../types';

let aiClient: GoogleGenAI | null = null;

function getAiClient(): GoogleGenAI | null {
  if (!aiClient && process.env.GEMINI_API_KEY) {
    aiClient = new GoogleGenAI();
  }
  return aiClient;
}

export interface CopilotInvestigationRequest {
  incidentId?: string;
  deviceId?: string;
  userPrompt: string;
}

export interface CopilotInvestigationResponse {
  answer: string;
  source: 'gemini' | 'rule_based_fallback';
  modelUsed: string;
  contextIncident?: Incident;
  contextDevice?: NetworkNode;
  recommendedAction?: string;
  timestamp: string;
}

export async function investigateWithCopilot(
  req: CopilotInvestigationRequest
): Promise<CopilotInvestigationResponse> {
  const timestamp = new Date().toISOString();
  
  // Resolve context
  const targetIncident = req.incidentId ? db.incidents.get(req.incidentId) : Array.from(db.incidents.values())[0];
  const targetDevice = req.deviceId 
    ? db.nodes.get(req.deviceId) 
    : (targetIncident ? db.nodes.get(targetIncident.targetDeviceId) : undefined);

  // Prepare system context prompt
  const topologySummary = Array.from(db.nodes.values())
    .map(n => `- ${n.id} (${n.name}, IP: ${n.ip}, Zone: ${n.zone}, Status: ${n.status}, Risk: ${n.riskScore}/100)`)
    .join('\n');

  const incidentContext = targetIncident ? `
CURRENT INVESTIGATED INCIDENT:
- ID: ${targetIncident.id}
- Title: ${targetIncident.title}
- Scenario: ${targetIncident.scenario} [Severity: ${targetIncident.severity.toUpperCase()}]
- Status: ${targetIncident.status}
- Target Device: ${targetIncident.targetDeviceId}
- Source IP: ${targetIncident.sourceIp}
- Detection Time: ${targetIncident.timestamp}
- Description: ${targetIncident.description}
- Model Confidence: ${targetIncident.confidenceScore}%
- SHAP Feature Attributions:
${targetIncident.shapFactors.map(s => `  * ${s.feature}: ${s.value} (Impact: ${s.impact}, SHAP delta: ${s.contributionScore})`).join('\n')}
- Recommended Action: ${targetIncident.recommendedAction}
- Containment Approved: ${targetIncident.containmentApproved}
` : 'No active incident selected.';

  const deviceContext = targetDevice ? `
CURRENT TARGET DEVICE TELEMETRY:
- Device ID: ${targetDevice.id}
- Name: ${targetDevice.name}
- Type: ${targetDevice.type}
- IP: ${targetDevice.ip}
- Zone: ${targetDevice.zone}
- Status: ${targetDevice.status}
- Risk Score: ${targetDevice.riskScore}/100
- Services: ${targetDevice.services.join(', ')}
- CPU Usage: ${targetDevice.cpuUsage}%
- Memory Usage: ${targetDevice.memoryUsage}%
- Current Traffic: ${targetDevice.trafficRate} pkts/s
` : '';

  const systemInstructions = `
You are the AI Investigation Copilot for SENTINEL-XR, an extended reality cybersecurity operations platform.
You analyze synthetic cyber incidents, explain machine learning model predictions, provide SHAP factor interpretations, evaluate blast radius across the 3D network topology, and recommend human-supervised containment procedures.

CRITICAL RULES:
1. Always ground your analysis strictly in the provided network topology, incident data, and device telemetry.
2. Never hallucinate fake event IDs, non-existent devices, or fictional attack vectors.
3. Explicitly reference Incident IDs (e.g. ${targetIncident ? targetIncident.id : 'N/A'}) and Target Device IDs in your responses.
4. For containment actions, state clearly that simulated actions require human approval before applying quarantine rules.
5. Provide concise, high-clarity answers structured for a SOC Analyst or XR operator.
`;

  const fullPrompt = `${systemInstructions}\n\nNETWORK TOPOLOGY:\n${topologySummary}\n\n${incidentContext}\n\n${deviceContext}\n\nUSER INQUIRY: "${req.userPrompt}"`;

  const client = getAiClient();
  if (client) {
    try {
      const response = await client.models.generateContent({
        model: 'gemini-2.5-flash',
        contents: fullPrompt,
      });

      const text = response.text || '';
      if (text.trim().length > 0) {
        return {
          answer: text,
          source: 'gemini',
          modelUsed: 'gemini-2.5-flash',
          contextIncident: targetIncident,
          contextDevice: targetDevice,
          recommendedAction: targetIncident?.recommendedAction,
          timestamp,
        };
      }
    } catch (err) {
      console.warn('Gemini API call returned error or rate limited, falling back to local expert engine:', err);
    }
  }

  // Local Rule-Based Expert Fallback
  let fallbackAnswer = '';
  const promptLower = req.userPrompt.toLowerCase();

  if (promptLower.includes('why') || promptLower.includes('attack') || promptLower.includes('reason')) {
    fallbackAnswer = `**Incident Analysis for ${targetIncident?.id || 'Active Threat'}:**\n\n` +
      `The detection pipeline observed an anomalous pattern targeting **${targetDevice?.name || targetIncident?.targetDeviceId}** (${targetDevice?.ip}).\n\n` +
      `**Key Contributing SHAP Factors:**\n` +
      (targetIncident?.shapFactors.map(f => `- **${f.feature}**: ${f.value} — *${f.impact} impact* (SHAP attribution +${f.contributionScore})`).join('\n') || '- Traffic deviation from nominal baseline.') +
      `\n\nThe supervised classifier classified this event as **${targetIncident?.scenario || 'Suspicious Traffic'}** with **${targetIncident?.confidenceScore || 92}% confidence**.`;
  } else if (promptLower.includes('contain') || promptLower.includes('isolate') || promptLower.includes('action') || promptLower.includes('respond')) {
    fallbackAnswer = `**Recommended Response for ${targetIncident?.id || 'Threat'}:**\n\n` +
      `Action: **${targetIncident?.recommendedAction || 'Isolate target device from DMZ perimeter ingress.'}**\n\n` +
      `*Safety Note:* In accordance with SENTINEL-XR containment policy, human authorization is required. You can approve containment in the Incident Management dashboard or via the VR Operations Center.`;
  } else if (promptLower.includes('device') || promptLower.includes('server') || promptLower.includes('status')) {
    fallbackAnswer = `**Device Telemetry Summary for ${targetDevice?.name || 'Target Node'}:**\n\n` +
      `- **IP Address:** \`${targetDevice?.ip}\`\n` +
      `- **Zone:** \`${targetDevice?.zone}\`\n` +
      `- **Operational Status:** \`${targetDevice?.status.toUpperCase()}\`\n` +
      `- **Current Risk Score:** **${targetDevice?.riskScore}/100**\n` +
      `- **Active Services:** ${targetDevice?.services.join(', ')}\n` +
      `- **Utilization:** CPU ${targetDevice?.cpuUsage}%, Memory ${targetDevice?.memoryUsage}%, Traffic ${targetDevice?.trafficRate} pkts/s.`;
  } else {
    fallbackAnswer = `**SENTINEL-XR Intelligence Briefing:**\n\n` +
      `Monitoring **${db.nodes.size} network nodes** across external, DMZ, and secure core zones.\n` +
      `Currently tracking **${db.incidents.size} incident(s)**, including **${targetIncident?.id}** (${targetIncident?.title}) at **${targetIncident?.severity.toUpperCase()}** severity.\n\n` +
      `Recommended next step: review incident evidence, inspect 3D blast radius in the topology, and approve containment if necessary.`;
  }

  return {
    answer: fallbackAnswer,
    source: 'rule_based_fallback',
    modelUsed: 'sentinel-expert-fallback-v1',
    contextIncident: targetIncident,
    contextDevice: targetDevice,
    recommendedAction: targetIncident?.recommendedAction,
    timestamp,
  };
}
