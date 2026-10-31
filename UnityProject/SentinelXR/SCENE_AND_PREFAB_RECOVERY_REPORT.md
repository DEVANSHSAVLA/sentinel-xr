# SCENE_AND_PREFAB_RECOVERY_REPORT

## SENTINEL-XR — Scene & Prefab Missing Component Recovery Analysis
**Audit Date:** 2026-10-31 06:46:27

---

## 1. Asset Breakdown

### `VRCommandCenter.unity` (1 missing instances)

| GameObject | Component ID | Missing GUID | Intended Class | Actual Script Path | Status |
|------------|--------------|--------------|----------------|--------------------|--------|
| `XR Origin (VR Command Center)` | 102 | `00000000000000000000000000000005` | `VRSocRoomManager` | `` | **NOT RECOVERABLE (Missing C# Script)** |

---

## 2. Recovery Classification Matrix

| Category | Count | Criteria | Action |
|----------|-------|----------|--------|
| **RECOVERABLE** | 32 | Exact C# script exists in `Assets/Scripts/` matching `m_EditorClassIdentifier`. Real GUID known. | Can be safely re-linked via GUID substitution or component reattachment when authorized. |
| **NOT RECOVERABLE** | 1 | Script class name (`VRSocRoomManager`) does not exist on disk (renamed to `VRSocManager`). | Requires manual confirmation before attaching `VRSocManager`. |
| **BLOCKED** | 0 | No conflicting or ambiguous GUID collisions found. | N/A |
