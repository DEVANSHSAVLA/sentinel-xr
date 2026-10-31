# SENTINEL-XR — Relational & In-Memory Database Schema

The database represents persistent cybersecurity operations data structured for relational deployment (PostgreSQL) and in-memory real-time retrieval (`src/server/db.ts`).

## Table Schemas

### 1. `devices` (or `network_nodes`)
```sql
CREATE TABLE devices (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    type VARCHAR(32) NOT NULL, -- gateway, firewall, router, server, database, workstation, client
    ip VARCHAR(45) NOT NULL,
    status VARCHAR(32) DEFAULT 'healthy', -- healthy, suspicious, critical, isolated
    risk_score INT DEFAULT 15,
    zone VARCHAR(32) NOT NULL, -- external, perimeter, dmz, internal, secure_core
    pos_x FLOAT NOT NULL,
    pos_y FLOAT NOT NULL,
    pos_z FLOAT NOT NULL,
    cpu_usage INT DEFAULT 20,
    memory_usage INT DEFAULT 40,
    traffic_rate INT DEFAULT 100,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2. `network_connections`
```sql
CREATE TABLE network_connections (
    id VARCHAR(64) PRIMARY KEY,
    source_id VARCHAR(64) REFERENCES devices(id),
    target_id VARCHAR(64) REFERENCES devices(id),
    protocol VARCHAR(16) NOT NULL, -- TCP, UDP, HTTPS, SSH, etc.
    port INT NOT NULL,
    status VARCHAR(32) DEFAULT 'normal', -- normal, suspicious, under_attack
    bandwidth_kbps INT DEFAULT 1000
);
```

### 3. `security_events`
```sql
CREATE TABLE security_events (
    id VARCHAR(64) PRIMARY KEY,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    source_ip VARCHAR(45) NOT NULL,
    dest_ip VARCHAR(45) NOT NULL,
    dest_port INT NOT NULL,
    protocol VARCHAR(16) NOT NULL,
    packet_rate INT NOT NULL,
    bytes_per_sec BIGINT NOT NULL,
    unique_sources INT NOT NULL,
    failed_auth_attempts INT DEFAULT 0,
    anomaly_score FLOAT NOT NULL,
    predicted_attack_type VARCHAR(64) NOT NULL,
    severity VARCHAR(16) NOT NULL,
    target_device_id VARCHAR(64) REFERENCES devices(id),
    raw_payload_summary TEXT
);
CREATE INDEX idx_events_timestamp ON security_events(timestamp DESC);
CREATE INDEX idx_events_target ON security_events(target_device_id);
```

### 4. `incidents`
```sql
CREATE TABLE incidents (
    id VARCHAR(64) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    severity VARCHAR(16) NOT NULL,
    status VARCHAR(32) NOT NULL, -- detected, investigating, assessing_risk, response_recommended, pending_approval, contained, verifying, resolved
    scenario VARCHAR(32) NOT NULL,
    target_device_id VARCHAR(64) REFERENCES devices(id),
    source_ip VARCHAR(45) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    description TEXT,
    confidence_score INT NOT NULL,
    recommended_action TEXT,
    containment_approved BOOLEAN DEFAULT FALSE,
    contained_at TIMESTAMP WITH TIME ZONE,
    resolved_at TIMESTAMP WITH TIME ZONE,
    forensic_notes TEXT
);
```

### 5. `incident_timeline`
```sql
CREATE TABLE incident_timeline (
    id SERIAL PRIMARY KEY,
    incident_id VARCHAR(64) REFERENCES incidents(id) ON DELETE CASCADE,
    time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    stage VARCHAR(32) NOT NULL,
    description TEXT NOT NULL,
    actor VARCHAR(64) NOT NULL
);
```
