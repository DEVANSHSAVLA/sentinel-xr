import React from 'react';
import { NetworkNode, Incident, SecurityEvent, SystemAnalytics } from '../types';
import { Shield, ShieldAlert, Cpu, AlertTriangle, ArrowUpRight, Zap, CheckCircle2 } from 'lucide-react';

interface DashboardViewProps {
  analytics: SystemAnalytics;
  incidents: Incident[];
  recentEvents: SecurityEvent[];
  nodes: NetworkNode[];
  onSelectIncident: (incident: Incident) => void;
  onNavigateTab: (tab: string) => void;
}

export const DashboardView: React.FC<DashboardViewProps> = ({
  analytics,
  incidents,
  recentEvents,
  nodes,
  onSelectIncident,
  onNavigateTab,
}) => {
  const activeIncidents = incidents.filter((i) => i.status !== 'resolved');

  return (
    <div className="space-y-6">
      {/* KPI Cards Row */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3.5">
        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">TOTAL DEVICES</span>
              <div className="flex items-baseline justify-between">
                <span className="text-2xl font-bold font-mono text-[var(--text-primary)]">{analytics.totalDevices}</span>
                <span className="text-xs text-[var(--text-secondary)] font-mono">nodes</span>
              </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            <span>{analytics.onlineDevices} Online</span>
            <span className="mx-1.5">·</span>
            <span>{analytics.isolatedDevices} Isolated</span>
          </div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">NETWORK RISK</span>
          <div className="flex items-baseline justify-between">
            <span
              className={`text-2xl font-bold font-mono ${
                analytics.networkRiskIndex > 65
                  ? 'text-rose-400'
                  : analytics.networkRiskIndex > 35
                  ? 'text-amber-400'
                  : 'text-emerald-400'
              }`}
            >
              {analytics.networkRiskIndex}
              <span className="text-xs text-slate-500 font-normal">/100</span>
            </span>
            <AlertTriangle className={`w-4 h-4 ${analytics.networkRiskIndex > 65 ? 'text-rose-400' : 'text-slate-600'}`} />
          </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            {analytics.networkRiskIndex > 65 ? 'Critical posture' : 'Nominal posture'}
          </div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">ACTIVE INCIDENTS</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold font-mono text-slate-100">{analytics.activeIncidents}</span>
            <span className="text-xs text-rose-400 font-mono font-semibold">{analytics.criticalIncidents} Critical</span>
          </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            <span>9-stage lifecycle</span>
          </div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">TRAFFIC THROUGHPUT</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold font-mono text-cyan-400">
              {analytics.trafficPacketsPerSec.toLocaleString()}
            </span>
            <span className="text-xs text-slate-500 font-mono">pkts/s</span>
          </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            <span>Synthetic simulation</span>
          </div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">AI DETECTOR ACCURACY</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold font-mono text-emerald-400">{analytics.detectionAccuracy}%</span>
            <Zap className="w-4 h-4 text-emerald-400" />
          </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            <span>Isolation + Random Forest</span>
          </div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] font-mono text-slate-400 block mb-1">XR SYNCHRONIZATION</span>
          <div className="flex items-baseline justify-between">
            <span className="text-xl font-bold font-mono text-slate-100">Live</span>
            <span className="text-xs text-cyan-400 font-mono">WebSockets</span>
          </div>
          <div className="text-[11px] text-slate-500 mt-2 font-mono">
            <span>AR + VR mirrored</span>
          </div>
        </div>
      </div>

      {/* Main Grid: Active Incidents & Topology Quick Action */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Active Incidents Spotlight (2 Columns) */}
        <div className="lg:col-span-2 space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-base font-bold text-[var(--text-primary)] font-heading">
                Active Security Incidents
              </h2>
              <p className="text-xs text-[var(--text-secondary)]">
                Threat events evaluated by ML detector requiring analyst triage or containment
              </p>
            </div>
            <button
              onClick={() => onNavigateTab('incidents')}
              className="text-xs text-cyan-400 hover:text-cyan-300 font-mono flex items-center gap-1 transition-colors"
            >
              View All Lifecycle <ArrowUpRight className="w-3.5 h-3.5" />
            </button>
          </div>

          {activeIncidents.length === 0 ? (
            <div className="glass-panel p-8 text-center">
              <CheckCircle2 className="w-8 h-8 text-emerald-400 mx-auto mb-2" />
              <h3 className="text-sm font-semibold text-[var(--text-primary)]">All Systems Nominal</h3>
              <p className="text-xs text-[var(--text-secondary)] max-w-sm mx-auto mt-1">
                No active threats or anomalies currently detected across network perimeters. Launch a simulation to evaluate response workflows.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {activeIncidents.map((incident) => (
                <div
                  key={incident.id}
                  onClick={() => onSelectIncident(incident)}
                  className="glass-panel p-4 hover:border-cyan-500/40 cursor-pointer transition-all border border-blue-500/20"
                >
                  <div className="flex items-start justify-between gap-3 mb-2">
                    <div>
                      <div className="flex items-center gap-2 text-xs font-mono text-[var(--text-secondary)] mb-1">
                        <span className="font-semibold text-cyan-400">{incident.id}</span>
                        <span>·</span>
                        <span>{incident.scenario}</span>
                        <span>·</span>
                        <span className={incident.severity === 'critical' ? 'text-rose-400 font-bold' : 'text-amber-400'}>
                          {incident.severity.toUpperCase()}
                        </span>
                      </div>
                      <h3 className="text-sm font-bold text-[var(--text-primary)]">{incident.title}</h3>
                    </div>

                    <span className="text-[11px] font-mono px-2 py-0.5 rounded bg-slate-800 text-slate-300">
                      Status: {incident.status}
                    </span>
                  </div>

                  <p className="text-xs text-[var(--text-secondary)] line-clamp-2 mb-3">
                    {incident.description}
                  </p>

                  <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-slate-800 text-[11px] font-mono text-[var(--text-secondary)]">
                    <div>
                      Target: <span className="text-[var(--text-primary)] font-semibold">{incident.targetDeviceId}</span>
                      <span className="mx-1.5">·</span>
                      Confidence: <span className="text-emerald-400">{incident.confidenceScore}%</span>
                    </div>
                    <div className="text-cyan-400 hover:underline">
                      Investigate with Copilot →
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Live Event Stream Sidebar */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-bold text-[var(--text-primary)] font-heading">
              Real-time Ingestion Stream
            </h2>
            <button
              onClick={() => onNavigateTab('threats')}
              className="text-xs text-[var(--text-secondary)] hover:text-[var(--text-primary)] font-mono"
            >
              Full Feed →
            </button>
          </div>

          <div className="glass-panel p-3 space-y-2 max-h-[460px] overflow-y-auto">
            {recentEvents.length === 0 ? (
              <div className="p-6 text-center text-xs text-slate-500 font-mono">
                Awaiting telemetry events...
              </div>
            ) : (
              recentEvents.slice(0, 8).map((evt) => (
                <div key={evt.id} className="p-2.5 rounded bg-slate-900/60 border border-slate-800 text-xs font-mono">
                  <div className="flex items-center justify-between text-[10px] text-slate-400 mb-1">
                    <span className="font-semibold text-slate-300">{evt.id}</span>
                    <span className={evt.severity === 'critical' ? 'text-rose-400' : evt.severity === 'high' ? 'text-amber-400' : 'text-slate-500'}>
                      {evt.predictedAttackType}
                    </span>
                  </div>
                  <div className="text-slate-300 text-[11px] truncate">
                    {evt.sourceIp} → {evt.destIp}:{evt.destPort}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1 flex items-center justify-between">
                    <span>Rate: {evt.packetRate} pkts/s</span>
                    <span>Score: {evt.anomalyScore.toFixed(2)}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
