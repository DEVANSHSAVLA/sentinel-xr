import React, { useState, useEffect, useRef } from 'react';
import { Header } from './components/Header';
import { DashboardView } from './components/DashboardView';
import { NetworkTopology3D } from './components/NetworkTopology3D';
import { DeviceExplorerView } from './components/DeviceExplorerView';
import { ThreatIntelView } from './components/ThreatIntelView';
import { IncidentManagementView } from './components/IncidentManagementView';
import { SimulationLabView } from './components/SimulationLabView';
import { AiCopilotView } from './components/AiCopilotView';
import { XrControlCenterView } from './components/XrControlCenterView';
import { AnalyticsReportsView } from './components/AnalyticsReportsView';
import {
  NetworkNode,
  NetworkConnection,
  Incident,
  SecurityEvent,
  SystemAnalytics,
  SimulationState,
  XrSessionStatus,
  UserProfile,
  SimulationScenarioId,
} from './types';

export function App() {
  const [currentTab, setCurrentTab] = useState<string>('dashboard');
  const [isLightMode, setIsLightMode] = useState<boolean>(false);

  // Core synchronized application state
  const [nodes, setNodes] = useState<NetworkNode[]>([]);
  const [connections, setConnections] = useState<NetworkConnection[]>([]);
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [recentEvents, setRecentEvents] = useState<SecurityEvent[]>([]);
  const [analytics, setAnalytics] = useState<SystemAnalytics>({
    totalDevices: 9,
    onlineDevices: 9,
    isolatedDevices: 0,
    activeIncidents: 1,
    criticalIncidents: 0,
    networkRiskIndex: 28,
    trafficPacketsPerSec: 4200,
    detectionAccuracy: 97.4,
    avgResolutionTimeMinutes: 4.8,
    threatDistribution: [],
  });
  const [simulationState, setSimulationState] = useState<SimulationState>({
    isActive: false,
    isPaused: false,
    currentScenario: null,
    targetDeviceId: null,
    intensity: 'medium',
    elapsedSeconds: 0,
    totalEventsGenerated: 0,
    activeIncidentsGenerated: 0,
  });
  const [xrStatus, setXrStatus] = useState<XrSessionStatus>({
    arConnected: false,
    vrConnected: false,
    lastSyncTimestamp: new Date().toISOString(),
    activeIncidentId: null,
    trackingMode: 'ARCore Plane / Marker Tracking',
    activeSpatialAnchor: 'ANCHOR_SENTINEL_SOC_ORIGIN',
    fpsAverage: 72,
  });

  const [currentUser, setCurrentUser] = useState<UserProfile>({
    id: 'USR-ADMIN',
    username: 'admin',
    name: 'Sarah Chen (Lead SecOps Architect)',
    role: 'Administrator',
    token: 'tok-admin-sentinel-2026',
  });
  const [allUsers, setAllUsers] = useState<UserProfile[]>([]);

  const [selectedNode, setSelectedNode] = useState<NetworkNode | null>(null);
  const [selectedIncident, setSelectedIncident] = useState<Incident | null>(null);

  const wsRef = useRef<WebSocket | null>(null);

  // Initial Data Fetch
  const fetchAllData = async () => {
    try {
      const [topRes, incRes, evtRes, anaRes, simRes, xrRes, usrRes] = await Promise.all([
        fetch('/api/v1/network/topology').then((r) => r.json()),
        fetch('/api/v1/incidents').then((r) => r.json()),
        fetch('/api/v1/events?limit=30').then((r) => r.json()),
        fetch('/api/v1/analytics/overview').then((r) => r.json()),
        fetch('/api/v1/simulation/status').then((r) => r.json()),
        fetch('/api/v1/xr/status').then((r) => r.json()),
        fetch('/api/v1/auth/users').then((r) => r.json()),
      ]);

      if (topRes.nodes) {
        setNodes(topRes.nodes);
        setConnections(topRes.connections);
      }
      if (Array.isArray(incRes)) setIncidents(incRes);
      if (Array.isArray(evtRes)) setRecentEvents(evtRes);
      if (anaRes) setAnalytics(anaRes);
      if (simRes) setSimulationState(simRes);
      if (xrRes) setXrStatus(xrRes);
      if (Array.isArray(usrRes)) {
        setAllUsers(usrRes);
        if (usrRes.length > 0) setCurrentUser(usrRes[0]);
      }
    } catch (err) {
      console.warn('Initial data load error:', err);
    }
  };

  useEffect(() => {
    fetchAllData();

    // Setup WebSocket connection
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const wsUrl = `${protocol}//${window.location.host}/ws`;
    const ws = new WebSocket(wsUrl);
    wsRef.current = ws;

    ws.onmessage = (event) => {
      try {
        const msg = JSON.parse(event.data);
        if (msg.type === 'SECURITY_EVENT') {
          setRecentEvents((prev) => [msg.data, ...prev.slice(0, 50)]);
        } else if (msg.type === 'TOPOLOGY_UPDATE') {
          setNodes(msg.data.nodes);
          setConnections(msg.data.connections);
        } else if (msg.type === 'INCIDENT_CREATED') {
          setIncidents((prev) => [msg.data, ...prev]);
        } else if (msg.type === 'INCIDENT_UPDATE') {
          setIncidents((prev) =>
            prev.map((i) => (i.id === msg.data.id ? msg.data : i))
          );
          if (selectedIncident?.id === msg.data.id) {
            setSelectedIncident(msg.data);
          }
        } else if (msg.type === 'SIMULATION_UPDATE') {
          setSimulationState(msg.data);
        } else if (msg.type === 'XR_UPDATE') {
          setXrStatus(msg.data);
        }
      } catch (e) {
        console.error('Error parsing WebSocket message:', e);
      }
    };

    const interval = setInterval(() => {
      fetch('/api/v1/analytics/overview')
        .then((r) => r.json())
        .then((data) => setAnalytics(data))
        .catch(() => {});
    }, 4000);

    return () => {
      clearInterval(interval);
      ws.close();
    };
  }, []);

  const handleToggleTheme = () => {
    setIsLightMode(!isLightMode);
    if (!isLightMode) {
      document.documentElement.classList.add('light');
    } else {
      document.documentElement.classList.remove('light');
    }
  };

  const handleIsolateNode = async (nodeId: string, isolate: boolean) => {
    try {
      const res = await fetch(`/api/v1/devices/${nodeId}/isolate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isolate }),
      });
      const data = await res.json();
      if (data.node) {
        setNodes((prev) => prev.map((n) => (n.id === nodeId ? data.node : n)));
        if (selectedNode?.id === nodeId) {
          setSelectedNode(data.node);
        }
      }
    } catch (err) {
      console.error('Error toggling node isolation:', err);
    }
  };

  const handleContainIncident = async (incidentId: string) => {
    try {
      const res = await fetch(`/api/v1/incidents/${incidentId}/contain`, {
        method: 'POST',
      });
      const updated = await res.json();
      setIncidents((prev) => prev.map((i) => (i.id === incidentId ? updated : i)));
      setSelectedIncident(updated);
      // Refresh topology
      const topRes = await fetch('/api/v1/network/topology').then((r) => r.json());
      if (topRes.nodes) {
        setNodes(topRes.nodes);
        setConnections(topRes.connections);
      }
    } catch (err) {
      console.error('Error containing incident:', err);
    }
  };

  const handleResolveIncident = async (incidentId: string) => {
    try {
      const res = await fetch(`/api/v1/incidents/${incidentId}/resolve`, {
        method: 'POST',
      });
      const updated = await res.json();
      setIncidents((prev) => prev.map((i) => (i.id === incidentId ? updated : i)));
      setSelectedIncident(updated);
      const topRes = await fetch('/api/v1/network/topology').then((r) => r.json());
      if (topRes.nodes) {
        setNodes(topRes.nodes);
        setConnections(topRes.connections);
      }
    } catch (err) {
      console.error('Error resolving incident:', err);
    }
  };

  const handleStartSimulation = async (
    scenarioId: SimulationScenarioId,
    targetId: string,
    intensity: 'low' | 'medium' | 'high' | 'extreme'
  ) => {
    try {
      const res = await fetch('/api/v1/simulation/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ scenarioId, targetDeviceId: targetId, intensity }),
      });
      const data = await res.json();
      setSimulationState(data);
    } catch (err) {
      console.error('Error starting simulation:', err);
    }
  };

  const handlePauseSimulation = async () => {
    try {
      const res = await fetch('/api/v1/simulation/pause', { method: 'POST' });
      const data = await res.json();
      setSimulationState(data);
    } catch (err) {
      console.error('Error pausing simulation:', err);
    }
  };

  const handleStopSimulation = async () => {
    try {
      const res = await fetch('/api/v1/simulation/stop', { method: 'POST' });
      const data = await res.json();
      setSimulationState(data);
    } catch (err) {
      console.error('Error stopping simulation:', err);
    }
  };

  const handleResetSimulation = async () => {
    try {
      await fetch('/api/v1/simulation/reset', { method: 'POST' });
      fetchAllData();
    } catch (err) {
      console.error('Error resetting simulation:', err);
    }
  };

  const handleTriggerXrSync = async () => {
    try {
      const res = await fetch('/api/v1/xr/sync', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          clientType: 'Web',
          activeIncidentId: selectedIncident?.id || 'INC-7041',
          activeSpatialAnchor: 'ANCHOR_SENTINEL_SOC_ORIGIN',
        }),
      });
      const data = await res.json();
      if (data.status) setXrStatus(data.status);
    } catch (err) {
      console.error('Error triggering XR sync:', err);
    }
  };

  return (
    <div className={`min-h-screen ${isLightMode ? 'light' : ''} bg-[var(--bg-base)] text-[var(--text-primary)] transition-colors duration-200`}>
      {/* Strict 3-zone Header */}
      <Header
        currentTab={currentTab}
        onSelectTab={(tab) => setCurrentTab(tab)}
        isLightMode={isLightMode}
        onToggleTheme={handleToggleTheme}
        currentUser={currentUser}
        allUsers={allUsers}
        onSwitchUser={(user) => setCurrentUser(user)}
        onQuickSimulate={() => {
          if (simulationState.isActive) {
            handleStopSimulation();
          } else {
            handleStartSimulation('ddos', 'server-01', 'high');
            setCurrentTab('simulation');
          }
        }}
        isSimulating={simulationState.isActive}
      />

      {/* Main Content Area */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 py-6">
        {currentTab === 'dashboard' && (
          <div className="space-y-6">
            <DashboardView
              analytics={analytics}
              incidents={incidents}
              recentEvents={recentEvents}
              nodes={nodes}
              onSelectIncident={(inc) => {
                setSelectedIncident(inc);
                setCurrentTab('incidents');
              }}
              onNavigateTab={(tab) => setCurrentTab(tab)}
            />

            {/* Inlined 3D Topology in Dashboard */}
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-base font-bold text-slate-100 font-heading">
                    Spatial 3D Infrastructure Topology
                  </h2>
                  <p className="text-xs text-slate-400">
                    Interact, inspect nodes, or quarantine compromised hosts directly in the holographic view
                  </p>
                </div>
                <button
                  onClick={() => setCurrentTab('topology')}
                  className="text-xs text-cyan-400 hover:text-cyan-300 font-mono"
                >
                  Full Screen 3D Mode →
                </button>
              </div>

              <NetworkTopology3D
                nodes={nodes}
                connections={connections}
                selectedNodeId={selectedNode?.id || null}
                onSelectNode={(node) => setSelectedNode(node)}
                onIsolateNode={handleIsolateNode}
                isSimulating={simulationState.isActive}
              />
            </div>
          </div>
        )}

        {currentTab === 'topology' && (
          <div className="space-y-4">
            <div>
              <h2 className="text-xl font-bold text-slate-100 font-heading">
                Interactive 3D Network Topology
              </h2>
              <p className="text-xs text-slate-400">
                Full 6DoF spatial view: rotate with left mouse drag, zoom with scroll wheel, click nodes to isolate
              </p>
            </div>

            <NetworkTopology3D
              nodes={nodes}
              connections={connections}
              selectedNodeId={selectedNode?.id || null}
              onSelectNode={(node) => setSelectedNode(node)}
              onIsolateNode={handleIsolateNode}
              isSimulating={simulationState.isActive}
            />
          </div>
        )}

        {currentTab === 'devices' && (
          <DeviceExplorerView
            nodes={nodes}
            incidents={incidents}
            onIsolateNode={handleIsolateNode}
            onSelectNode={(node) => {
              setSelectedNode(node);
              setCurrentTab('topology');
            }}
            onAskCopilotAboutDevice={(node) => {
              setSelectedNode(node);
              setCurrentTab('copilot');
            }}
          />
        )}

        {currentTab === 'threats' && (
          <ThreatIntelView events={recentEvents} />
        )}

        {currentTab === 'incidents' && (
          <IncidentManagementView
            incidents={incidents}
            selectedIncident={selectedIncident}
            onSelectIncident={(inc) => setSelectedIncident(inc)}
            onContainIncident={handleContainIncident}
            onResolveIncident={handleResolveIncident}
            onAskCopilot={(inc) => {
              setSelectedIncident(inc);
              setCurrentTab('copilot');
            }}
          />
        )}

        {currentTab === 'simulation' && (
          <SimulationLabView
            nodes={nodes}
            simulationState={simulationState}
            onStartSimulation={handleStartSimulation}
            onPauseSimulation={handlePauseSimulation}
            onStopSimulation={handleStopSimulation}
            onResetSimulation={handleResetSimulation}
            recentEvents={recentEvents}
          />
        )}

        {currentTab === 'copilot' && (
          <AiCopilotView
            incidents={incidents}
            nodes={nodes}
            initialIncident={selectedIncident}
            onApproveContainment={handleContainIncident}
          />
        )}

        {currentTab === 'xr' && (
          <XrControlCenterView
            xrStatus={xrStatus}
            nodes={nodes}
            incidents={incidents}
            onTriggerXrSync={handleTriggerXrSync}
          />
        )}

        {currentTab === 'reports' && (
          <AnalyticsReportsView
            analytics={analytics}
            incidents={incidents}
          />
        )}
      </main>
    </div>
  );
}
export default App;
