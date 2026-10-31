import React, { useState } from 'react';
import { NetworkNode, Incident } from '../types';
import { Search, Server, ShieldAlert, RefreshCw, Cpu, HardDrive, Wifi, ExternalLink } from 'lucide-react';

interface DeviceExplorerViewProps {
  nodes: NetworkNode[];
  incidents: Incident[];
  onIsolateNode: (nodeId: string, isolate: boolean) => void;
  onSelectNode: (node: NetworkNode) => void;
  onAskCopilotAboutDevice: (device: NetworkNode) => void;
}

export const DeviceExplorerView: React.FC<DeviceExplorerViewProps> = ({
  nodes,
  incidents,
  onIsolateNode,
  onSelectNode,
  onAskCopilotAboutDevice,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedZone, setSelectedZone] = useState('all');
  const [selectedStatus, setSelectedStatus] = useState('all');

  const filteredNodes = nodes.filter((n) => {
    const matchesSearch =
      n.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      n.ip.toLowerCase().includes(searchTerm.toLowerCase()) ||
      n.id.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesZone = selectedZone === 'all' || n.zone === selectedZone;
    const matchesStatus = selectedStatus === 'all' || n.status === selectedStatus;
    return matchesSearch && matchesZone && matchesStatus;
  });

  return (
    <div className="space-y-6">
      {/* Title & Filter Bar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading">
            Network Device Inventory
          </h2>
          <p className="text-xs text-slate-400">
            Real-time telemetry and state inspection for all {nodes.length} simulated topology nodes
          </p>
        </div>

        {/* Search & Filter Controls */}
        <div className="flex flex-wrap items-center gap-2.5">
          <div className="relative">
            <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search name, IP, ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-8 pr-3 py-1.5 text-xs rounded-lg glass-panel text-slate-200 placeholder-slate-500 focus:outline-none focus:border-cyan-400 font-mono w-48"
            />
          </div>

          <select
            value={selectedZone}
            onChange={(e) => setSelectedZone(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg glass-panel text-slate-300 font-mono focus:outline-none cursor-pointer"
            aria-label="Filter by Network Zone"
          >
            <option value="all" className="bg-slate-900">All Zones</option>
            <option value="external" className="bg-slate-900">External</option>
            <option value="perimeter" className="bg-slate-900">Perimeter</option>
            <option value="dmz" className="bg-slate-900">DMZ</option>
            <option value="internal" className="bg-slate-900">Internal Subnet</option>
            <option value="secure_core" className="bg-slate-900">Secure Vault Core</option>
          </select>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg glass-panel text-slate-300 font-mono focus:outline-none cursor-pointer"
            aria-label="Filter by Node Status"
          >
            <option value="all" className="bg-slate-900">All Statuses</option>
            <option value="healthy" className="bg-slate-900">Healthy</option>
            <option value="suspicious" className="bg-slate-900">Suspicious</option>
            <option value="critical" className="bg-slate-900">Critical</option>
            <option value="isolated" className="bg-slate-900">Isolated</option>
          </select>
        </div>
      </div>

      {/* Device Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredNodes.map((node) => {
          const nodeIncidents = incidents.filter(
            (i) => i.targetDeviceId === node.id && i.status !== 'resolved'
          );

          return (
            <div
              key={node.id}
              className={`glass-panel p-4 flex flex-col justify-between transition-all ${
                node.status === 'critical'
                  ? 'border-rose-500/40 glow-red'
                  : node.status === 'isolated'
                  ? 'border-slate-700/50 opacity-80'
                  : 'hover:border-cyan-500/30'
              }`}
            >
              <div>
                {/* Header */}
                <div className="flex items-start justify-between gap-3 mb-2">
                  <div className="flex items-center gap-2">
                    <Server className={`w-4 h-4 ${node.status === 'isolated' ? 'text-slate-500' : 'text-cyan-400'}`} />
                    <div>
                      <h3 className="text-sm font-bold text-slate-100">{node.name}</h3>
                      <span className="text-[11px] font-mono text-slate-400">{node.ip}</span>
                    </div>
                  </div>

                  <span
                    className={`text-[10px] font-mono px-2 py-0.5 rounded uppercase font-semibold ${
                      node.status === 'critical'
                        ? 'bg-rose-500/20 text-rose-400 border border-rose-500/30'
                        : node.status === 'suspicious'
                        ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30'
                        : node.status === 'isolated'
                        ? 'bg-slate-800 text-slate-400 border border-slate-700'
                        : 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30'
                    }`}
                  >
                    {node.status}
                  </span>
                </div>

                {/* Metadata strip */}
                <div className="flex items-center gap-2 text-xs font-mono text-slate-400 my-2.5">
                  <span>ID: {node.id}</span>
                  <span>·</span>
                  <span>Zone: {node.zone.toUpperCase()}</span>
                  <span>·</span>
                  <span>Risk: <strong className={node.riskScore > 50 ? 'text-rose-400' : 'text-slate-200'}>{node.riskScore}/100</strong></span>
                </div>

                {/* Performance telemetry */}
                <div className="grid grid-cols-3 gap-2 my-3 p-2 rounded bg-slate-900/60 border border-slate-800/80 text-[11px] font-mono">
                  <div>
                    <span className="text-slate-500 flex items-center gap-1">
                      <Cpu className="w-3 h-3" /> CPU
                    </span>
                    <span className="text-slate-200 font-semibold">{node.cpuUsage}%</span>
                  </div>
                  <div>
                    <span className="text-slate-500 flex items-center gap-1">
                      <HardDrive className="w-3 h-3" /> RAM
                    </span>
                    <span className="text-slate-200 font-semibold">{node.memoryUsage}%</span>
                  </div>
                  <div>
                    <span className="text-slate-500 flex items-center gap-1">
                      <Wifi className="w-3 h-3" /> Traffic
                    </span>
                    <span className="text-cyan-400 font-semibold">{node.trafficRate} p/s</span>
                  </div>
                </div>

                {/* Services list */}
                <div className="text-[10px] text-slate-400 font-mono mb-3">
                  <span className="text-slate-500 block mb-1">BOUND SERVICES:</span>
                  <div className="flex flex-wrap gap-1">
                    {node.services.map((svc, idx) => (
                      <span key={idx} className="bg-slate-800/80 px-1.5 py-0.5 rounded text-slate-300">
                        {svc}
                      </span>
                    ))}
                  </div>
                </div>

                {/* Associated Incidents Indicator */}
                {nodeIncidents.length > 0 && (
                  <div className="p-2 rounded bg-rose-950/40 border border-rose-500/30 text-[11px] text-rose-300 font-mono mb-3">
                    Active Incident: {nodeIncidents[0].title}
                  </div>
                )}
              </div>

              {/* Action buttons */}
              <div className="pt-3 border-t border-slate-800 flex items-center gap-2">
                <button
                  onClick={() => onSelectNode(node)}
                  className="flex-1 py-1.5 px-2.5 text-xs font-medium text-slate-300 hover:text-white glass-panel hover:bg-slate-800 transition-colors rounded flex items-center justify-center gap-1"
                >
                  <ExternalLink className="w-3 h-3" /> Inspect 3D
                </button>

                <button
                  onClick={() => onAskCopilotAboutDevice(node)}
                  className="flex-1 py-1.5 px-2.5 text-xs font-medium text-cyan-400 hover:text-cyan-300 glass-panel hover:bg-slate-800 transition-colors rounded"
                >
                  AI Copilot
                </button>

                {node.status === 'isolated' ? (
                  <button
                    onClick={() => onIsolateNode(node.id, false)}
                    className="py-1.5 px-3 text-xs font-semibold text-emerald-300 bg-emerald-950/60 hover:bg-emerald-900 border border-emerald-500/40 rounded transition-colors"
                    title="Reconnect node"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                  </button>
                ) : (
                  <button
                    onClick={() => onIsolateNode(node.id, true)}
                    className="py-1.5 px-3 text-xs font-semibold text-rose-300 bg-rose-950/60 hover:bg-rose-900 border border-rose-500/40 rounded transition-colors"
                    title="Quarantine / Isolate node"
                  >
                    <ShieldAlert className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
