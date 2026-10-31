from pydantic import BaseModel, Field
from typing import List, Optional, Literal

class NetworkNodeModel(BaseModel):
    id: str
    name: str
    type: str
    ip: str
    status: Literal['healthy', 'suspicious', 'critical', 'isolated'] = 'healthy'
    risk_score: int = Field(ge=0, le=100, default=15)
    zone: str
    services: List[str] = []
    cpu_usage: int = 25
    memory_usage: int = 40
    traffic_rate: int = 150

class ShapFactorModel(BaseModel):
    feature: str
    value: str
    impact: str
    contribution_score: float

class IncidentModel(BaseModel):
    id: str
    title: str
    severity: Literal['low', 'medium', 'high', 'critical']
    status: str
    scenario: str
    target_device_id: str
    source_ip: str
    timestamp: str
    description: str
    confidence_score: int
    shap_factors: List[ShapFactorModel] = []
    recommended_action: str
    containment_approved: bool = False

class SimulationRequestModel(BaseModel):
    scenario_id: Literal['ddos', 'bruteforce', 'portscan', 'lateral'] = 'ddos'
    target_device_id: str = 'server-01'
    intensity: Literal['low', 'medium', 'high', 'extreme'] = 'medium'

class AiInvestigateRequest(BaseModel):
    incident_id: Optional[str] = None
    device_id: Optional[str] = None
    user_prompt: str
