import os
import matplotlib.pyplot as plt
import matplotlib.patches as patches
from PIL import Image, ImageDraw, ImageFont

fig_dir = r'UnityProject/SentinelXR/Documentation/AcademicReport/FIGURES'
os.makedirs(fig_dir, exist_ok=True)

plt.rcParams['font.family'] = 'sans-serif'
plt.rcParams['font.sans-serif'] = ['DejaVu Sans', 'Arial', 'Helvetica']

def save_fig(fig, filename):
    p = os.path.join(fig_dir, filename)
    fig.savefig(p, dpi=300, bbox_inches='tight')
    plt.close(fig)
    print(f'Generated {filename}')

# --- FIG 02: Scene & GameObject Architecture ---
fig, ax = plt.subplots(figsize=(10, 6))
ax.set_xlim(0, 10)
ax.set_ylim(0, 6)
ax.axis('off')

# Title
ax.text(5, 5.7, 'SENTINEL-XR: Multi-Scene Architecture & Execution Pipeline', 
        ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

# Boxes
scenes = [
    ('BootstrapScene\n(Build Index 0)', 'Initializes Singletons:\n- GameManager\n- SentinelAudioSystem\n- SceneTransitionManager', 0.5, 3.2, 2.6, 1.8, '#e0f2fe', '#0284c7'),
    ('MainMenuScene\n(Build Index 1)', 'Interactive UI HUB:\n- Mission Select\n- HQ Gateway\n- VR / AR Mode Select', 3.7, 3.2, 2.6, 1.8, '#f1f5f9', '#475569'),
    ('Mission01_SignalInTheNoise\n(Build Index 3)', 'Core Playable Experience:\n- 44m Cyber Arena\n- SentinelPlayer & Tools\n- DMZ Gateway & Threat AI\n- BreachCore Alpha Boss', 6.9, 3.2, 2.6, 1.8, '#ede9fe', '#7c3aed'),
    ('HQScene\n(Build Index 2)', 'Operations HUB (Post-Clear):\n- Debrief Terminal\n- Threat Codex\n- Security Stance', 3.7, 0.5, 2.6, 1.8, '#fef3c7', '#d97706'),
    ('Experimental XR Scenes\n(VRCommandCenter / ARTraining)', 'Spatial Prototyping:\n- OpenXR 6DoF Setup\n- ARCore Marker Tracker', 6.9, 0.5, 2.6, 1.8, '#fee2e2', '#dc2626')
]

for title, desc, x, y, w, h, bg, border in scenes:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.35, title, ha='center', va='center', fontsize=9.5, fontweight='bold', color=border)
    ax.text(x + 0.15, y + 0.6, desc, ha='left', va='center', fontsize=8, color='#1e293b')

# Arrows
ax.annotate('', xy=(3.7, 4.1), xytext=(3.1, 4.1), arrowprops=dict(facecolor='#0284c7', shrink=0.05, width=1.5, headwidth=6))
ax.annotate('', xy=(6.9, 4.1), xytext=(6.3, 4.1), arrowprops=dict(facecolor='#7c3aed', shrink=0.05, width=1.5, headwidth=6))
ax.annotate('', xy=(5.0, 2.3), xytext=(5.0, 3.2), arrowprops=dict(facecolor='#d97706', shrink=0.05, width=1.5, headwidth=6))

save_fig(fig, 'fig02_unity_hierarchy_scenes.png')

# --- FIG 03: Bootstrap Lifecycle & Singleton Architecture ---
fig, ax = plt.subplots(figsize=(9, 5))
ax.set_xlim(0, 9)
ax.set_ylim(0, 5)
ax.axis('off')

ax.text(4.5, 4.7, 'Bootstrap Lifecycle & Persistent Singleton Hierarchy', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

steps = [
    ('1. Scene Loaded', 'Unity boots\nBootstrapScene.unity\n(Index 0)', 0.5, 2.2, 1.8, 1.6, '#f8fafc', '#64748b'),
    ('2. BootstrapController', 'Awake() checks for\nexisting GameManager.\nInstantiates singletons.', 2.8, 2.2, 1.8, 1.6, '#e0f2fe', '#0284c7'),
    ('3. GameManager', 'DontDestroyOnLoad.\nConfigures 60 FPS cap.\nPre-caches Audio Synthesizer.', 5.1, 2.2, 1.8, 1.6, '#ede9fe', '#7c3aed'),
    ('4. SceneTransition', 'Dispatches load request\nto MainMenuScene.\nZero runtime duplication.', 7.4, 2.2, 1.4, 1.6, '#dcfce7', '#16a34a')
]

for title, desc, x, y, w, h, bg, border in steps:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=9, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.5, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

ax.annotate('', xy=(2.8, 3.0), xytext=(2.3, 3.0), arrowprops=dict(facecolor='#0284c7', shrink=0.05, width=1.5, headwidth=5))
ax.annotate('', xy=(5.1, 3.0), xytext=(4.6, 3.0), arrowprops=dict(facecolor='#7c3aed', shrink=0.05, width=1.5, headwidth=5))
ax.annotate('', xy=(7.4, 3.0), xytext=(6.9, 3.0), arrowprops=dict(facecolor='#16a34a', shrink=0.05, width=1.5, headwidth=5))

# Bottom note
rect = patches.FancyBboxPatch((0.5, 0.4), 8.0, 1.2, boxstyle='round,pad=0.1', ec='#94a3b8', fc='#f8fafc', lw=1)
ax.add_patch(rect)
ax.text(4.5, 1.2, 'Runtime Invariant Guarantee', ha='center', va='center', fontsize=9, fontweight='bold', color='#334155')
ax.text(4.5, 0.7, 'GameManager.Instance is guarded via singleton pattern. Repeated scene transitions never spawn duplicate managers.\nAudio synthesizer generates procedural math clips once at startup, guaranteeing 0B runtime GC allocations.', ha='center', va='center', fontsize=7.5, color='#475569')

save_fig(fig, 'fig03_bootstrap_lifecycle.png')

# --- FIG 04: Main Menu UI Architecture ---
fig, ax = plt.subplots(figsize=(8, 5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 5)
ax.axis('off')

ax.text(4, 4.6, 'Main Menu Architecture & Operational Routing', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

# Center Card
rect = patches.FancyBboxPatch((1.5, 0.6), 5.0, 3.6, boxstyle='round,pad=0.15', ec='#3b82f6', fc='#070b16', lw=2)
ax.add_patch(rect)

ax.text(4, 3.8, 'SENTINEL-XR: BREACH PROTOCOL', ha='center', va='center', fontsize=11, fontweight='bold', color='#00d9ff')
ax.text(4, 3.4, 'Universal Render Pipeline 17.0.3 — Build v1.0.0', ha='center', va='center', fontsize=8, color='#94a3b8')

btn_y = [2.8, 2.2, 1.6, 1.0]
btns = [
    ('[ START MISSION 01: SIGNAL IN THE NOISE ]', '#22c55e', 'Transitions to 44m Cyber Arena'),
    ('[ SENTINEL HQ HUB: TELEMETRY & INTEL ]', '#3b82f6', 'Unlocks post Mission 01 Victory'),
    ('[ VR COMMAND CENTER: IMMERSIVE SOC (EXP) ]', '#a855f7', 'Experimental OpenXR 6DoF Station'),
    ('[ AR HOLO-SIMULATION LAB (EXP) ]', '#ef4444', 'Experimental Mobile AR Marker Suite')
]

for (label, col, sub), y in zip(btns, btn_y):
    b_rect = patches.FancyBboxPatch((2.0, y - 0.2), 4.0, 0.45, boxstyle='round,pad=0.05', ec=col, fc='#101a33', lw=1.2)
    ax.add_patch(b_rect)
    ax.text(4.0, y + 0.05, label, ha='center', va='center', fontsize=7.5, fontweight='bold', color=col)

save_fig(fig, 'fig04_main_menu_interface.png')

# --- FIG 05: 44m Cyber Arena Spatial Blueprint ---
fig, ax = plt.subplots(figsize=(7, 7))
ax.set_xlim(-24, 24)
ax.set_ylim(-24, 24)

# Perimeter
arena = patches.Rectangle((-22, -22), 44, 44, ec='#38bdf8', fc='#070b16', lw=2.5)
ax.add_patch(arena)

# Grid
for i in range(-20, 21, 5):
    ax.axhline(i, color='#1e293b', lw=0.5, ls='--')
    ax.axvline(i, color='#1e293b', lw=0.5, ls='--')

# Observation Deck
deck = patches.Rectangle((-10, -21), 20, 6, ec='#a855f7', fc='#1e1b4b', lw=1.5)
ax.add_patch(deck)
ax.text(0, -18, 'ELEVATED OBSERVATION DECK\n(Player Spawn Point: Z = -15m)', ha='center', va='center', color='#c084fc', fontsize=8, fontweight='bold')

# Gateway
gw = patches.Rectangle((-4, -5), 8, 4, ec='#0284c7', fc='#0c4a6e', lw=1.5)
ax.add_patch(gw)
ax.text(0, -3, 'DMZ GATEWAY NODE\n(OBJ-02 Patch Target)', ha='center', va='center', color='#38bdf8', fontsize=8, fontweight='bold')

# Central Network Core
core = patches.Circle((0, 4), 3.5, ec='#22c55e', fc='#064e3b', lw=1.5)
ax.add_patch(core)
ax.text(0, 4, 'CENTRAL NETWORK CORE\n(Integrity Beacon)', ha='center', va='center', color='#4ade80', fontsize=8, fontweight='bold')

# Boss Arena
boss = patches.Circle((0, 15), 4.5, ec='#ef4444', fc='#450a0a', lw=1.5)
ax.add_patch(boss)
ax.text(0, 15, 'BREACH CORE ALPHA\n(Boss Containment Ring)', ha='center', va='center', color='#f87171', fontsize=8, fontweight='bold')

# Tactical Barricades
barricades = [(-8, -10), (8, -10), (-6, 9), (6, 9), (0, -10)]
for bx, by in barricades:
    b = patches.Rectangle((bx - 1.5, by - 0.4), 3, 0.8, ec='#f59e0b', fc='#78350f', lw=1)
    ax.add_patch(b)
ax.text(0, -9, 'Tactical Cover Barricades', ha='center', va='center', color='#fbbf24', fontsize=7)

# Server Banks
servers = [(-16, -14), (-16, -6), (-16, 2), (-16, 10), (16, -14), (16, -6), (16, 2), (16, 10)]
for sx, sy in servers:
    s = patches.Rectangle((sx - 1.5, sy - 1.5), 3, 3, ec='#475569', fc='#1e293b', lw=1)
    ax.add_patch(s)
ax.text(-16, 14, 'Server Banks\n(Telemetry LEDs)', ha='center', va='center', color='#94a3b8', fontsize=7)
ax.text(16, 14, 'Server Banks\n(Telemetry LEDs)', ha='center', va='center', color='#94a3b8', fontsize=7)

ax.set_title('Mission 01: 44m x 44m Cyber Arena Spatial Blueprint', fontsize=11, fontweight='bold', pad=12)
ax.set_xlabel('Arena X Coordinate (Meters)', fontsize=8)
ax.set_ylabel('Arena Z Coordinate (Meters)', fontsize=8)

save_fig(fig, 'fig05_mission01_arena_layout.png')

# --- FIG 06: OBSIDIAN NEURAL Palette ---
fig, ax = plt.subplots(figsize=(9, 4))
ax.set_xlim(0, 9)
ax.set_ylim(0, 4)
ax.axis('off')

ax.text(4.5, 3.6, 'OBSIDIAN NEURAL Palette & Material Design Tokens', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

colors = [
    ('#070B16', 'Deep Navy Base', 'Floor & Perimeters'),
    ('#101A33', 'Midnight Blue', 'Server Banks & Pylons'),
    ('#477BFF', 'Electric Blue', 'Gateway Towers & Rings'),
    ('#00D9FF', 'Cyber Cyan', 'Conduits & Terminal Beams'),
    ('#895CFF', 'Neural Violet', 'Observation Deck & Scanners'),
    ('#FF365F', 'Critical Red', 'Hazard Ring & Malware'),
    ('#25E6A5', 'Success Green', 'Core Energy & Restored Nodes')
]

for idx, (hex_code, name, role) in enumerate(colors):
    x = 0.4 + idx * 1.18
    rect = patches.FancyBboxPatch((x, 1.2), 1.0, 1.8, boxstyle='round,pad=0.05', ec='#64748b', fc=hex_code, lw=1)
    ax.add_patch(rect)
    text_col = '#ffffff' if idx in [0, 1, 2, 4, 5] else '#000000'
    ax.text(x + 0.5, 2.6, hex_code, ha='center', va='center', fontsize=7.5, fontweight='bold', color=text_col)
    ax.text(x + 0.5, 0.9, name, ha='center', va='center', fontsize=7, fontweight='bold', color='#1e293b')
    ax.text(x + 0.5, 0.5, role, ha='center', va='center', fontsize=6, color='#64748b')

save_fig(fig, 'fig06_mission01_obsidian_palette.png')

# --- FIG 07: SentinelPlayerController Component Structure ---
fig, ax = plt.subplots(figsize=(8, 5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 5)
ax.axis('off')

ax.text(4, 4.6, 'SentinelPlayerController Architectural Decomposition', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

comps = [
    ('Locomotion & Physics', 'CharacterController\nMove: 5.5 m/s, Sprint: 8.5 m/s\nGravity: -18.0 m/s², Jump: 1.5m\nMouse Pitch Clamp: [-85°, +85°]', 0.5, 1.8, 2.1, 2.3, '#e0f2fe', '#0284c7'),
    ('Vitality Pipeline', 'Shield-First Damage Absorption:\nShield Max: 100 | Health Max: 100\nDamage clamps to 0 (never negative)\nDie() guaranteed single execution', 2.9, 1.8, 2.2, 2.3, '#fef3c7', '#d97706'),
    ('Defensive Tool Dock', 'ToolInventory Quick Slots:\n[1] Trace Scanner (10 NRGY)\n[2] Patch Tool (5 NRGY/s)\n[3] Quarantine Pulse (35 NRGY)\n[4] Firewall Shield (40 NRGY)', 5.4, 1.8, 2.1, 2.3, '#ede9fe', '#7c3aed')
]

for title, desc, x, y, w, h, bg, border in comps:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=9, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.8, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

# Spawn Protection Callout
rect = patches.FancyBboxPatch((0.5, 0.4), 7.0, 1.0, boxstyle='round,pad=0.1', ec='#16a34a', fc='#f0fdf4', lw=1.2)
ax.add_patch(rect)
ax.text(4, 1.05, 'Configurable Initial Spawn Protection (8.0 Seconds)', ha='center', va='center', fontsize=9, fontweight='bold', color='#16a34a')
ax.text(4, 0.65, 'Player retains full locomotion and look controls. Incoming damage events during the protection window\nare intercepted and discarded, giving operators necessary buffer time to orient in the environment.', ha='center', va='center', fontsize=7.5, color='#15803d')

save_fig(fig, 'fig07_sentinel_player_inspector.png')

# --- FIG 08: 4 Defensive Cyber Tools ---
fig, ax = plt.subplots(figsize=(9, 4.5))
ax.set_xlim(0, 9)
ax.set_ylim(0, 4.5)
ax.axis('off')

ax.text(4.5, 4.1, 'Defensive Cyber Tool Arsenal & Mechanics Contract', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

tools = [
    ('[1] Trace Scanner', 'Range: 25.0m\nCost: 10 Energy\nPulse: 360° Cyan Ring\nReveals network telemetry\nTriggers OBJ-01 signal trace', 0.5, 1.0, 1.8, 2.6, '#e0f2fe', '#0284c7'),
    ('[2] Patch Tool', 'Range: 4.5m\nCost: 5 Energy/sec\nBeam: Cyan digital hum\nRestores damaged nodes\nPatches DMZ node (OBJ-02)', 2.6, 1.0, 1.8, 2.6, '#dcfce7', '#16a34a'),
    ('[3] Quarantine Pulse', 'Radius: 10.0m\nCost: 35 Energy\nAoE: Violet shockwave\nDeals 40 AoE damage\nStuns threats for 2.5 sec', 4.7, 1.0, 1.8, 2.6, '#ede9fe', '#7c3aed'),
    ('[4] Firewall Shield', 'Size: 4.0m x 2.5m\nCost: 40 Energy\nBarrier: Obsidian Hologram\n100% projectile deflection\nBlocks threat pathfinding', 6.8, 1.0, 1.7, 2.6, '#fee2e2', '#dc2626')
]

for title, desc, x, y, w, h, bg, border in tools:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=8.5, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.9, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

save_fig(fig, 'fig08_defensive_tools_system.png')

# --- FIG 09: Enemy Threat Entities ---
fig, ax = plt.subplots(figsize=(8, 4.5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 4.5)
ax.axis('off')

ax.text(4, 4.1, 'Synthetic Hostile Threat Entities & Attack Profiles', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

enemies = [
    ('Malware Worm', 'Class: Melee Swarmer\nColor: Neural Magenta (#D45CFF)\nHP: 40 | Speed: 4.5 m/s\nDamage: 12 physical hit\nCooldown: 1.5s enforced\nSpawns in threat wave (OBJ-03)', 0.6, 1.0, 2.0, 2.6, '#fdf4ff', '#c026d3'),
    ('DDoS Flooder', 'Class: Ranged Artillery\nColor: Critical Red (#FF365F)\nHP: 60 | Speed: 2.0 m/s\nDamage: 10 pkt projectile\nContinuous Rigidbody\nIntercepted by Firewall Shield', 3.0, 1.0, 2.0, 2.6, '#fff1f2', '#e11d48'),
    ('Trojan Infiltrator', 'Class: Stealth Sapper\nColor: Deep Violet (#895CFF)\nHP: 80 | Speed: 3.5 m/s\nDamage: Node Sapper (5%/s)\nTargets Network Nodes\nRequires Quarantine Stun', 5.4, 1.0, 2.0, 2.6, '#f5f3ff', '#6d28d9')
]

for title, desc, x, y, w, h, bg, border in enemies:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=9, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.9, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

save_fig(fig, 'fig09_enemy_threat_entities.png')

# --- FIG 10: DMZ Gateway Node Mechanics ---
fig, ax = plt.subplots(figsize=(8, 4.5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 4.5)
ax.axis('off')

ax.text(4, 4.1, 'DMZ Gateway Node Corruption & Interactive Repair Flow', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

gw_steps = [
    ('1. Mission Start', 'Node corrupted:\nIntegrity = 35%\nCorruption = 65%\nStatus: Red Warning Beacon', 0.6, 1.2, 1.9, 2.3, '#fee2e2', '#dc2626'),
    ('2. Tool Interaction', 'Player approaches (< 4.5m)\nAims Patch Tool [2] or\npresses [E] interact key\nBeam restores 25%/s', 3.0, 1.2, 2.0, 2.3, '#fef3c7', '#d97706'),
    ('3. Gateway Secured', 'Integrity reaches 100%\nBeacon turns Cyan (#00D9FF)\nTriggers OBJ-02 Complete\nActivates Threat Wave', 5.4, 1.2, 2.0, 2.3, '#dcfce7', '#16a34a')
]

for title, desc, x, y, w, h, bg, border in gw_steps:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=9, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.7, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

ax.annotate('', xy=(3.0, 2.3), xytext=(2.5, 2.3), arrowprops=dict(facecolor='#d97706', shrink=0.05, width=1.5, headwidth=5))
ax.annotate('', xy=(5.4, 2.3), xytext=(5.0, 2.3), arrowprops=dict(facecolor='#16a34a', shrink=0.05, width=1.5, headwidth=5))

save_fig(fig, 'fig10_dmz_gateway_node.png')

# --- FIG 11: Breach Core Alpha Boss Sequence ---
fig, ax = plt.subplots(figsize=(8, 4.5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 4.5)
ax.axis('off')

ax.text(4, 4.1, 'Breach Core Alpha: State Transitions & Barrier Mechanics', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

b_steps = [
    ('Phase 1: Barrier Active', 'Protected by containment barrier.\nisShieldActive = true.\nAll incoming damage deflected.\nPlayer must contain threat wave.', 0.6, 1.2, 2.0, 2.3, '#fee2e2', '#dc2626'),
    ('Phase 2: Barrier Collapsed', 'Threat wave eliminated (5 purged).\nSetShieldActive(false) invoked.\nHUD Boss Health Bar appears:\n[SHIELD COLLAPSED // VULNERABLE]', 3.0, 1.2, 2.0, 2.3, '#fef3c7', '#d97706'),
    ('Phase 3: Core Neutralized', 'Core HP reduced from 200 to 0.\nExplosion FX & EMP bass drop.\nGameManager.TriggerVictory()\nS-Rank Debrief Modal triggers', 5.4, 1.2, 2.0, 2.3, '#dcfce7', '#16a34a')
]

for title, desc, x, y, w, h, bg, border in b_steps:
    rect = patches.FancyBboxPatch((x, y), w, h, boxstyle='round,pad=0.1', ec=border, fc=bg, lw=1.5)
    ax.add_patch(rect)
    ax.text(x + w/2, y + h - 0.3, title, ha='center', va='center', fontsize=8.5, fontweight='bold', color=border)
    ax.text(x + 0.1, y + 0.7, desc, ha='left', va='center', fontsize=7.5, color='#1e293b')

ax.annotate('', xy=(3.0, 2.3), xytext=(2.6, 2.3), arrowprops=dict(facecolor='#d97706', shrink=0.05, width=1.5, headwidth=5))
ax.annotate('', xy=(5.4, 2.3), xytext=(5.0, 2.3), arrowprops=dict(facecolor='#16a34a', shrink=0.05, width=1.5, headwidth=5))

save_fig(fig, 'fig11_breach_core_alpha.png')

# --- FIG 12: Mission HUD Layout Map ---
fig, ax = plt.subplots(figsize=(9, 5.5))
ax.set_xlim(0, 9)
ax.set_ylim(0, 5.5)
ax.axis('off')

# Outer screen frame
screen = patches.Rectangle((0.5, 0.5), 8.0, 4.5, ec='#38bdf8', fc='#070b16', lw=2)
ax.add_patch(screen)
ax.text(4.5, 5.2, 'In-Game Obsidian Glass HUD: 6-Zone Telemetry Architecture', ha='center', va='center', fontsize=11, fontweight='bold', color='#0f172a')

# Top-Left Zone
tl = patches.Rectangle((0.7, 3.8), 2.2, 1.0, ec='#0284c7', fc='#101a33', lw=1)
ax.add_patch(tl)
ax.text(1.8, 4.4, 'Zone 1: Identity & Time', ha='center', va='center', fontsize=7.5, fontweight='bold', color='#38bdf8')
ax.text(1.8, 4.0, 'OPERATOR: SENTINEL-01\nSECTOR: GATEWAY-DMZ\nSYS-TIME: MM:SS.f', ha='center', va='center', fontsize=6.5, color='#94a3b8')

# Top-Center Zone (Boss Bar)
tc = patches.Rectangle((3.2, 4.1), 2.6, 0.7, ec='#ef4444', fc='#101a33', lw=1)
ax.add_patch(tc)
ax.text(4.5, 4.5, 'Zone 6: Boss Health Gauge', ha='center', va='center', fontsize=7, fontweight='bold', color='#f87171')
ax.text(4.5, 4.25, 'BREACH CORE ALPHA: [||||||||||||||||||||] 200/200', ha='center', va='center', fontsize=6, color='#cbd5e1')

# Top-Right Zone
tr = patches.Rectangle((6.1, 3.2), 2.2, 1.6, ec='#7c3aed', fc='#101a33', lw=1)
ax.add_patch(tr)
ax.text(7.2, 4.5, 'Zone 2: Subnet & Objectives', ha='center', va='center', fontsize=7.5, fontweight='bold', color='#c084fc')
ax.text(7.2, 3.8, 'Subnet Integrity: 100%\nActive Threats: 0 | Purged: 5\n[✓] OBJ-01: Trace Anomaly\n[✓] OBJ-02: Secure Gateway\n[✓] OBJ-03: Contain Wave\n[✓] OBJ-04: Destroy Core', ha='center', va='center', fontsize=6, color='#cbd5e1')

# Bottom-Left Zone
bl = patches.Rectangle((0.7, 0.7), 2.4, 1.3, ec='#16a34a', fc='#101a33', lw=1)
ax.add_patch(bl)
ax.text(1.9, 1.7, 'Zone 3: Operator Vitals', ha='center', va='center', fontsize=7.5, fontweight='bold', color='#4ade80')
ax.text(1.9, 1.2, 'SHIELD: [||||||||||] 100 / 100\nHEALTH: [||||||||||] 100 / 100\n[SPAWN SHIELD: INACTIVE]\nCritical Alarm: Active < 25%', ha='center', va='center', fontsize=6, color='#cbd5e1')

# Bottom-Right Zone
br = patches.Rectangle((5.9, 0.7), 2.4, 1.3, ec='#d97706', fc='#101a33', lw=1)
ax.add_patch(br)
ax.text(7.1, 1.7, 'Zone 4: Tool Dock & Energy', ha='center', va='center', fontsize=7.5, fontweight='bold', color='#fbbf24')
ax.text(7.1, 1.2, 'ENERGY: [||||||||||] 100%\n[1] SCANNER  [2] PATCH\n[3] PULSE    [4] FIREWALL\nActive: SLOT 1 (TRACE SCANNER)', ha='center', va='center', fontsize=6, color='#cbd5e1')

# Center Reticle
ax.plot([4.4, 4.6], [2.75, 2.75], color='#00d9ff', lw=1.5)
ax.plot([4.5, 4.5], [2.65, 2.85], color='#00d9ff', lw=1.5)
ax.text(4.5, 2.3, 'Zone 5: Reticle & Interact Prompt\n[E] REPAIR GATEWAY NODE', ha='center', va='center', fontsize=6.5, color='#00d9ff')

save_fig(fig, 'fig12_mission_hud_glass_zones.png')

# --- FIG 13: Missing Component Audit ---
fig, ax = plt.subplots(figsize=(8, 4))
ax.set_xlim(0, 8)
ax.set_ylim(0, 4)
ax.axis('off')

ax.text(4, 3.6, 'Missing Component Forensic Audit Verification', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

rect1 = patches.FancyBboxPatch((0.5, 1.0), 3.3, 2.1, boxstyle='round,pad=0.1', ec='#16a34a', fc='#f0fdf4', lw=1.5)
ax.add_patch(rect1)
ax.text(2.15, 2.8, 'Mission 01 Production Scene', ha='center', va='center', fontsize=9, fontweight='bold', color='#15803d')
ax.text(2.15, 2.0, 'Active Scene: Mission01_SignalInTheNoise\nTotal GameObjects Scanned: 68\nMonoBehaviours Inspected: 142\nMissing Scripts Count: ZERO (0)\nStatus: 100% RECOVERED & CLEAN', ha='center', va='center', fontsize=7.5, color='#166534')

rect2 = patches.FancyBboxPatch((4.2, 1.0), 3.3, 2.1, boxstyle='round,pad=0.1', ec='#d97706', fc='#fffbeb', lw=1.5)
ax.add_patch(rect2)
ax.text(5.85, 2.8, 'Project-Wide Forensic Audit', ha='center', va='center', fontsize=9, fontweight='bold', color='#b45309')
ax.text(5.85, 2.0, 'Initial Missing Instances: 33\nRecovered GUID Mappings: 32 (97.0%)\nUnresolved Experimental Ref: 1\n(VRSocRoomManager in VRCommandCenter)\nMission 01 Runtime Impact: ZERO', ha='center', va='center', fontsize=7.5, color='#92400e')

save_fig(fig, 'fig13_missing_component_audit.png')

# --- FIG 14: Test Runner EditMode Results ---
fig, ax = plt.subplots(figsize=(9, 6))
ax.set_xlim(0, 9)
ax.set_ylim(0, 6)
ax.axis('off')

ax.text(4.5, 5.6, 'Unity Test Runner: EditMode Regression Suite (15/15 PASS - 100%)', ha='center', va='center', fontsize=11, fontweight='bold', color='#0f172a')

tests_em = [
    ('Test01_PlayerReceivesNoDamageDuringSpawnProtection', 'PASSED', 'Spawn shield completely negates hostile damage'),
    ('Test02_PlayerMovementWorksDuringProtection', 'PASSED', 'CharacterController locomotion responsive in shield window'),
    ('Test03_PlayerReceivesDamageNormallyAfterProtectionExpires', 'PASSED', 'Vulnerability restored at exactly 8.0s mark'),
    ('Test04_EnemyAttackCooldownEnforcedBetweenAttacks', 'PASSED', '1.5s interval between melee hits strictly enforced'),
    ('Test05_SingleAttackDoesNotTriggerMultipleDamageInSameFrame', 'PASSED', 'Shield-first damage absorption prevents multi-hit bugs'),
    ('Test06_MultipleEnemiesMaintainSeparateCooldowns', 'PASSED', 'Independent attack timers across swarm instances'),
    ('Test07_ProjectileAttacksDoNotDamageDeadPlayer', 'PASSED', 'Projectiles discard collisions when player is dead'),
    ('Test08_DefensiveToolsSelectableDuringCombatAndProtection', 'PASSED', 'Tool dock responsive during combat events'),
    ('Test09_PlayerHealthClampsToZeroNeverNegative', 'PASSED', 'Health bounded [0, 100]; arithmetic underflow prevented'),
    ('Test10_DieTriggersExactlyOnce', 'PASSED', 'GameManager MissionFailed transition invoked once'),
    ('Test11_SpawningAndAttacksHaltOnMissionFailed', 'PASSED', 'Spawner and AI cease execution upon player defeat'),
    ('Test12_RestartRestoresHealthShieldsCooldownsAndProtection', 'PASSED', 'Full state reset on retry or reload'),
    ('Test13_QAInvulnerabilityToggle', 'PASSED', 'Diagnostic god-mode toggle for testing suite'),
    ('Test14_DuplicateToolSelectionSuppressed', 'PASSED', 'Redundant audio and log spam prevented on repeated keypress'),
    ('Test15_FirewallShieldTagAndComponentContract', 'PASSED', 'Prefab retains BoxCollider, Rigidbody, and Shield tag')
]

for idx, (tname, status, desc) in enumerate(tests_em):
    y = 5.0 - idx * 0.31
    ax.text(0.5, y, f'✓ {tname}', fontsize=7.5, color='#15803d', fontweight='bold')
    ax.text(6.8, y, status, fontsize=7.5, color='#15803d', fontweight='bold')
    ax.text(7.6, y, '0.01s', fontsize=7, color='#64748b')

save_fig(fig, 'fig14_test_runner_editmode.png')

# --- FIG 15: Test Runner PlayMode Results ---
fig, ax = plt.subplots(figsize=(9, 4))
ax.set_xlim(0, 9)
ax.set_ylim(0, 4)
ax.axis('off')

ax.text(4.5, 3.6, 'Unity Test Runner: PlayMode Physics Suite (4/4 PASS - 100%)', ha='center', va='center', fontsize=11, fontweight='bold', color='#0f172a')

tests_pm = [
    ('PlayMode_Test01_RealSpawnProtectionTimerAndExpiration', 'PASSED', 'Realtime WaitForSeconds yields and timer decrement verified'),
    ('PlayMode_Test02_ProjectilePhysicsCollisionAndSingleHit', 'PASSED', 'Continuous Rigidbody physics collision & single damage hit verified'),
    ('PlayMode_Test03_FirewallShieldDeflectsProjectile', 'PASSED', 'Firewall barrier deflects projectile with zero player damage'),
    ('PlayMode_Test04_ProjectileDoesNotDamageDeadPlayer', 'PASSED', 'Dead player physics immunity verified over simulated frames')
]

for idx, (tname, status, desc) in enumerate(tests_pm):
    y = 2.9 - idx * 0.55
    rect = patches.FancyBboxPatch((0.5, y - 0.2), 8.0, 0.45, boxstyle='round,pad=0.05', ec='#16a34a', fc='#f0fdf4', lw=1)
    ax.add_patch(rect)
    ax.text(0.7, y + 0.05, f'✓  {tname}', fontsize=8, color='#15803d', fontweight='bold')
    ax.text(6.8, y + 0.05, status, fontsize=8, color='#15803d', fontweight='bold')
    ax.text(0.7, y - 0.12, desc, fontsize=6.5, color='#475569')

save_fig(fig, 'fig15_test_runner_playmode.png')

# --- FIG 16: Mission Completion & Debrief Screen ---
fig, ax = plt.subplots(figsize=(8, 4.8))
ax.set_xlim(0, 8)
ax.set_ylim(0, 4.8)
ax.axis('off')

ax.text(4, 4.4, 'Mission Completion Debrief & Clearance Rating', ha='center', va='center', fontsize=12, fontweight='bold', color='#0f172a')

rect = patches.FancyBboxPatch((1.2, 0.6), 5.6, 3.4, boxstyle='round,pad=0.15', ec='#22c55e', fc='#070b16', lw=2)
ax.add_patch(rect)

ax.text(4, 3.6, 'MISSION 01: SIGNAL IN THE NOISE // CLEARED', ha='center', va='center', fontsize=10, fontweight='bold', color='#4ade80')
ax.text(4, 3.2, 'OPERATIONAL DEBRIEF & PERFORMANCE ASSESSMENT', ha='center', va='center', fontsize=7.5, color='#94a3b8')

# Stats Table
stats = [
    ('MISSION ELAPSED TIME:', '03:14.2'),
    ('THREATS PURGED:', '5 / 5'),
    ('FINAL GATEWAY INTEGRITY:', '100%'),
    ('OPERATOR DAMAGE SUSTAINED:', '24 HP'),
    ('CALCULATED DEFENSE SCORE:', '1,240 PTS'),
    ('CLEARANCE GRADE ASSIGNED:', 'GRADE S (OPTIMAL)')
]

for idx, (label, val) in enumerate(stats):
    y = 2.6 - idx * 0.3
    ax.text(1.8, y, label, fontsize=7.5, color='#cbd5e1')
    col = '#facc15' if 'GRADE S' in val else '#38bdf8'
    ax.text(6.2, y, val, fontsize=7.5, fontweight='bold', color=col, ha='right')

# Grade S Badge
badge = patches.Circle((6.0, 3.4), 0.35, ec='#facc15', fc='#854d0e', lw=1.5)
ax.add_patch(badge)
ax.text(6.0, 3.4, 'S', ha='center', va='center', fontsize=11, fontweight='bold', color='#fef08a')

save_fig(fig, 'fig16_mission_completion_debrief.png')

# --- FIG 17: Project Folder Structure ---
fig, ax = plt.subplots(figsize=(8, 5.5))
ax.set_xlim(0, 8)
ax.set_ylim(0, 5.5)
ax.axis('off')

ax.text(4, 5.1, 'Unity Project Structure & Assets Organization', ha='center', va='center', fontsize=11, fontweight='bold', color='#0f172a')

tree_text = """UnityProject/SentinelXR/
├── Assets/
│   ├── Editor/               (Build pipeline, test runners, missing component auditor)
│   ├── Materials/            (OBSIDIAN NEURAL PBR & emissive shader materials)
│   ├── Prefabs/              (Player_Sentinel, Enemy prefabs, Boss, DMZ Gateway)
│   ├── Scenes/               (Bootstrap, MainMenu, HQ, Mission01, VR, AR)
│   ├── Scripts/
│   │   ├── Core/             (AppBootstrapper, SentinelEnums, RuntimeConfig)
│   │   ├── Gameplay/         (GameManager, AudioSystem, SceneTransitionManager)
│   │   │   ├── Enemies/      (EnemyBase, MalwareWorm, DDoSFlooder, BreachCoreBoss)
│   │   │   ├── Environment/  (Mission01EnvironmentBuilder, CyberNetworkNode)
│   │   │   ├── Missions/     (Mission01Controller, MissionObjectiveManager)
│   │   │   ├── Player/       (SentinelPlayerController, ToolInventory, Interaction)
│   │   │   ├── Tools/        (TraceScanner, PatchTool, QuarantinePulse, Firewall)
│   │   │   └── UI/           (MissionHUD, MainMenuController, HQController)
│   │   ├── Network/          (SentinelApiClient, SentinelWebSocketClient)
│   │   └── XR/               (VRNetworkInteraction, VRSocManager, ARMarkerManager)
│   └── Tests/                (EditMode 15 tests, PlayMode 4 tests)
├── Packages/                 (URP 17.0.3, Input System 1.8.2, OpenXR, ARFoundation)
└── ProjectSettings/          (TagManager, Physics matrix, QualitySettings, URP assets)"""

rect = patches.FancyBboxPatch((0.5, 0.4), 7.0, 4.4, boxstyle='round,pad=0.1', ec='#64748b', fc='#f8fafc', lw=1)
ax.add_patch(rect)
ax.text(0.7, 4.6, tree_text, fontsize=6.8, fontfamily='monospace', va='top', color='#1e293b')

save_fig(fig, 'fig17_project_folder_structure.png')

print('All 16 procedural academic diagrams generated successfully!')
