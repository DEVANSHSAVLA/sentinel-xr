from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from typing import List, Dict, Any
import json
import asyncio
from datetime import datetime

from backend.models import NetworkNodeModel, IncidentModel, SimulationRequestModel, AiInvestigateRequest
from backend.ml_engine import classify_attack

app = FastAPI(title="SENTINEL-XR 2.0 FastAPI Service", version="2.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# In-Memory State
nodes_db: Dict[str, Dict[str, Any]] = {
    "gateway-01": {"id": "gateway-01", "name": "Internet Gateway", "type": "gateway", "ip": "198.51.100.1", "status": "healthy", "risk_score": 12, "zone": "external", "services": ["BGP", "Scrubbing"], "cpu_usage": 20, "memory_usage": 30, "traffic_rate": 800},
    "firewall-01": {"id": "firewall-01", "name": "Perimeter NextGen Firewall", "type": "firewall", "ip": "10.0.0.1", "status": "healthy", "risk_score": 18, "zone": "perimeter", "services": ["IPS/IDS", "TLS Proxy"], "cpu_usage": 40, "memory_usage": 50, "traffic_rate": 820},
    "router-core": {"id": "router-core", "name": "Core Distribution Router", "type": "router", "ip": "10.0.0.2", "status": "healthy", "risk_score": 15, "zone": "internal", "services": ["OSPF", "VLANs"], "cpu_usage": 30, "memory_usage": 40, "traffic_rate": 1400},
    "server-01": {"id": "server-01", "name": "DMZ Web App Cluster", "type": "server", "ip": "10.0.1.10", "status": "healthy", "risk_score": 22, "zone": "dmz", "services": ["HTTPS (443)", "Nginx"], "cpu_usage": 35, "memory_usage": 60, "traffic_rate": 410},
    "server-02": {"id": "server-02", "name": "DMZ API Gateway", "type": "server", "ip": "10.0.1.15", "status": "healthy", "risk_score": 25, "zone": "dmz", "services": ["REST API", "OAuth"], "cpu_usage": 45, "memory_usage": 55, "traffic_rate": 530},
    "db-cluster-01": {"id": "db-cluster-01", "name": "Secure Vault Database", "type": "database", "ip": "10.0.2.20", "status": "healthy", "risk_score": 10, "zone": "secure_core", "services": ["PostgreSQL", "Redis"], "cpu_usage": 28, "memory_usage": 70, "traffic_rate": 290},
    "workstation-01": {"id": "workstation-01", "name": "SOC Analyst Workstation", "type": "workstation", "ip": "10.0.3.14", "status": "healthy", "risk_score": 14, "zone": "internal", "services": ["EDR Agent"], "cpu_usage": 20, "memory_usage": 45, "traffic_rate": 60},
    "workstation-02": {"id": "workstation-02", "name": "DevOps Terminal", "type": "workstation", "ip": "10.0.3.18", "status": "healthy", "risk_score": 16, "zone": "internal", "services": ["Docker", "Kubectl"], "cpu_usage": 30, "memory_usage": 50, "traffic_rate": 90},
    "client-pool": {"id": "client-pool", "name": "Simulated Client Subnet", "type": "client", "ip": "10.0.4.50", "status": "healthy", "risk_score": 8, "zone": "internal", "services": ["DHCP", "VPN"], "cpu_usage": 15, "memory_usage": 30, "traffic_rate": 180}
}

active_connections: List[WebSocket] = []

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    active_connections.append(websocket)
    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        active_connections.remove(websocket)

async def broadcast_ws(message: Dict[str, Any]):
    text = json.dumps(message)
    for conn in active_connections:
        await conn.send_text(text)

@app.get("/api/v1/health")
def get_health():
    return {
        "status": "healthy",
        "service": "SENTINEL-XR 2.0 FastAPI Backend",
        "active_nodes": len(nodes_db),
        "timestamp": datetime.utcnow().isoformat()
    }

@app.get("/api/v1/devices")
def get_devices():
    return list(nodes_db.values())

@app.post("/api/v1/devices/{device_id}/isolate")
async def isolate_device(device_id: str, payload: Dict[str, bool]):
    if device_id in nodes_db:
        isolate = payload.get("isolate", True)
        nodes_db[device_id]["status"] = "isolated" if isolate else "healthy"
        await broadcast_ws({"type": "TOPOLOGY_UPDATE", "data": {"nodes": list(nodes_db.values())}})
        return {"success": True, "device": nodes_db[device_id]}
    return {"error": "Device not found"}, 404

@app.get("/api/v1/network/topology")
def get_topology():
    return {"nodes": list(nodes_db.values())}

@app.post("/api/v1/simulation/start")
async def start_simulation(req: SimulationRequestModel):
    await broadcast_ws({
        "type": "SIMULATION_UPDATE",
        "data": {"status": "running", "scenario": req.scenario_id, "target": req.target_device_id}
    })
    return {"success": True, "scenario": req.scenario_id}
