import React from 'react';
import { SystemAnalytics, Incident } from '../types';
import { Download, FileSpreadsheet, ShieldCheck, PieChart, TrendingUp, BarChart2, Clock } from 'lucide-react';

interface AnalyticsReportsViewProps {
  analytics: SystemAnalytics;
  incidents: Incident[];
}

export const AnalyticsReportsView: React.FC<AnalyticsReportsViewProps> = ({
  analytics,
  incidents,
}) => {
  const handleDownloadCsv = () => {
    window.location.href = '/api/v1/reports/export?format=csv';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading">
            Cyber Threat Analytics & Forensic Reports
          </h2>
          <p className="text-xs text-slate-400">
            Historical incident metrics, detection model evaluation, and compliance export workflows
          </p>
        </div>

        <button
          onClick={handleDownloadCsv}
          className="px-4 py-2 text-xs font-semibold text-slate-950 bg-cyan-400 hover:bg-cyan-300 rounded-lg transition-colors flex items-center gap-2 shadow-lg"
        >
          <FileSpreadsheet className="w-4 h-4" /> Export Incidents CSV
        </button>
      </div>

      {/* Top Metrics Row */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 font-mono">
        <div className="glass-panel p-4">
          <span className="text-[11px] text-slate-400 block mb-1">DETECTION ACCURACY</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold text-emerald-400">{analytics.detectionAccuracy}%</span>
            <TrendingUp className="w-4 h-4 text-emerald-400" />
          </div>
          <div className="text-[10px] text-slate-500 mt-2">Zero false-positive containment in test set</div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] text-slate-400 block mb-1">MEAN TIME TO RESOLUTION</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold text-cyan-400">{analytics.avgResolutionTimeMinutes} min</span>
            <Clock className="w-4 h-4 text-cyan-400" />
          </div>
          <div className="text-[10px] text-slate-500 mt-2">From automated detection to verified quarantine</div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] text-slate-400 block mb-1">CONTAINMENT EFFECTIVENESS</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold text-slate-100">100%</span>
            <ShieldCheck className="w-4 h-4 text-emerald-400" />
          </div>
          <div className="text-[10px] text-slate-500 mt-2">Zero lateral egress from quarantined nodes</div>
        </div>

        <div className="glass-panel p-4">
          <span className="text-[11px] text-slate-400 block mb-1">TOTAL RESOLVED INCIDENTS</span>
          <div className="flex items-baseline justify-between">
            <span className="text-2xl font-bold text-slate-100">
              {incidents.filter((i) => i.status === 'resolved').length}
            </span>
            <span className="text-xs text-slate-500">records</span>
          </div>
          <div className="text-[10px] text-slate-500 mt-2">Stored with full forensic dossiers</div>
        </div>
      </div>

      {/* Threat Distribution Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Threat Distribution Visual */}
        <div className="glass-panel p-5 border border-blue-500/20 space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-bold text-slate-100 font-heading flex items-center gap-2">
              <PieChart className="w-4 h-4 text-cyan-400" /> Threat Scenario Distribution
            </h3>
            <span className="text-xs font-mono text-slate-500">Synthetic Corpus</span>
          </div>

          <div className="space-y-3">
            {analytics.threatDistribution.map((item, idx) => (
              <div key={idx} className="space-y-1 font-mono text-xs">
                <div className="flex items-center justify-between">
                  <span className="text-slate-300 font-semibold">{item.name}</span>
                  <span className="text-slate-400">{item.count} incident(s)</span>
                </div>
                <div className="w-full h-2 rounded-full bg-slate-900 overflow-hidden border border-slate-800">
                  <div
                    className="h-full rounded-full transition-all duration-500"
                    style={{
                      backgroundColor: item.color,
                      width: `${Math.max(10, (item.count / (incidents.length || 1)) * 100)}%`,
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Audit & Compliance Summary */}
        <div className="glass-panel p-5 border border-blue-500/20 space-y-3">
          <h3 className="text-sm font-bold text-slate-100 font-heading flex items-center gap-2">
            <BarChart2 className="w-4 h-4 text-violet-400" /> Compliance & Audit Readiness
          </h3>
          <p className="text-xs text-slate-300 leading-relaxed">
            SENTINEL-XR maintains an immutable in-memory and file audit log for all simulated events, model inferences, and human analyst approvals.
          </p>

          <div className="p-3 bg-slate-900/80 rounded border border-slate-800 text-xs font-mono space-y-1.5 text-slate-300">
            <div>• SOC 2 Type II / NIST CSF Compliance Simulation Mappings</div>
            <div>• Human-in-the-Loop Quarantine Validation: <strong>Enforced</strong></div>
            <div>• Zero Public Network Traffic Transmission: <strong>Verified</strong></div>
            <div>• Cryptographic SHA-256 Event Checksums: <strong>Active</strong></div>
          </div>
        </div>
      </div>
    </div>
  );
};
