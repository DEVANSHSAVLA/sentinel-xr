import React, { useState } from 'react';
import { Incident, NetworkNode } from '../types';
import { Send, Bot, User, Sparkles, ShieldAlert, Cpu, Terminal, CheckCircle2 } from 'lucide-react';

interface AiCopilotViewProps {
  incidents: Incident[];
  nodes: NetworkNode[];
  initialIncident?: Incident | null;
  onApproveContainment?: (incidentId: string) => Promise<void>;
}

interface Message {
  sender: 'user' | 'assistant';
  text: string;
  timestamp: string;
  modelUsed?: string;
  source?: string;
  recommendedAction?: string;
  contextIncidentId?: string;
}

export const AiCopilotView: React.FC<AiCopilotViewProps> = ({
  incidents,
  nodes,
  initialIncident,
  onApproveContainment,
}) => {
  const [selectedIncidentId, setSelectedIncidentId] = useState<string>(
    initialIncident?.id || incidents[0]?.id || ''
  );
  const [inputPrompt, setInputPrompt] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [messages, setMessages] = useState<Message[]>([
    {
      sender: 'assistant',
      text: `Greetings, Analyst. I am the **SENTINEL-XR AI Investigation Copilot** powered by Google Gemini and real-time backend telemetry.\n\nI can explain active incidents, analyze SHAP feature attribution metrics from our ML pipeline, evaluate blast radius across the 3D topology, and recommend verified containment actions.\n\nSelect an incident above or ask any question about the network state.`,
      timestamp: new Date().toLocaleTimeString(),
      modelUsed: 'gemini-2.5-flash',
      source: 'gemini',
    },
  ]);

  const activeIncident = incidents.find((i) => i.id === selectedIncidentId);

  const handleSend = async (customPrompt?: string) => {
    const textToSend = customPrompt || inputPrompt;
    if (!textToSend.trim() || isLoading) return;

    const userMessage: Message = {
      sender: 'user',
      text: textToSend,
      timestamp: new Date().toLocaleTimeString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    if (!customPrompt) setInputPrompt('');
    setIsLoading(true);

    try {
      const response = await fetch('/api/v1/ai/investigate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          incidentId: selectedIncidentId,
          userPrompt: textToSend,
        }),
      });

      const data = await response.json();
      const botMessage: Message = {
        sender: 'assistant',
        text: data.answer || 'No response returned from copilot.',
        timestamp: new Date().toLocaleTimeString(),
        modelUsed: data.modelUsed,
        source: data.source,
        recommendedAction: data.recommendedAction,
        contextIncidentId: selectedIncidentId,
      };

      setMessages((prev) => [...prev, botMessage]);
    } catch (err) {
      console.error('Copilot request failed:', err);
      setMessages((prev) => [
        ...prev,
        {
          sender: 'assistant',
          text: 'Error contacting AI Copilot backend service. Please check network connectivity.',
          timestamp: new Date().toLocaleTimeString(),
          source: 'error',
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  const QUICK_QUESTIONS = [
    `Why is ${activeIncident?.targetDeviceId || 'Server-01'} flagged as under attack?`,
    `Explain the SHAP feature factors and ML confidence score for ${activeIncident?.id || 'the active incident'}.`,
    `What is the recommended containment action and network blast radius?`,
    `Generate an executive incident summary and remediation report.`,
  ];

  return (
    <div className="space-y-6">
      {/* Header & Incident Selector */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-100 font-heading flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-cyan-400" />
            AI Investigation Copilot
          </h2>
          <p className="text-xs text-slate-400">
            Grounded reasoning on live telemetry, Isolation Forest anomaly indices, and SHAP explainability
          </p>
        </div>

        {/* Incident Context Selector */}
        <div className="flex items-center gap-2">
          <span className="text-xs text-slate-400 font-mono">Investigating Incident:</span>
          <select
            value={selectedIncidentId}
            onChange={(e) => setSelectedIncidentId(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg glass-panel text-cyan-400 font-mono font-semibold focus:outline-none cursor-pointer"
          >
            {incidents.map((inc) => (
              <option key={inc.id} value={inc.id} className="bg-slate-900 text-slate-200">
                {inc.id} - {inc.scenario} ({inc.targetDeviceId})
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Main Chat Workspace */}
      <div className="glass-panel-elevated p-4 flex flex-col h-[580px] border border-blue-500/25">
        {/* Messages Scroll Area */}
        <div className="flex-1 overflow-y-auto space-y-4 p-2 pr-3">
          {messages.map((msg, idx) => (
            <div
              key={idx}
              className={`flex gap-3 text-xs leading-relaxed ${
                msg.sender === 'user' ? 'justify-end' : 'justify-start'
              }`}
            >
              {msg.sender === 'assistant' && (
                <div className="w-7 h-7 rounded-lg bg-cyan-950/80 border border-cyan-500/40 flex items-center justify-center text-cyan-400 shrink-0">
                  <Bot className="w-4 h-4" />
                </div>
              )}

              <div
                className={`p-3.5 rounded-xl max-w-2xl font-mono ${
                  msg.sender === 'user'
                    ? 'bg-blue-600 text-white rounded-tr-none'
                    : 'bg-slate-900/90 border border-slate-800 text-slate-200 rounded-tl-none'
                }`}
              >
                <div className="flex items-center justify-between gap-4 text-[10px] text-slate-400 mb-1 border-b border-white/10 pb-1">
                  <span>{msg.sender === 'user' ? 'Analyst Query' : 'SENTINEL AI Copilot'}</span>
                  <span>{msg.timestamp}</span>
                </div>

                <div className="whitespace-pre-wrap text-[11px] leading-relaxed">
                  {msg.text}
                </div>

                {msg.recommendedAction && onApproveContainment && msg.contextIncidentId && (
                  <div className="mt-3 pt-2 border-t border-slate-800 flex items-center justify-between">
                    <span className="text-[10px] text-amber-400 font-semibold">Recommended Containment Available</span>
                    <button
                      onClick={() => onApproveContainment(msg.contextIncidentId!)}
                      className="px-2.5 py-1 text-[11px] font-semibold text-white bg-rose-600 hover:bg-rose-500 rounded transition-colors"
                    >
                      Approve Containment
                    </button>
                  </div>
                )}

                {msg.modelUsed && (
                  <div className="mt-2 text-[9px] text-slate-500 flex items-center justify-end gap-1">
                    <span>Engine: {msg.modelUsed}</span>
                  </div>
                )}
              </div>

              {msg.sender === 'user' && (
                <div className="w-7 h-7 rounded-lg bg-blue-950/80 border border-blue-500/40 flex items-center justify-center text-blue-300 shrink-0">
                  <User className="w-4 h-4" />
                </div>
              )}
            </div>
          ))}

          {isLoading && (
            <div className="flex items-center gap-2 text-xs font-mono text-cyan-400 p-2">
              <Bot className="w-4 h-4 animate-spin" />
              <span>Analyzing telemetry features and synthesizing investigation briefing...</span>
            </div>
          )}
        </div>

        {/* Quick Suggestion Chips */}
        <div className="pt-2 pb-2 flex flex-wrap gap-1.5 border-t border-slate-800">
          {QUICK_QUESTIONS.map((q, idx) => (
            <button
              key={idx}
              onClick={() => handleSend(q)}
              disabled={isLoading}
              className="text-[11px] text-slate-300 hover:text-cyan-300 glass-panel hover:bg-slate-800/80 px-2.5 py-1 rounded-md transition-colors text-left"
            >
              {q}
            </button>
          ))}
        </div>

        {/* Chat Input Bar */}
        <div className="pt-2 flex items-center gap-2">
          <input
            type="text"
            value={inputPrompt}
            onChange={(e) => setInputPrompt(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
            placeholder="Ask Copilot about incidents, SHAP factors, device telemetry, or containment..."
            disabled={isLoading}
            className="flex-1 px-3.5 py-2 text-xs rounded-lg glass-panel text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-400 font-mono"
          />
          <button
            onClick={() => handleSend()}
            disabled={isLoading || !inputPrompt.trim()}
            className="px-4 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-500 disabled:opacity-50 rounded-lg transition-colors flex items-center gap-1.5 shadow-lg shadow-blue-900/30"
          >
            <Send className="w-3.5 h-3.5" /> Send
          </button>
        </div>
      </div>
    </div>
  );
};
