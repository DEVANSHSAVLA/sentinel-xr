# SENTINEL-XR: BREACH PROTOCOL — UNITY EDITOR OPENING & PLAY GUIDE

**Project Location:** `UnityProject/SentinelXR/`  
**Engine Version:** Unity 6 (`6000.0.23f1`)  
**Render Pipeline:** Universal Render Pipeline (URP 17.0.3)  
**Game Title:** SENTINEL-XR: BREACH PROTOCOL  
**Release Version:** `2.0.0-rc2`  

---

## 1. HOW TO OPEN THE PROJECT IN UNITY HUB

1. **Launch Unity Hub** on your workstation (Windows, macOS, or Linux).
2. Click the **"Add"** button (or **"Add project from disk"**).
3. In the file explorer, navigate to this repository and select the folder:
   ```
   UnityProject/SentinelXR
   ```
4. Confirm selection. The project will appear in your Unity Hub project list as **`SENTINEL-XR 2.0`**.
5. Ensure the Editor version is set to **`6000.0.23f1`** (Unity 6). If prompted, click to launch.
6. Unity will import packages (`manifest.json`), compile scripts under `Assets/Scripts/SentinelXR.asmdef`, and generate the project library.

---

## 2. HOW TO LAUNCH & PLAY THE GAME IN PLAY MODE

### Recommended Startup Path:
You can play the game starting from either the **Bootstrap Sequence** or directly from the **Main Menu**:

#### Option A: Full Bootstrap Sequence (Production Flow)
1. In the Project browser, open:
   ```
   Assets/Scenes/BootstrapScene.unity
   ```
2. Press the **Play** button (`Ctrl+P` / `Cmd+P`) at the top of the Unity Editor.
3. The bootloader initializes `GameManager` and `SceneTransitionManager`, displays the neural boot sequence, and transitions into `MainMenuScene`.

#### Option B: Direct Main Menu (Immediate Access)
1. In the Project browser, open:
   ```
   Assets/Scenes/MainMenuScene.unity
   ```
   *(Alternatively, use top menu: `SENTINEL-XR → Open Main Game Scene`)*
2. Press **Play**.
3. Click **"▶ LAUNCH MISSION 01: SIGNAL IN THE NOISE"** to enter the fully playable combat level.

---

## 3. GAMEPLAY CONTROLS & CYBER TOOLSET

### Movement & Interaction
- **W, A, S, D:** Operator Movement
- **Mouse:** 360° Freelook Pitch & Yaw
- **Left Shift (Hold):** Tactical Overclock Sprint
- **Spacebar:** Jump / Vertical Thruster
- **E Key:** Quick-Patch Network Node / Interact with Cyber Terminals
- **Escape Key:** Tactical Pause Menu (Resume, Restart, Abort to HQ)

### The 4 Defensive Cyber Tools
Equip tools using keys `1`, `2`, `3`, `4` or the **Mouse Scroll Wheel**:

| Key | Tool Name | Primary Action | Tactical Function |
|:---:|:---|:---|:---|
| **[1]** | **Trace Scanner** | Left-Click (Pulse) | Emits 360° electromagnetic sweep. Decodes node telemetry and exposes stealth **Trojan Infiltrators**. |
| **[2]** | **Patch Tool** | Left-Click (Hold Beam) | Projects a continuous repair beam. Restores network node integrity (HP) and purges corruption. Cleanses malware. |
| **[3]** | **Quarantine Pulse** | Left-Click / **Q** | Radial EMP wave. Stuns and freezes all hostile malware entities within 14 meters for 4 seconds, knocking them back. |
| **[4]** | **Firewall Shield** | Left-Click | Deploys a directional stateful energy barrier in front of the operator, deflecting incoming hostile SYN packets. |

---

## 4. MISSION 01 OBJECTIVES GUIDE

In **Mission 01: Signal in the Noise**, your goal is to secure DMZ Gateway Subnet Alpha:
1. **Objective 1 — Trace Anomalous Breach Signal:** Equip Trace Scanner `[1]` and scan the sector to identify corrupted nodes.
2. **Objective 2 — Repel Hostile Infiltration:** Defeat 5 incoming malware threat entities (Malware Bit-Worms, DDoS Flooders).
3. **Objective 3 — Patch DMZ Gateway Node:** Use Patch Tool `[2]` or press `E` on the Gateway node to restore integrity to 100%.
4. **Objective 4 — Neutralize Breach Core Alpha:** Once Gateway integrity is restored, Breach Core Alpha's protective shield drops. Target and destroy the core to seal the breach.

Upon victory, the **Forensic Debrief** screen generates a cryptographic SHA-256 evidence seal and awards bounty credits.

---

## 5. UNITY EDITOR VALIDATOR UTILITY

An integrated project validator is accessible directly from the Unity Editor top menu bar:
1. Click **`SENTINEL-XR → Validate Project`**.
2. An interactive Editor window will open, verifying:
   - Folder structure (`Assets/Scenes`, `Assets/Scripts`, `Assets/Prefabs`, `Assets/Materials`)
   - Scene asset files
   - Build Settings scene order
   - Assembly definitions
   - Package dependencies (URP 17.0.3, OpenXR 1.12.1, AR Foundation 6.0.2)
   - Project Version (`6000.0.23f1`)
3. Click **"Open MainGameScene"** to immediately jump to the menu.

---

## 6. STANDALONE BUILD INSTRUCTIONS

To build a standalone Windows executable from the Unity Editor:
1. Click **`SENTINEL-XR → Build → Windows Standalone (RC2)`**.
2. Or open **`File → Build Profiles`**, select **Windows Standalone (x86_64)**, and click **Build**.
3. Output executable will compile into `release/windows/SentinelXR.exe` and `Builds/Windows/`.
