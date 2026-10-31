import React, { useState } from 'react';
import { XrSessionStatus, Incident, NetworkNode } from '../types';
import { Eye, Glasses, Smartphone, QrCode, Download, RefreshCw, CheckCircle2, Shield, Radio } from 'lucide-react';

interface XrControlCenterViewProps {
  xrStatus: XrSessionStatus;
  nodes: NetworkNode[];
  incidents: Incident[];
  onTriggerXrSync: () => void;
}

export const XrControlCenterView: React.FC<XrControlCenterViewProps> = ({
  xrStatus,
  nodes,
  incidents,
  onTriggerXrSync,
}) => {
  const [selectedMarkerDevice, setSelectedMarkerDevice] = useState<string>('server-01');
  const [activeTab, setActiveTab] = useState<'status' | 'markers' | 'soc_guide'>('status');

  const selectedNode = nodes.find((n) => n.id === selectedMarkerDevice) || nodes[0];

  const handleDownloadMarker = () => {
    // Generate SVG marker with device info and encoded QR styling
    const svgContent = `
      <svg xmlns="http://www.w3.org/2000/svg" width="600" height="600" viewBox="0 0 600 600">
        <rect width="600" height="600" fill="#070B16"/>
        <rect x="30" y="30" width="540" height="540" fill="none" stroke="#00D9FF" stroke-width="4" stroke-dasharray="16,8"/>
        <text x="300" y="80" fill="#ECF3FF" font-family="monospace" font-size="24" font-weight="bold" text-anchor="middle">SENTINEL-XR PHYSICAL AR ANCHOR</text>
        <text x="300" y="115" fill="#00D9FF" font-family="monospace" font-size="16" text-anchor="middle">DEVICE ID: ${selectedNode.id}</text>
        <rect x="150" y="150" width="300" height="300" fill="#FFFFFF" rx="12"/>
        <!-- QR Anchor Representation Matrix -->
        <rect x="180" y="180" width="80" height="80" fill="#070B16"/>
        <rect x="195" y="195" width="50" height="50" fill="#FFFFFF"/>
        <rect x="207" y="207" width="26" height="26" fill="#070B16"/>
        <rect x="340" y="180" width="80" height="80" fill="#070B16"/>
        <rect x="355" y="195" width="50" height="50" fill="#FFFFFF"/>
        <rect x="367" y="207" width="26" height="26" fill="#070B16"/>
        <rect x="180" y="340" width="80" height="80" fill="#070B16"/>
        <rect x="195" y="355" width="50" height="50" fill="#FFFFFF"/>
        <rect x="207" y="367" width="26" height="26" fill="#070B16"/>
        <rect x="280" y="280" width="40" height="40" fill="#477BFF"/>
        <!-- Telemetry Spec on Marker -->
        <text x="300" y="490" fill="#ECF3FF" font-family="monospace" font-size="15" text-anchor="middle">${selectedNode.name}</text>
        <text x="300" y="520" fill="#91A2BC" font-family="monospace" font-size="13" text-anchor="middle">IP: ${selectedNode.ip} · ZONE: ${selectedNode.zone.toUpperCase()}</text>
        <text x="300" y="550" fill="#5A6E8C" font-family="monospace" font-size="11" text-anchor="middle">Tracked via AR Foundation 2D Image Anchor Provider</text>
      </svg>
    `;

    const blob = new Blob([svgContent], { type: 'image/svg+xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `SENTINEL_AR_MARKER_${selectedNode.id}.svg`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      {/* View Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading">
            Extended Reality (XR) Operations Hub
          </h2>
          <p className="text-xs text-slate-400">
            Bidirectional real-time state synchronization between Web, Unity AR (ARCore), and Unity VR (OpenXR)
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={onTriggerXrSync}
            className="px-3 py-1.5 text-xs font-semibold text-cyan-400 glass-panel hover:bg-slate-800 rounded-lg transition-colors flex items-center gap-1.5"
          >
            <RefreshCw className="w-3.5 h-3.5" /> Trigger State Sync
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-800 pb-2">
        <button
          onClick={() => setActiveTab('status')}
          className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-colors ${
            activeTab === 'status' ? 'bg-cyan-500 text-slate-950' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          XR Session Telemetry
        </button>
        <button
          onClick={() => setActiveTab('markers')}
          className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-colors ${
            activeTab === 'markers' ? 'bg-cyan-500 text-slate-950' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          AR Image Target Generator
        </button>
        <button
          onClick={() => setActiveTab('soc_guide')}
          className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-colors ${
            activeTab === 'soc_guide' ? 'bg-cyan-500 text-slate-950' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          Immersive SOC Guide & Architecture
        </button>
      </div>

      {activeTab === 'status' && (
        <div className="space-y-6">
          {/* Status Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* AR Status */}
            <div className="glass-panel p-5 border border-blue-500/20">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2 text-cyan-400 font-bold font-mono text-sm">
                  <Smartphone className="w-4 h-4" /> Unity AR Client
                </div>
                <span
                  className={`text-[10px] font-mono px-2 py-0.5 rounded font-semibold ${
                    xrStatus.arConnected
                      ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30'
                      : 'bg-slate-800 text-slate-400'
                  }`}
                >
                  {xrStatus.arConnected ? 'Connected' : 'Available (ARCore)'}
                </span>
              </div>
              <p className="text-xs text-slate-300 mb-3">
                Spatial tracking for Android ARCore devices. Overlays holographic threat stats on physical server racks via QR/image anchors.
              </p>
              <div className="text-[11px] font-mono text-slate-500 space-y-1">
                <div>Tracking: AR Foundation Plane / Image Track</div>
                <div>Anchor: {xrStatus.activeSpatialAnchor}</div>
              </div>
            </div>

            {/* VR Status */}
            <div className="glass-panel p-5 border border-blue-500/20">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2 text-violet-400 font-bold font-mono text-sm">
                  <Glasses className="w-4 h-4" /> Unity VR SOC
                </div>
                <span
                  className={`text-[10px] font-mono px-2 py-0.5 rounded font-semibold ${
                    xrStatus.vrConnected
                      ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30'
                      : 'bg-slate-800 text-slate-400'
                  }`}
                >
                  {xrStatus.vrConnected ? 'Connected' : 'Available (OpenXR)'}
                </span>
              </div>
              <p className="text-xs text-slate-300 mb-3">
                Fully immersive 3D Security Operations Room with grab-inspect topology nodes, floating threat boards, and AI station.
              </p>
              <div className="text-[11px] font-mono text-slate-500 space-y-1">
                <div>Engine: OpenXR 6DoF + Fallback Desktop WASD</div>
                <div>Target Frame Budget: {xrStatus.fpsAverage} FPS</div>
              </div>
            </div>

            {/* Sync Engine */}
            <div className="glass-panel p-5 border border-blue-500/20">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2 text-emerald-400 font-bold font-mono text-sm">
                  <Radio className="w-4 h-4" /> Real-time Bus
                </div>
                <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 font-semibold">
                  WebSockets Synchronized
                </span>
              </div>
              <p className="text-xs text-slate-300 mb-3">
                Shared state pipeline: Ingested events and approved containment actions instantly update Web, AR, and VR simultaneously.
              </p>
              <div className="text-[11px] font-mono text-slate-500 space-y-1">
                <div>Last Synchronized: {new Date(xrStatus.lastSyncTimestamp).toLocaleTimeString()}</div>
                <div>Active Incident Hook: {xrStatus.activeIncidentId || 'INC-7041'}</div>
              </div>
            </div>
          </div>

          {/* Unified Synchronization Protocol Flow Diagram */}
          <div className="glass-panel-elevated p-5 border border-cyan-500/30 space-y-3">
            <h3 className="text-sm font-bold text-slate-100 font-heading">
              SENTINEL-XR Unified Synchronization Architecture
            </h3>
            <p className="text-xs text-slate-400">
              How actions initiated in either Web, AR, or VR execute against the single authoritative backend:
            </p>

            <div className="p-4 bg-slate-950 rounded-lg border border-slate-800 font-mono text-xs text-slate-300 leading-relaxed overflow-x-auto">
              {`[ WEB DASHBOARD ] ───▶ [ FASTAPI / EXPRESS BACKEND ] ◀─── [ UNITY VR SOC ]
        │                             │                              │
        │                             ▼                              │
        │                  [ SHARED EVENT BUS & DB ]                 │
        │                             │                              │
        │                             ▼                              │
        │               [ ML INFERENCE & SHAP ENGINE ]               │
        │                             │                              │
        ▼                             ▼                              ▼
  Live 3D WebGL              Real-time WebSockets             Physical AR Overlay
  (Topology + Copilot)        (ws://localhost:3000/ws)        (Server Rack Marker)`}
            </div>
          </div>
        </div>
      )}

      {activeTab === 'markers' && (
        <div className="glass-panel-elevated p-6 border border-cyan-500/30 space-y-5">
          <div>
            <h3 className="text-base font-bold text-slate-100 font-heading">
              AR Physical Device Anchor Marker Generator
            </h3>
            <p className="text-xs text-slate-400">
              Print or display this optical tracking marker on your physical workstation or server chassis. The Unity AR client recognizes this anchor and projects spatial threat metrics.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-center">
            {/* Controls */}
            <div className="space-y-4">
              <div>
                <label className="text-xs text-slate-400 font-mono block mb-1.5">
                  Select Device for Physical AR Anchor:
                </label>
                <select
                  value={selectedMarkerDevice}
                  onChange={(e) => setSelectedMarkerDevice(e.target.value)}
                  className="w-full p-2.5 text-xs rounded-lg glass-panel text-slate-200 font-mono focus:outline-none focus:border-cyan-400 cursor-pointer"
                >
                  {nodes.map((n) => (
                    <option key={n.id} value={n.id} className="bg-slate-900 text-slate-200">
                      {n.name} ({n.id} - {n.ip})
                    </option>
                  ))}
                </select>
              </div>

              <div className="p-3 bg-slate-900/80 rounded border border-slate-800 text-xs font-mono space-y-1.5">
                <div className="text-slate-400">Target Node: <strong className="text-cyan-400">{selectedNode.name}</strong></div>
                <div className="text-slate-400">IP: <span className="text-slate-200">{selectedNode.ip}</span></div>
                <div className="text-slate-400">Zone: <span className="text-slate-200">{selectedNode.zone.toUpperCase()}</span></div>
                <div className="text-slate-400">Status: <span className="text-emerald-400">{selectedNode.status.toUpperCase()}</span></div>
              </div>

              <button
                onClick={handleDownloadMarker}
                className="w-full py-2.5 px-4 text-xs font-bold text-slate-950 bg-cyan-400 hover:bg-cyan-300 rounded-lg transition-colors flex items-center justify-center gap-2 shadow-lg"
              >
                <Download className="w-4 h-4" /> Download Printable AR Marker (.svg)
              </button>
            </div>

            {/* Marker Visual Preview */}
            <div className="flex flex-col items-center justify-center p-6 bg-slate-950 rounded-xl border border-dashed border-cyan-500/40">
              <div className="w-64 h-64 bg-[#070B16] rounded-lg border-2 border-dashed border-cyan-400 p-4 flex flex-col items-center justify-between text-center font-mono">
                <span className="text-[10px] text-cyan-400 font-bold uppercase tracking-wider">
                  SENTINEL-XR AR ANCHOR
                </span>
                <div className="w-36 h-36 bg-white rounded-lg p-2 flex items-center justify-center">
                  <QrCode className="w-32 h-32 text-slate-950" />
                </div>
                <div className="text-[10px] text-slate-300">
                  {selectedNode.id} · {selectedNode.ip}
                </div>
              </div>
              <span className="text-[11px] text-slate-500 font-mono mt-2">
                Preview of 2D Image Tracking Target
              </span>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'soc_guide' && (
        <div className="glass-panel-elevated p-6 border border-cyan-500/30 space-y-4">
          <h3 className="text-base font-bold text-slate-100 font-heading">
            Unity XR Implementation & Operations Guide
          </h3>
          <p className="text-xs text-slate-300 leading-relaxed">
            The SENTINEL-XR monorepo includes native Unity 6 XR source code located at <code className="text-cyan-400 font-mono">xr/unity/SentinelXR/</code>.
          </p>

          <div className="space-y-3 text-xs font-mono">
            <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
              <span className="text-cyan-400 font-bold block mb-1">1. Augmented Reality (AR) Architecture</span>
              <p className="text-slate-300 leading-relaxed">
                Uses <strong>AR Foundation</strong> and <strong>ARCore XR Plugin</strong> with <code className="text-slate-200">ARTrackedImageManager</code>. When camera frames identify the physical server markers, <code className="text-slate-200">ARDeviceOverlay.cs</code> binds to the world-space transform and renders a floating holographic telemetry card displaying live CPU, RAM, risk score, and active incidents queried from <code className="text-slate-200">/api/v1/devices/:id</code>.
              </p>
            </div>

            <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
              <span className="text-violet-400 font-bold block mb-1">2. Virtual Reality (VR) SOC Architecture</span>
              <p className="text-slate-300 leading-relaxed">
                Uses <strong>OpenXR</strong> and the <strong>XR Interaction Toolkit</strong> with <code className="text-slate-200">XRGrabInteractable</code>. The central holographic 3D network model replicates the Three.js WebGL spatial topology. Analysts can grab and rotate nodes, inspect incident blast radii, interact with the floating AI Copilot panel, and trigger simulated containment approvals.
              </p>
            </div>

            <div className="p-3 bg-slate-900/80 rounded border border-slate-800">
              <span className="text-emerald-400 font-bold block mb-1">3. Desktop Simulation Fallback</span>
              <p className="text-slate-300 leading-relaxed">
                For developers without physical headsets or ARCore phones, the browser-based 3D topology and desktop navigation modes provide a complete 6DoF walkthrough experience using the exact same backend endpoints and WebSocket feeds.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
