import React from 'react';
import { Shield, Moon, Sun, Play, UserCheck } from 'lucide-react';
import { UserProfile } from '../types';

interface HeaderProps {
  currentTab: string;
  onSelectTab: (tab: string) => void;
  isLightMode: boolean;
  onToggleTheme: () => void;
  currentUser: UserProfile;
  allUsers: UserProfile[];
  onSwitchUser: (user: UserProfile) => void;
  onQuickSimulate: () => void;
  isSimulating: boolean;
}

export const Header: React.FC<HeaderProps> = ({
  currentTab,
  onSelectTab,
  isLightMode,
  onToggleTheme,
  currentUser,
  allUsers,
  onSwitchUser,
  onQuickSimulate,
  isSimulating,
}) => {
  return (
    <header className="sticky top-0 z-40 flex items-center justify-between gap-8 px-6 py-3.5 border-b border-blue-500/20 bg-[#070b16]/90 backdrop-blur-md">
      {/* Zone 1: Single text element wordmark */}
      <div className="flex items-center gap-2.5 shrink-0">
        <div className="w-8 h-8 rounded-lg bg-blue-600/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
          <Shield className="w-5 h-5" />
        </div>
        <div>
          <a
            href="#"
            onClick={(e) => {
              e.preventDefault();
              onSelectTab('dashboard');
            }}
            className="text-lg font-bold tracking-wider text-[var(--text-primary)] uppercase font-heading whitespace-nowrap"
          >
            SENTINEL<span className="text-cyan-400">-XR 2.0</span>
          </a>
          <span className="block text-[9px] font-mono text-cyan-400 font-semibold tracking-wider">
            UNITY 6 URP EDITION
          </span>
        </div>
      </div>

      {/* Zone 2: Clean single-line text navigation links */}
      <nav className="hidden lg:flex items-center gap-6 text-xs font-medium text-slate-400">
        {[
          { id: 'dashboard', label: 'Dashboard' },
          { id: 'topology', label: '3D Topology' },
          { id: 'devices', label: 'Devices' },
          { id: 'threats', label: 'Threat Intel' },
          { id: 'incidents', label: 'Incidents' },
          { id: 'simulation', label: 'Sim Lab' },
          { id: 'copilot', label: 'AI Copilot' },
          { id: 'xr', label: 'XR Center' },
          { id: 'reports', label: 'Reports' },
        ].map((item) => (
          <button
            key={item.id}
            onClick={() => onSelectTab(item.id)}
            className={`transition-colors whitespace-nowrap py-1 relative ${
              currentTab === item.id
                ? 'text-cyan-400 font-semibold'
                : 'hover:text-slate-200'
            }`}
          >
            {item.label}
            {currentTab === item.id && (
              <span className="absolute bottom-0 left-0 w-full h-0.5 bg-cyan-400 rounded-full" />
            )}
          </button>
        ))}
      </nav>

      {/* Zone 3: 1 primary action + theme & user switch */}
      <div className="flex items-center gap-3 shrink-0">
        {/* User Role Selector */}
        <div className="hidden sm:flex items-center gap-1.5 glass-panel px-2.5 py-1 text-xs">
          <UserCheck className="w-3.5 h-3.5 text-blue-400" />
          <select
            value={currentUser.username}
            onChange={(e) => {
              const u = allUsers.find((x) => x.username === e.target.value);
              if (u) onSwitchUser(u);
            }}
            className="bg-transparent text-slate-300 font-mono text-[11px] focus:outline-none cursor-pointer"
            aria-label="Switch User Role"
          >
            {allUsers.map((u) => (
              <option key={u.id} value={u.username} className="bg-slate-900 text-slate-200">
                {u.role}: {u.username}
              </option>
            ))}
          </select>
        </div>

        {/* Theme Toggle */}
        <button
          onClick={onToggleTheme}
          className="p-1.5 rounded-lg glass-panel hover:text-cyan-400 text-slate-400 transition-colors"
          title={isLightMode ? 'Switch to Midnight Ops Theme' : 'Switch to Light Lab Theme'}
          aria-label="Toggle Theme"
        >
          {isLightMode ? <Moon className="w-4 h-4" /> : <Sun className="w-4 h-4 text-amber-400" />}
        </button>

        {/* Primary Action Button: Launch/Pause Simulation */}
        <button
          onClick={onQuickSimulate}
          className={`px-3.5 py-1.5 text-xs font-semibold rounded-lg transition-colors flex items-center gap-1.5 whitespace-nowrap ${
            isSimulating
              ? 'bg-rose-600 hover:bg-rose-500 text-white'
              : 'bg-blue-600 hover:bg-blue-500 text-white'
          }`}
          aria-label="Toggle Attack Simulation"
        >
          <Play className="w-3.5 h-3.5 fill-current" />
          {isSimulating ? 'Stop Simulation' : 'Run Simulation'}
        </button>
      </div>
    </header>
  );
};
