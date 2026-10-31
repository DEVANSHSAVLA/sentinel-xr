import React, { useState } from 'react';
import { NetworkNode, SimulationState, SimulationScenarioId, SecurityEvent } from '../types';
import { Play, Pause, Square, RotateCcw, AlertTriangle, ShieldCheck, Flame, Zap } from 'lucide-react';

interface SimulationLabViewProps {
  nodes: NetworkNode[];
  simulationState: SimulationState;
  onStartSimulation: (scenarioId: SimulationScenarioId, targetId: string, intensity: 'low' | 'medium' | 'high' | 'extreme') => void;
  onPauseSimulation: () => void;
  onStopSimulation: () => void;
  onResetSimulation: () => void;
  recentEvents: SecurityEvent[];
}

export const SimulationLabView: React.FC<SimulationLabViewProps> = ({
  nodes,
  simulationState,
  onStartSimulation,
  onPauseSimulation,
  onStopSimulation,
  onResetSimulation,
  recentEvents,
}) => {
  const [selectedScenario, setSelectedScenario] = useState<SimulationScenarioId>('ddos');
  const [selectedTarget, setSelectedTarget] = useState<string>('server-01');
  const [selectedIntensity, setSelectedIntensity] = useState<'low' | 'medium' | 'high' | 'extreme'>('high');

  const SCENARIOS = [
    {
      id: 'ddos' as SimulationScenarioId,
      title: 'Scenario A: DDoS Volumetric Surge',
      tag: 'Volumetric',
      desc: 'Simulates a distributed SYN flood inundating target web ingress with elevated packet rates and thousands of unique source IPs.',
      targetRecommend: 'server-01 (DMZ Web App)',
    },
    {
      id: 'bruteforce' as SimulationScenarioId,
      title: 'Scenario B: SSH Credential Stuffing',
      tag: 'Authentication',
      desc: 'Generates automated dictionary authentication failure bursts on SSH daemon (Port 22) probing administrative credentials.',
      targetRecommend: 'server-02 (DMZ API Gateway)',
    },
    {
      id: 'portscan' as SimulationScenarioId,
      title: 'Scenario C: Horizontal Port Reconnaissance',
      tag: 'Reconnaissance',
      desc: 'Simulates stealth SYN port sweep examining sequential ports across target perimeter to map exposed services.',
      targetRecommend: 'firewall-01 (Perimeter)',
    },
    {
      id: 'lateral' as SimulationScenarioId,
      title: 'Scenario D: East-West Lateral Movement',
      tag: 'Lateral Pivot',
      desc: 'Simulates internal compromised workstation attempting SMB traversal and Kerberos ticket probes into the secure database cluster.',
      targetRecommend: 'db-cluster-01 (Secure Vault)',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Title & Safety Banner */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading">
            Cybersecurity Attack Simulation Laboratory
          </h2>
          <p className="text-xs text-slate-400">
            Controlled synthetic incident generator passing through the live feature extraction and ML detection pipeline
          </p>
        </div>

        <div className="glass-panel px-3.5 py-1.5 flex items-center gap-2 text-xs font-mono text-emerald-400 border border-emerald-500/30">
          <ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" />
          <span>ISOLATED SYNTHETIC ENVIRONMENT — ZERO EXTERNAL TRAFFIC</span>
        </div>
      </div>

      {/* Simulation Master Control Console */}
      <div className="glass-panel-elevated p-6 border border-blue-500/30 shadow-2xl">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Col 1: Scenario Selection */}
          <div className="space-y-3">
            <label className="text-xs font-mono text-slate-400 uppercase tracking-wider block">
              1. Select Attack Scenario
            </label>
            <div className="space-y-2">
              {SCENARIOS.map((sc) => (
                <div
                  key={sc.id}
                  onClick={() => setSelectedScenario(sc.id)}
                  className={`p-3 rounded-lg cursor-pointer transition-all border ${
                    selectedScenario === sc.id
                      ? 'bg-blue-950/60 border-cyan-400 shadow-md'
                      : 'bg-slate-900/50 border-slate-800 hover:border-slate-700'
                  }`}
                >
                  <div className="flex items-center justify-between text-xs mb-1">
                    <span className="font-bold text-slate-200">{sc.title}</span>
                    <span className="text-[10px] font-mono text-cyan-400">{sc.tag}</span>
                  </div>
                  <p className="text-[11px] text-slate-400 line-clamp-2 leading-relaxed">
                    {sc.desc}
                  </p>
                </div>
              ))}
            </div>
          </div>

          {/* Col 2: Target & Intensity Configuration */}
          <div className="space-y-4">
            <label className="text-xs font-mono text-slate-400 uppercase tracking-wider block">
              2. Target Device & Intensity
            </label>

            <div>
              <span className="text-xs text-slate-300 font-mono block mb-1.5">Simulated Target Node:</span>
              <select
                value={selectedTarget}
                onChange={(e) => setSelectedTarget(e.target.value)}
                className="w-full p-2.5 text-xs rounded-lg glass-panel text-slate-200 font-mono focus:outline-none focus:border-cyan-400 cursor-pointer"
              >
                {nodes.map((n) => (
                  <option key={n.id} value={n.id} className="bg-slate-900 text-slate-200">
                    {n.name} ({n.id} - {n.ip}) [{n.zone}]
                  </option>
                ))}
              </select>
            </div>

            <div>
              <span className="text-xs text-slate-300 font-mono block mb-1.5">Packet Generation Intensity:</span>
              <div className="grid grid-cols-4 gap-2 font-mono text-xs">
                {(['low', 'medium', 'high', 'extreme'] as const).map((lvl) => (
                  <button
                    key={lvl}
                    onClick={() => setSelectedIntensity(lvl)}
                    className={`py-2 rounded uppercase font-semibold transition-colors ${
                      selectedIntensity === lvl
                        ? 'bg-cyan-500 text-slate-950 font-bold'
                        : 'bg-slate-900 text-slate-400 hover:text-slate-200 border border-slate-800'
                    }`}
                  >
                    {lvl}
                  </button>
                ))}
              </div>
            </div>

            {/* Telemetry Preview */}
            <div className="p-3 rounded bg-slate-950/80 border border-slate-800 text-xs font-mono space-y-1">
              <div className="text-[10px] text-slate-500 uppercase">EXPECTED SIMULATION SIGNATURE:</div>
              <div className="text-slate-300 text-[11px]">
                {selectedScenario === 'ddos' && 'SYN Flood: ~15,000 to 30,000 pkts/s with random external source spoofing.'}
                {selectedScenario === 'bruteforce' && 'SSH Wave: 12-25 failed attempts/sec targeting port 22 with dictionary words.'}
                {selectedScenario === 'portscan' && 'Port Sweep: Probing TCP 1-1024 with stealth FIN/SYN flags.'}
                {selectedScenario === 'lateral' && 'East-West Pivot: 10.0.3.14 -> 10.0.2.20 traversing SMB (445) and Kerberos.'}
              </div>
            </div>
          </div>

          {/* Col 3: Live Execution Engine Status & Controls */}
          <div className="space-y-4 flex flex-col justify-between">
            <div>
              <label className="text-xs font-mono text-slate-400 uppercase tracking-wider block mb-3">
                3. Execution Controls & Telemetry
              </label>

              <div className="grid grid-cols-2 gap-2 text-xs font-mono mb-4">
                <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
                  <span className="text-slate-500 block text-[10px]">ENGINE STATUS</span>
                  <span className={`text-sm font-bold ${simulationState.isActive ? 'text-rose-400' : 'text-slate-400'}`}>
                    {simulationState.isActive
                      ? simulationState.isPaused ? 'PAUSED' : 'RUNNING'
                      : 'STANDBY'}
                  </span>
                </div>
                <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
                  <span className="text-slate-500 block text-[10px]">EVENTS INGESTED</span>
                  <span className="text-sm font-bold text-cyan-400">
                    {simulationState.totalEventsGenerated}
                  </span>
                </div>
                <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
                  <span className="text-slate-500 block text-[10px]">ELAPSED TIME</span>
                  <span className="text-sm font-bold text-slate-200">
                    {simulationState.elapsedSeconds}s
                  </span>
                </div>
                <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
                  <span className="text-slate-500 block text-[10px]">ACTIVE INCIDENTS</span>
                  <span className="text-sm font-bold text-amber-400">
                    {simulationState.activeIncidentsGenerated}
                  </span>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="space-y-2">
              {!simulationState.isActive ? (
                <button
                  onClick={() => onStartSimulation(selectedScenario, selectedTarget, selectedIntensity)}
                  className="w-full py-3 px-4 text-xs font-bold uppercase tracking-wider text-white bg-blue-600 hover:bg-blue-500 rounded-lg transition-colors flex items-center justify-center gap-2 shadow-lg shadow-blue-900/30"
                >
                  <Play className="w-4 h-4 fill-current" /> Launch Attack Simulation
                </button>
              ) : (
                <div className="grid grid-cols-2 gap-2">
                  <button
                    onClick={onPauseSimulation}
                    className="py-2.5 px-3 text-xs font-semibold text-slate-200 bg-slate-800 hover:bg-slate-700 rounded-lg transition-colors flex items-center justify-center gap-1.5"
                  >
                    <Pause className="w-4 h-4" />
                    {simulationState.isPaused ? 'Resume' : 'Pause'}
                  </button>
                  <button
                    onClick={onStopSimulation}
                    className="py-2.5 px-3 text-xs font-semibold text-white bg-rose-600 hover:bg-rose-500 rounded-lg transition-colors flex items-center justify-center gap-1.5"
                  >
                    <Square className="w-4 h-4" /> Stop
                  </button>
                </div>
              )}

              <button
                onClick={onResetSimulation}
                className="w-full py-2 px-3 text-xs font-medium text-slate-400 hover:text-slate-200 glass-panel rounded-lg transition-colors flex items-center justify-center gap-1.5"
              >
                <RotateCcw className="w-3.5 h-3.5" /> Reset Topology & Simulation State
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Live Stream of Generated Simulation Events */}
      <div className="space-y-3">
        <h3 className="text-sm font-bold text-slate-100 font-heading">
          Generated Synthetic Telemetry Feed
        </h3>

        <div className="glass-panel p-3 max-h-64 overflow-y-auto space-y-2 font-mono text-xs">
          {recentEvents.length === 0 ? (
            <div className="p-8 text-center text-slate-500">
              Launch a scenario above to observe real-time synthetic traffic generated, normalized, and piped into the ML classifier.
            </div>
          ) : (
            recentEvents.slice(0, 15).map((evt) => (
              <div
                key={evt.id}
                className="p-2.5 rounded bg-slate-900/70 border border-slate-800 flex items-center justify-between text-xs"
              >
                <div className="flex items-center gap-2.5">
                  <span className="text-slate-500 text-[10px]">{new Date(evt.timestamp).toLocaleTimeString()}</span>
                  <span className="font-semibold text-slate-200">{evt.id}</span>
                  <span className="text-slate-400">· {evt.sourceIp} → {evt.destIp}:{evt.destPort}</span>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-[11px] text-cyan-400">{evt.packetRate} pkts/s</span>
                  <span
                    className={`font-semibold ${
                      evt.predictedAttackType === 'Normal' ? 'text-slate-400' : 'text-rose-400'
                    }`}
                  >
                    {evt.predictedAttackType}
                  </span>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};
