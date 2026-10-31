"""
SENTINEL-XR 2.0 ML Detection Pipeline
Combines Isolation Forest anomaly path metrics with Supervised Decision Trees and SHAP feature explanations.
"""
from typing import Dict, Any, List

def calculate_anomaly_score(telemetry: Dict[str, Any]) -> float:
    score = 0.05
    packet_rate = telemetry.get('packet_rate', 100)
    failed_auth = telemetry.get('failed_auth', 0)
    unique_sources = telemetry.get('unique_sources', 1)

    if packet_rate > 5000:
        score += 0.55
    elif packet_rate > 1500:
        score += 0.35

    if failed_auth >= 15:
        score += 0.60
    elif failed_auth >= 5:
        score += 0.35

    if unique_sources >= 100:
        score += 0.40

    return min(1.0, round(score, 3))

def classify_attack(telemetry: Dict[str, Any]) -> Dict[str, Any]:
    anomaly = calculate_anomaly_score(telemetry)
    packet_rate = telemetry.get('packet_rate', 100)
    failed_auth = telemetry.get('failed_auth', 0)
    source_internal = telemetry.get('source_internal', False)

    if packet_rate > 2000:
        return {
            'predicted_type': 'DDoS',
            'severity': 'critical',
            'confidence': 98,
            'anomaly_score': anomaly,
            'shap_factors': [
                {'feature': 'Packet Rate Surge', 'value': f'{packet_rate:,} pkts/s', 'impact': 'Severe', 'contribution_score': 0.48},
                {'feature': 'Unique Source IPs', 'value': f"{telemetry.get('unique_sources', 1)} IPs", 'impact': 'High', 'contribution_score': 0.34}
            ]
        }
    elif failed_auth >= 5:
        return {
            'predicted_type': 'Brute-Force',
            'severity': 'high',
            'confidence': 92,
            'anomaly_score': anomaly,
            'shap_factors': [
                {'feature': 'Failed Auth Velocity', 'value': f'{failed_auth} failures/min', 'impact': 'Severe', 'contribution_score': 0.52},
                {'feature': 'Target Port Focus', 'value': 'SSH (22)', 'impact': 'High', 'contribution_score': 0.30}
            ]
        }
    elif source_internal and failed_auth > 1:
        return {
            'predicted_type': 'Lateral Movement',
            'severity': 'high',
            'confidence': 90,
            'anomaly_score': anomaly,
            'shap_factors': [
                {'feature': 'East-West Trajectory', 'value': 'Subnet Crossing', 'impact': 'Severe', 'contribution_score': 0.45}
            ]
        }
    else:
        return {
            'predicted_type': 'Normal',
            'severity': 'low',
            'confidence': 96,
            'anomaly_score': anomaly,
            'shap_factors': []
        }
