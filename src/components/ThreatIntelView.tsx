import React, { useState } from 'react';
import { SecurityEvent } from '../types';
import { ShieldAlert, Search, Filter, Terminal, ArrowDownRight, Layers } from 'lucide-react';

interface ThreatIntelViewProps {
  events: SecurityEvent[];
  onInspectEvent?: (evt: SecurityEvent) => void;
}

export const ThreatIntelView: React.FC<ThreatIntelViewProps> = ({ events }) => {
  const [selectedSeverity, setSelectedSeverity] = useState('all');
  const [selectedType, setSelectedType] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [activeModalEvent, setActiveModalEvent] = useState<SecurityEvent | null>(null);

  const filteredEvents = events.filter((e) => {
    const matchesSeverity = selectedSeverity === 'all' || e.severity === selectedSeverity;
    const matchesType = selectedType === 'all' || e.predictedAttackType === selectedType;
    const matchesSearch =
      e.id.toLowerCase().includes(searchQuery.toLowerCase()) ||
      e.sourceIp.includes(searchQuery) ||
      e.destIp.includes(searchQuery) ||
      (e.targetDeviceId && e.targetDeviceId.toLowerCase().includes(searchQuery.toLowerCase()));
    return matchesSeverity && matchesType && matchesSearch;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading">
            Threat Intelligence & Telemetry Ingestion
          </h2>
          <p className="text-xs text-slate-400">
            Real-time packet telemetry evaluated by Isolation Forest anomaly detector and Random Forest classifier
          </p>
        </div>

        {/* Filters */}
        <div className="flex flex-wrap items-center gap-2.5">
          <div className="relative">
            <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search IP, Event ID, Device..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-8 pr-3 py-1.5 text-xs rounded-lg glass-panel text-slate-200 placeholder-slate-500 focus:outline-none focus:border-cyan-400 font-mono w-52"
            />
          </div>

          <select
            value={selectedSeverity}
            onChange={(e) => setSelectedSeverity(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg glass-panel text-slate-300 font-mono focus:outline-none cursor-pointer"
            aria-label="Filter by Severity"
          >
            <option value="all" className="bg-slate-900">All Severities</option>
            <option value="critical" className="bg-slate-900">Critical</option>
            <option value="high" className="bg-slate-900">High</option>
            <option value="medium" className="bg-slate-900">Medium</option>
            <option value="low" className="bg-slate-900">Low</option>
          </select>

          <select
            value={selectedType}
            onChange={(e) => setSelectedType(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg glass-panel text-slate-300 font-mono focus:outline-none cursor-pointer"
            aria-label="Filter by Attack Classification"
          >
            <option value="all" className="bg-slate-900">All Classifications</option>
            <option value="DDoS" className="bg-slate-900">DDoS</option>
            <option value="Brute-Force" className="bg-slate-900">Brute-Force</option>
            <option value="Port-Scan" className="bg-slate-900">Port-Scan</option>
            <option value="Lateral Movement" className="bg-slate-900">Lateral Movement</option>
            <option value="Normal" className="bg-slate-900">Normal</option>
          </select>
        </div>
      </div>

      {/* Events Table */}
      <div className="glass-panel overflow-hidden border border-blue-500/20">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs font-mono">
            <thead className="bg-slate-900/80 text-[11px] text-slate-400 border-b border-slate-800 uppercase tracking-wider">
              <tr>
                <th className="py-3 px-4">Event ID / Time</th>
                <th className="py-3 px-4">Source IP</th>
                <th className="py-3 px-4">Target IP / Port</th>
                <th className="py-3 px-4">Classification</th>
                <th className="py-3 px-4">Severity</th>
                <th className="py-3 px-4">Packet Rate</th>
                <th className="py-3 px-4">Anomaly Score</th>
                <th className="py-3 px-4 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60">
              {filteredEvents.length === 0 ? (
                <tr>
                  <td colSpan={8} className="py-12 text-center text-slate-500">
                    No matching security events in buffer. Start a simulation in Sim Lab to stream live attack records.
                  </td>
                </tr>
              ) : (
                filteredEvents.slice(0, 30).map((evt) => (
                  <tr key={evt.id} className="hover:bg-slate-800/40 transition-colors">
                    <td className="py-2.5 px-4">
                      <div className="font-semibold text-slate-200">{evt.id}</div>
                      <div className="text-[10px] text-slate-500">
                        {new Date(evt.timestamp).toLocaleTimeString()}
                      </div>
                    </td>
                    <td className="py-2.5 px-4 text-slate-300 font-semibold">{evt.sourceIp}</td>
                    <td className="py-2.5 px-4 text-slate-300">
                      <div>{evt.destIp}:{evt.destPort}</div>
                      {evt.targetDeviceId && (
                        <div className="text-[10px] text-cyan-400">{evt.targetDeviceId}</div>
                      )}
                    </td>
                    <td className="py-2.5 px-4">
                      <span
                        className={`font-semibold ${
                          evt.predictedAttackType === 'DDoS'
                            ? 'text-rose-400'
                            : evt.predictedAttackType === 'Brute-Force'
                            ? 'text-amber-400'
                            : evt.predictedAttackType === 'Lateral Movement'
                            ? 'text-violet-400'
                            : evt.predictedAttackType === 'Port-Scan'
                            ? 'text-cyan-400'
                            : 'text-slate-400'
                        }`}
                      >
                        {evt.predictedAttackType}
                      </span>
                    </td>
                    <td className="py-2.5 px-4">
                      <span
                        className={`text-[10px] uppercase px-2 py-0.5 rounded font-semibold ${
                          evt.severity === 'critical'
                            ? 'bg-rose-500/20 text-rose-400 border border-rose-500/30'
                            : evt.severity === 'high'
                            ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30'
                            : evt.severity === 'medium'
                            ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/30'
                            : 'bg-slate-800 text-slate-400'
                        }`}
                      >
                        {evt.severity}
                      </span>
                    </td>
                    <td className="py-2.5 px-4 text-slate-300">
                      {evt.packetRate.toLocaleString()} <span className="text-[10px] text-slate-500">p/s</span>
                    </td>
                    <td className="py-2.5 px-4">
                      <div className="flex items-center gap-2">
                        <span className={`font-bold ${evt.anomalyScore > 0.6 ? 'text-rose-400' : 'text-slate-300'}`}>
                          {evt.anomalyScore.toFixed(2)}
                        </span>
                        <div className="w-12 h-1.5 rounded-full bg-slate-800 overflow-hidden">
                          <div
                            className={`h-full ${evt.anomalyScore > 0.6 ? 'bg-rose-500' : 'bg-cyan-400'}`}
                            style={{ width: `${Math.min(100, evt.anomalyScore * 100)}%` }}
                          />
                        </div>
                      </div>
                    </td>
                    <td className="py-2.5 px-4 text-right">
                      <button
                        onClick={() => setActiveModalEvent(evt)}
                        className="text-xs text-cyan-400 hover:text-cyan-300 underline font-mono"
                      >
                        Payload
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Raw Payload Modal */}
      {activeModalEvent && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm animate-in fade-in">
          <div className="glass-panel-elevated p-6 max-w-lg w-full border border-cyan-500/40 shadow-2xl space-y-4">
            <div className="flex items-center justify-between border-b border-slate-800 pb-3">
              <div className="flex items-center gap-2 text-cyan-400 font-mono text-sm font-bold">
                <Terminal className="w-4 h-4" />
                Raw Packet Inspection [{activeModalEvent.id}]
              </div>
              <button
                onClick={() => setActiveModalEvent(null)}
                className="text-slate-400 hover:text-white text-xs px-2 py-1 rounded bg-slate-800"
              >
                ✕
              </button>
            </div>

            <div className="space-y-2 text-xs font-mono">
              <div className="grid grid-cols-2 gap-2 p-3 bg-slate-900/80 rounded border border-slate-800">
                <div>
                  <span className="text-slate-500 block text-[10px]">SOURCE</span>
                  <span className="text-slate-200 font-semibold">{activeModalEvent.sourceIp}</span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">DESTINATION</span>
                  <span className="text-slate-200 font-semibold">{activeModalEvent.destIp}:{activeModalEvent.destPort}</span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">PROTOCOL</span>
                  <span className="text-cyan-400 font-semibold">{activeModalEvent.protocol}</span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">AUTH FAILURES</span>
                  <span className="text-rose-400 font-semibold">{activeModalEvent.failedAuthAttempts}</span>
                </div>
              </div>

              <div>
                <span className="text-slate-400 text-[11px] block mb-1">DECODED PACKET / EVENT SUMMARY:</span>
                <div className="p-3 bg-slate-950 rounded border border-slate-800 text-slate-300 break-words leading-relaxed">
                  {activeModalEvent.rawPayloadSummary}
                </div>
              </div>

              <div className="p-3 bg-blue-950/40 border border-blue-500/30 rounded text-[11px] text-blue-300">
                Isolation Forest Anomaly Index: <strong>{activeModalEvent.anomalyScore.toFixed(3)}</strong> · Classified by Random Forest as <strong>{activeModalEvent.predictedAttackType}</strong> with {activeModalEvent.severity.toUpperCase()} severity.
              </div>
            </div>

            <div className="pt-2 flex justify-end">
              <button
                onClick={() => setActiveModalEvent(null)}
                className="px-4 py-1.5 text-xs font-semibold text-slate-300 bg-slate-800 hover:bg-slate-700 rounded transition-colors"
              >
                Close Inspector
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
