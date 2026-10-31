import React, { useState } from 'react';
import { Incident } from '../types';
import { ShieldCheck, ShieldAlert, CheckCircle, FileText, Download, UserCheck, AlertTriangle } from 'lucide-react';

interface IncidentManagementViewProps {
  incidents: Incident[];
  selectedIncident: Incident | null;
  onSelectIncident: (incident: Incident) => void;
  onContainIncident: (incidentId: string) => Promise<void>;
  onResolveIncident: (incidentId: string) => Promise<void>;
  onAskCopilot: (incident: Incident) => void;
}

const LIFECYCLE_STAGES = [
  'detected',
  'investigating',
  'assessing_risk',
  'response_recommended',
  'pending_approval',
  'contained',
  'verifying',
  'resolved',
];

export const IncidentManagementView: React.FC<IncidentManagementViewProps> = ({
  incidents,
  selectedIncident,
  onSelectIncident,
  onContainIncident,
  onResolveIncident,
  onAskCopilot,
}) => {
  const [isProcessing, setIsProcessing] = useState(false);
  const [reportDossier, setReportDossier] = useState<string | null>(null);

  const activeIncident = selectedIncident || incidents[0] || null;

  const handleContain = async () => {
    if (!activeIncident) return;
    setIsProcessing(true);
    try {
      await onContainIncident(activeIncident.id);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleResolve = async () => {
    if (!activeIncident) return;
    setIsProcessing(true);
    try {
      await onResolveIncident(activeIncident.id);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleViewReport = async () => {
    if (!activeIncident) return;
    try {
      const res = await fetch(`/api/v1/incidents/${activeIncident.id}/report`);
      const data = await res.json();
      setReportDossier(data.report);
    } catch (err) {
      console.error('Failed to load forensic dossier:', err);
    }
  };

  const handleDownloadReport = () => {
    if (!reportDossier && !activeIncident) return;
    const content = reportDossier || 'No report generated';
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `SENTINEL_FORENSIC_${activeIncident?.id || 'INC'}.txt`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      {/* View Header */}
      <div>
        <h2 className="text-xl font-bold text-slate-100 font-heading">
          Incident Response & Containment Center
        </h2>
        <p className="text-xs text-slate-400">
          9-stage incident lifecycle with SHAP Explainable AI attribution and human-in-the-loop containment
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Incident List (Left 4 Columns) */}
        <div className="lg:col-span-4 space-y-3">
          <div className="text-xs font-mono text-slate-400 uppercase tracking-wider mb-1">
            Tracked Incidents ({incidents.length})
          </div>

          <div className="space-y-2 max-h-[680px] overflow-y-auto pr-1">
            {incidents.length === 0 ? (
              <div className="glass-panel p-6 text-center text-xs text-slate-500 font-mono">
                No recorded incidents. Run a simulation to generate an active security incident.
              </div>
            ) : (
              incidents.map((inc) => (
                <div
                  key={inc.id}
                  onClick={() => onSelectIncident(inc)}
                  className={`glass-panel p-3.5 cursor-pointer transition-all border ${
                    activeIncident?.id === inc.id
                      ? 'border-cyan-400 bg-blue-950/40 shadow-lg'
                      : 'border-blue-500/15 hover:border-blue-500/40'
                  }`}
                >
                  <div className="flex items-start justify-between gap-2 mb-1.5">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-bold font-mono text-cyan-400">{inc.id}</span>
                      <span className="text-[10px] text-slate-500 font-mono">· {inc.scenario}</span>
                    </div>
                    <span
                      className={`text-[10px] uppercase font-semibold font-mono px-1.5 py-0.5 rounded ${
                        inc.severity === 'critical'
                          ? 'bg-rose-500/20 text-rose-400'
                          : inc.severity === 'high'
                          ? 'bg-amber-500/20 text-amber-400'
                          : 'bg-cyan-500/20 text-cyan-400'
                      }`}
                    >
                      {inc.severity}
                    </span>
                  </div>

                  <h4 className="text-xs font-semibold text-slate-200 line-clamp-1 mb-1">
                    {inc.title}
                  </h4>

                  <div className="flex items-center justify-between text-[10px] text-slate-400 font-mono pt-1 border-t border-slate-800">
                    <span>Target: {inc.targetDeviceId}</span>
                    <span className="capitalize">{inc.status.replace('_', ' ')}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Detailed Incident Workspace (Right 8 Columns) */}
        {activeIncident ? (
          <div className="lg:col-span-8 space-y-5">
            {/* Primary Details Card */}
            <div className="glass-panel-elevated p-5 border border-cyan-500/30">
              <div className="flex flex-col md:flex-row md:items-start justify-between gap-4 mb-4">
                <div>
                  <div className="flex items-center gap-2 text-xs font-mono text-slate-400 mb-1">
                    <span className="text-cyan-400 font-bold">{activeIncident.id}</span>
                    <span>·</span>
                    <span>Scenario: {activeIncident.scenario}</span>
                    <span>·</span>
                    <span>Confidence: <strong className="text-emerald-400">{activeIncident.confidenceScore}%</strong></span>
                  </div>
                  <h3 className="text-lg font-bold text-slate-100 font-heading">
                    {activeIncident.title}
                  </h3>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => onAskCopilot(activeIncident)}
                    className="px-3 py-1.5 text-xs font-semibold text-cyan-300 bg-cyan-950/60 hover:bg-cyan-900 border border-cyan-500/40 rounded-lg transition-colors"
                  >
                    Investigate with Copilot
                  </button>
                  <button
                    onClick={handleViewReport}
                    className="px-3 py-1.5 text-xs font-semibold text-slate-300 bg-slate-800 hover:bg-slate-700 border border-slate-700 rounded-lg transition-colors flex items-center gap-1.5"
                  >
                    <FileText className="w-3.5 h-3.5" /> Forensic Report
                  </button>
                </div>
              </div>

              {/* 9-Stage Visual Lifecycle Progress Bar */}
              <div className="my-5 p-3 rounded-lg bg-slate-900/80 border border-slate-800">
                <span className="text-[10px] font-mono text-slate-500 uppercase block mb-2">
                  Incident Response Lifecycle
                </span>
                <div className="grid grid-cols-4 md:grid-cols-8 gap-1 text-center font-mono">
                  {LIFECYCLE_STAGES.map((stage, idx) => {
                    const currentIndex = LIFECYCLE_STAGES.indexOf(activeIncident.status);
                    const isPassed = idx <= currentIndex;
                    const isCurrent = idx === currentIndex;

                    return (
                      <div
                        key={stage}
                        className={`py-1.5 px-1 rounded text-[10px] uppercase font-semibold transition-colors ${
                          isCurrent
                            ? 'bg-cyan-500 text-slate-950 shadow-md font-bold'
                            : isPassed
                            ? 'bg-blue-950 text-cyan-300 border border-blue-500/30'
                            : 'bg-slate-950 text-slate-600 border border-slate-800'
                        }`}
                      >
                        {stage.replace('_', ' ')}
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Target & Offender Spec */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3 my-4 p-3 bg-slate-900/60 rounded border border-slate-800 text-xs font-mono">
                <div>
                  <span className="text-slate-500 block text-[10px]">TARGET DEVICE</span>
                  <span className="text-slate-200 font-semibold">{activeIncident.targetDeviceId}</span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">SOURCE IP</span>
                  <span className="text-rose-400 font-semibold">{activeIncident.sourceIp}</span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">SEVERITY</span>
                  <span className={`font-semibold uppercase ${activeIncident.severity === 'critical' ? 'text-rose-400' : 'text-amber-400'}`}>
                    {activeIncident.severity}
                  </span>
                </div>
                <div>
                  <span className="text-slate-500 block text-[10px]">CURRENT STAGE</span>
                  <span className="text-cyan-400 font-semibold capitalize">{activeIncident.status.replace('_', ' ')}</span>
                </div>
              </div>

              {/* Description */}
              <div className="mb-4">
                <span className="text-[11px] font-mono text-slate-400 uppercase block mb-1">
                  DETECTION NARRATIVE
                </span>
                <p className="text-xs text-slate-300 leading-relaxed bg-slate-950/70 p-3 rounded border border-slate-800">
                  {activeIncident.description}
                </p>
              </div>

              {/* SHAP Explainable AI Breakdown */}
              <div className="mb-4">
                <span className="text-[11px] font-mono text-slate-400 uppercase block mb-2">
                  SHAP Explainable AI Feature Attribution Factors
                </span>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-2.5">
                  {activeIncident.shapFactors.map((factor, idx) => (
                    <div key={idx} className="p-3 bg-slate-900/70 rounded border border-blue-500/20 text-xs font-mono">
                      <div className="flex items-center justify-between text-[10px] text-slate-400 mb-1">
                        <span className="font-semibold text-slate-300">{factor.feature}</span>
                        <span className={factor.impact === 'Severe' ? 'text-rose-400 font-bold' : 'text-amber-400'}>
                          {factor.impact}
                        </span>
                      </div>
                      <div className="text-slate-100 font-semibold text-sm">{factor.value}</div>
                      <div className="text-[10px] text-cyan-400 mt-1">
                        SHAP Score Contribution: +{factor.contributionScore}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Recommended Response & Human Containment Actions */}
              <div className="p-4 rounded-lg bg-blue-950/30 border border-blue-500/30 my-4 space-y-3">
                <div className="flex items-start gap-2.5">
                  <AlertTriangle className="w-4 h-4 text-amber-400 shrink-0 mt-0.5" />
                  <div>
                    <h4 className="text-xs font-bold text-slate-200 uppercase font-heading tracking-wider">
                      Recommended Containment Action (Human Approval Required)
                    </h4>
                    <p className="text-xs text-slate-300 mt-1">
                      {activeIncident.recommendedAction}
                    </p>
                  </div>
                </div>

                <div className="flex flex-wrap items-center gap-3 pt-2">
                  {!activeIncident.containmentApproved ? (
                    <button
                      onClick={handleContain}
                      disabled={isProcessing}
                      className="px-4 py-2 text-xs font-semibold text-white bg-rose-600 hover:bg-rose-500 rounded-lg transition-colors flex items-center gap-1.5 shadow-lg shadow-rose-900/30 disabled:opacity-50"
                    >
                      <ShieldAlert className="w-4 h-4" />
                      Approve & Execute Simulated Containment
                    </button>
                  ) : activeIncident.status !== 'resolved' ? (
                    <button
                      onClick={handleResolve}
                      disabled={isProcessing}
                      className="px-4 py-2 text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-500 rounded-lg transition-colors flex items-center gap-1.5 shadow-lg shadow-emerald-900/30 disabled:opacity-50"
                    >
                      <CheckCircle className="w-4 h-4" />
                      Mark Incident Verified & Resolved
                    </button>
                  ) : (
                    <div className="text-xs font-mono text-emerald-400 flex items-center gap-1.5 font-semibold">
                      <CheckCircle className="w-4 h-4" /> Incident Successfully Resolved
                    </div>
                  )}

                  <span className="text-[11px] text-slate-500 font-mono">
                    {activeIncident.containmentApproved
                      ? `Contained at ${new Date(activeIncident.containedAt || '').toLocaleTimeString()}`
                      : 'Pending human supervisor authorization'}
                  </span>
                </div>
              </div>

              {/* Audit Timeline */}
              <div className="pt-3 border-t border-slate-800">
                <span className="text-[11px] font-mono text-slate-400 uppercase block mb-2">
                  Forensic Timeline & Audit Trail
                </span>
                <div className="space-y-2">
                  {activeIncident.timeline.map((item, idx) => (
                    <div key={idx} className="flex items-start gap-3 text-xs font-mono">
                      <span className="text-slate-500 text-[10px] w-20 shrink-0">
                        {new Date(item.time).toLocaleTimeString()}
                      </span>
                      <span className="text-cyan-400 font-semibold w-24 shrink-0 uppercase text-[10px]">
                        [{item.stage.replace('_', ' ')}]
                      </span>
                      <span className="text-slate-300">
                        {item.description} <em className="text-slate-500 not-italic text-[10px]">by {item.actor}</em>
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Forensic Dossier Modal */}
            {reportDossier && (
              <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in">
                <div className="glass-panel-elevated p-6 max-w-2xl w-full border border-cyan-500/40 shadow-2xl space-y-4">
                  <div className="flex items-center justify-between border-b border-slate-800 pb-3">
                    <h3 className="text-sm font-bold text-cyan-400 font-mono flex items-center gap-2">
                      <FileText className="w-4 h-4" /> Incident Forensic Dossier [{activeIncident.id}]
                    </h3>
                    <button
                      onClick={() => setReportDossier(null)}
                      className="text-slate-400 hover:text-white text-xs px-2 py-1 rounded bg-slate-800"
                    >
                      ✕
                    </button>
                  </div>

                  <pre className="p-4 bg-slate-950 rounded border border-slate-800 text-[11px] font-mono text-slate-300 max-h-[420px] overflow-y-auto whitespace-pre-wrap leading-relaxed">
                    {reportDossier}
                  </pre>

                  <div className="flex items-center justify-end gap-3 pt-2">
                    <button
                      onClick={handleDownloadReport}
                      className="px-4 py-1.5 text-xs font-semibold text-slate-950 bg-cyan-400 hover:bg-cyan-300 rounded transition-colors flex items-center gap-1.5"
                    >
                      <Download className="w-3.5 h-3.5" /> Download Dossier (.txt)
                    </button>
                    <button
                      onClick={() => setReportDossier(null)}
                      className="px-4 py-1.5 text-xs font-semibold text-slate-300 bg-slate-800 hover:bg-slate-700 rounded transition-colors"
                    >
                      Close
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="lg:col-span-8 glass-panel p-12 text-center text-slate-500 font-mono text-xs">
            Select an incident from the left panel to review telemetry and approve response containment.
          </div>
        )}
      </div>
    </div>
  );
};
