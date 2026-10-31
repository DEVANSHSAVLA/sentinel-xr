import React, { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { NetworkNode, NetworkConnection } from '../types';
import { ShieldAlert, Activity, RefreshCw, ZoomIn, ZoomOut, Eye, Layers } from 'lucide-react';

interface Props {
  nodes: NetworkNode[];
  connections: NetworkConnection[];
  selectedNodeId: string | null;
  onSelectNode: (node: NetworkNode | null) => void;
  onIsolateNode: (nodeId: string, isolate: boolean) => void;
  isSimulating: boolean;
}

export const NetworkTopology3D: React.FC<Props> = ({
  nodes,
  connections,
  selectedNodeId,
  onSelectNode,
  onIsolateNode,
  isSimulating,
}) => {
  const mountRef = useRef<HTMLDivElement>(null);
  const [hoveredNode, setHoveredNode] = useState<NetworkNode | null>(null);
  const [isDockCollapsed, setIsDockCollapsed] = useState(false);
  const [cameraZoomLevel, setCameraZoomLevel] = useState(1);
  const [showWireframe, setShowWireframe] = useState(false);

  // References to keep state in animation loop without tearing down Three.js canvas
  const nodesRef = useRef(nodes);
  nodesRef.current = nodes;
  const connectionsRef = useRef(connections);
  connectionsRef.current = connections;
  const selectedNodeIdRef = useRef(selectedNodeId);
  selectedNodeIdRef.current = selectedNodeId;
  const onSelectNodeRef = useRef(onSelectNode);
  onSelectNodeRef.current = onSelectNode;
  const isSimulatingRef = useRef(isSimulating);
  isSimulatingRef.current = isSimulating;

  const nodeMeshesMap = useRef<Map<string, THREE.Group>>(new Map());
  const controlsRef = useRef<OrbitControls | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);

  useEffect(() => {
    const container = mountRef.current;
    if (!container) return;

    // Detect software WebGL
    const isSoftwareGl = (() => {
      try {
        const gl = document.createElement('canvas').getContext('webgl2');
        if (!gl) return true;
        const ext = gl.getExtension('WEBGL_debug_renderer_info');
        const param = ext ? ext.UNMASKED_RENDERER_WEBGL : gl.RENDERER;
        const gpu = String(gl.getParameter(param));
        gl.getExtension('WEBGL_lose_context')?.loseContext();
        return /SwiftShader|llvmpipe|softpipe|Software|Basic Render/i.test(gpu);
      } catch {
        return true;
      }
    })();

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x070b16);
    scene.fog = new THREE.FogExp2(0x070b16, 0.035);

    const width = container.clientWidth || 800;
    const height = container.clientHeight || 500;
    const camera = new THREE.PerspectiveCamera(50, width / height, 0.1, 1000);
    camera.position.set(0, 7, 14);
    cameraRef.current = camera;

    const renderer = new THREE.WebGLRenderer({
      antialias: !isSoftwareGl,
      powerPreference: 'high-performance',
      alpha: true,
    });
    renderer.setSize(width, height);
    renderer.setPixelRatio(isSoftwareGl ? 1 : Math.min(window.devicePixelRatio, 2));
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.2;
    container.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = !isSoftwareGl;
    controls.dampingFactor = 0.05;
    controls.maxPolarAngle = Math.PI / 2.05;
    controls.minDistance = 4;
    controls.maxDistance = 25;
    controlsRef.current = controls;

    // Lighting setup
    const ambientLight = new THREE.AmbientLight(0x182642, 1.5);
    scene.add(ambientLight);

    const dirLight = new THREE.DirectionalLight(0x477bff, 2.2);
    dirLight.position.set(10, 15, 10);
    scene.add(dirLight);

    const cyanPoint = new THREE.PointLight(0x00d9ff, 3, 20);
    cyanPoint.position.set(0, 5, 0);
    scene.add(cyanPoint);

    // Floor holographic radar grid
    const gridHelper = new THREE.GridHelper(30, 30, 0x00d9ff, 0x112244);
    gridHelper.position.y = -3;
    scene.add(gridHelper);

    // Coordinate rings
    const ringGeo = new THREE.RingGeometry(8, 8.05, 64);
    const ringMat = new THREE.MeshBasicMaterial({ color: 0x477bff, side: THREE.DoubleSide, transparent: true, opacity: 0.3 });
    const ringMesh = new THREE.Mesh(ringGeo, ringMat);
    ringMesh.rotation.x = Math.PI / 2;
    ringMesh.position.y = -2.99;
    scene.add(ringMesh);

    // Procedural Node Meshes creation
    const nodeGroup = new THREE.Group();
    scene.add(nodeGroup);

    // Lines Group
    const connectionLinesGroup = new THREE.Group();
    scene.add(connectionLinesGroup);

    // Animated particles along edges
    const particleCount = 120;
    const particleGeometry = new THREE.BufferGeometry();
    const particlePositions = new Float32Array(particleCount * 3);
    const particleColors = new Float32Array(particleCount * 3);
    for (let i = 0; i < particleCount * 3; i++) {
      particlePositions[i] = 0;
      particleColors[i] = 0.2;
    }
    particleGeometry.setAttribute('position', new THREE.BufferAttribute(particlePositions, 3));
    particleGeometry.setAttribute('color', new THREE.BufferAttribute(particleColors, 3));
    const particleMaterial = new THREE.PointsMaterial({
      size: 0.16,
      vertexColors: true,
      transparent: true,
      opacity: 0.9,
      blending: THREE.AdditiveBlending,
    });
    const packetParticles = new THREE.Points(particleGeometry, particleMaterial);
    scene.add(packetParticles);

    // Raycasting for node hover and clicks
    const raycaster = new THREE.Raycaster();
    const mouse = new THREE.Vector2();

    const onPointerMove = (e: MouseEvent) => {
      const rect = renderer.domElement.getBoundingClientRect();
      mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
      mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

      raycaster.setFromCamera(mouse, camera);
      const intersects = raycaster.intersectObjects(nodeGroup.children, true);
      if (intersects.length > 0) {
        let parent = intersects[0].object.parent;
        while (parent && !(parent as any).userData?.nodeId && parent !== nodeGroup) {
          parent = parent.parent;
        }
        if (parent && (parent as any).userData?.nodeId) {
          const found = nodesRef.current.find((n) => n.id === (parent as any).userData.nodeId);
          setHoveredNode(found || null);
          return;
        }
      }
      setHoveredNode(null);
    };

    const onClick = (e: MouseEvent) => {
      const rect = renderer.domElement.getBoundingClientRect();
      mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
      mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

      raycaster.setFromCamera(mouse, camera);
      const intersects = raycaster.intersectObjects(nodeGroup.children, true);
      if (intersects.length > 0) {
        let parent = intersects[0].object.parent;
        while (parent && !(parent as any).userData?.nodeId && parent !== nodeGroup) {
          parent = parent.parent;
        }
        if (parent && (parent as any).userData?.nodeId) {
          const found = nodesRef.current.find((n) => n.id === (parent as any).userData.nodeId);
          if (found) {
            onSelectNodeRef.current(found);
            return;
          }
        }
      }
    };

    renderer.domElement.addEventListener('pointermove', onPointerMove);
    renderer.domElement.addEventListener('click', onClick);

    // Resize handler
    const handleResize = () => {
      if (!container) return;
      const w = container.clientWidth;
      const h = container.clientHeight;
      camera.aspect = w / h;
      camera.updateProjectionMatrix();
      renderer.setSize(w, h);
    };
    window.addEventListener('resize', handleResize);

    // Animation loop
    let animId = 0;
    let clock = new THREE.Clock();

    const animate = () => {
      animId = requestAnimationFrame(animate);
      const delta = clock.getDelta();
      const elapsed = clock.getElapsedTime();

      controls.update();

      // Rotate scene slowly when idle
      if (!hoveredNode) {
        ringMesh.rotation.z += delta * 0.08;
      }

      // Re-synchronize node meshes
      const currentNodes = nodesRef.current;
      nodeMeshesMap.current.forEach((mesh, id) => {
        if (!currentNodes.some((n) => n.id === id)) {
          nodeGroup.remove(mesh);
          nodeMeshesMap.current.delete(id);
        }
      });

      currentNodes.forEach((node) => {
        let meshGroup = nodeMeshesMap.current.get(node.id);
        if (!meshGroup) {
          meshGroup = new THREE.Group();
          (meshGroup as any).userData = { nodeId: node.id };

          // Node procedural geometry (composite 3-layer representation)
          // 1. Base pedestal
          const baseGeo = new THREE.CylinderGeometry(0.5, 0.6, 0.15, 16);
          const baseMat = new THREE.MeshStandardMaterial({
            color: 0x111a2d,
            metalness: 0.8,
            roughness: 0.3,
          });
          const baseMesh = new THREE.Mesh(baseGeo, baseMat);
          baseMesh.position.y = -0.4;
          meshGroup.add(baseMesh);

          // 2. Core geometry based on node type
          let coreGeo: THREE.BufferGeometry;
          if (node.type === 'gateway' || node.type === 'router') {
            coreGeo = new THREE.OctahedronGeometry(0.45);
          } else if (node.type === 'database') {
            coreGeo = new THREE.CylinderGeometry(0.35, 0.35, 0.7, 16);
          } else if (node.type === 'firewall') {
            coreGeo = new THREE.BoxGeometry(0.65, 0.65, 0.65);
          } else {
            coreGeo = new THREE.IcosahedronGeometry(0.42, 1);
          }

          const coreMat = new THREE.MeshStandardMaterial({
            color: node.status === 'critical' ? 0xff456a :
                   node.status === 'suspicious' ? 0xffb547 :
                   node.status === 'isolated' ? 0x5a6e8c : 0x00d9ff,
            emissive: node.status === 'critical' ? 0x77001a :
                      node.status === 'suspicious' ? 0x553300 :
                      node.status === 'isolated' ? 0x111122 : 0x003366,
            emissiveIntensity: 0.8,
            metalness: 0.6,
            roughness: 0.2,
          });
          const coreMesh = new THREE.Mesh(coreGeo, coreMat);
          (coreMesh as any).name = 'core';
          meshGroup.add(coreMesh);

          // 3. Orbital energy ring
          const orbRingGeo = new THREE.TorusGeometry(0.65, 0.02, 8, 32);
          const orbRingMat = new THREE.MeshBasicMaterial({
            color: 0x00d9ff,
            transparent: true,
            opacity: 0.6,
          });
          const orbRing = new THREE.Mesh(orbRingGeo, orbRingMat);
          orbRing.rotation.x = Math.PI / 2;
          (orbRing as any).name = 'ring';
          meshGroup.add(orbRing);

          meshGroup.position.set(...node.position);
          nodeGroup.add(meshGroup);
          nodeMeshesMap.current.set(node.id, meshGroup);
        }

        // Live visual updates on meshes
        meshGroup.position.set(...node.position);
        const coreMesh = meshGroup.getObjectByName('core') as THREE.Mesh;
        const orbRing = meshGroup.getObjectByName('ring') as THREE.Mesh;

        if (coreMesh && coreMesh.material) {
          const mat = coreMesh.material as THREE.MeshStandardMaterial;
          const isSelected = selectedNodeIdRef.current === node.id;
          
          if (node.status === 'isolated') {
            mat.color.setHex(0x5a6e8c);
            mat.emissive.setHex(0x111a2d);
            mat.opacity = 0.45;
            mat.transparent = true;
          } else if (node.status === 'critical') {
            mat.color.setHex(0xff456a);
            mat.emissive.setHex(0x990022);
            mat.emissiveIntensity = 0.9 + Math.sin(elapsed * 6) * 0.4;
          } else if (node.status === 'suspicious') {
            mat.color.setHex(0xffb547);
            mat.emissive.setHex(0x664400);
            mat.emissiveIntensity = 0.7 + Math.sin(elapsed * 4) * 0.2;
          } else {
            mat.color.setHex(isSelected ? 0x895cff : 0x00d9ff);
            mat.emissive.setHex(isSelected ? 0x441188 : 0x003366);
          }

          if (orbRing) {
            orbRing.rotation.z += delta * (node.status === 'critical' ? 4 : 1.2);
            orbRing.scale.setScalar(node.status === 'isolated' ? 0.7 : 1 + (isSelected ? 0.25 : 0));
          }
        }
      });

      // Update connection lines
      connectionLinesGroup.clear();
      const nodePosMap = new Map<string, THREE.Vector3>();
      currentNodes.forEach((n) => nodePosMap.set(n.id, new THREE.Vector3(...n.position)));

      connectionsRef.current.forEach((conn) => {
        const p1 = nodePosMap.get(conn.source);
        const p2 = nodePosMap.get(conn.target);
        if (!p1 || !p2) return;

        const isAttacked = conn.status === 'under_attack' || (isSimulatingRef.current && (conn.target === 'server-01' || conn.target === 'server-02'));
        const lineColor = isAttacked ? 0xff456a : conn.status === 'suspicious' ? 0xffb547 : 0x477bff;

        const points = [p1, p2];
        const lineGeo = new THREE.BufferGeometry().setFromPoints(points);
        const lineMat = new THREE.LineBasicMaterial({
          color: lineColor,
          transparent: true,
          opacity: isAttacked ? 0.85 : 0.35,
          linewidth: isAttacked ? 2 : 1,
        });
        const line = new THREE.Line(lineGeo, lineMat);
        connectionLinesGroup.add(line);
      });

      // Update animated packet particles
      const posAttr = particleGeometry.attributes.position as THREE.BufferAttribute;
      const colAttr = particleGeometry.attributes.color as THREE.BufferAttribute;
      const activeConns = connectionsRef.current;

      if (activeConns.length > 0) {
        for (let i = 0; i < particleCount; i++) {
          const connIndex = i % activeConns.length;
          const conn = activeConns[connIndex];
          const p1 = nodePosMap.get(conn.source);
          const p2 = nodePosMap.get(conn.target);

          if (p1 && p2) {
            const speed = conn.status === 'under_attack' ? 1.8 : 0.6;
            const t = ((elapsed * speed + (i / particleCount)) % 1);
            const px = p1.x + (p2.x - p1.x) * t;
            const py = p1.y + (p2.y - p1.y) * t + Math.sin(t * Math.PI) * 0.25;
            const pz = p1.z + (p2.z - p1.z) * t;

            posAttr.setXYZ(i, px, py, pz);

            if (conn.status === 'under_attack' || (isSimulatingRef.current && conn.target === 'server-01')) {
              colAttr.setXYZ(i, 1.0, 0.27, 0.42); // Neon red attack packet
            } else {
              colAttr.setXYZ(i, 0.0, 0.85, 1.0); // Cyan healthy packet
            }
          }
        }
        posAttr.needsUpdate = true;
        colAttr.needsUpdate = true;
      }

      renderer.render(scene, camera);
    };

    animate();

    return () => {
      cancelAnimationFrame(animId);
      window.removeEventListener('resize', handleResize);
      renderer.domElement.removeEventListener('pointermove', onPointerMove);
      renderer.domElement.removeEventListener('click', onClick);
      controls.dispose();
      renderer.dispose();
      if (container.contains(renderer.domElement)) {
        container.removeChild(renderer.domElement);
      }
    };
  }, []);

  const handleResetCamera = () => {
    if (cameraRef.current && controlsRef.current) {
      cameraRef.current.position.set(0, 7, 14);
      controlsRef.current.target.set(0, 0, 0);
      controlsRef.current.update();
      setCameraZoomLevel(1);
    }
  };

  const handleZoom = (direction: 'in' | 'out') => {
    if (cameraRef.current && controlsRef.current) {
      const zoomFactor = direction === 'in' ? 0.85 : 1.15;
      cameraRef.current.position.multiplyScalar(zoomFactor);
      controlsRef.current.update();
      setCameraZoomLevel((prev) => (direction === 'in' ? prev + 0.15 : Math.max(0.4, prev - 0.15)));
    }
  };

  const selectedNode = nodes.find((n) => n.id === selectedNodeId);

  return (
    <div className="relative w-full h-[540px] rounded-xl overflow-hidden border border-blue-500/20 bg-[#070b16]">
      {/* Three.js Canvas Container */}
      <div ref={mountRef} className="w-full h-full cursor-grab active:cursor-grabbing" />

      {/* Top Left Spec / Title Badge */}
      <div className="absolute top-4 left-4 z-10 flex items-center gap-3 glass-panel px-3.5 py-2">
        <Activity className="w-4 h-4 text-cyan-400" />
        <div>
          <h4 className="text-xs font-semibold text-slate-100 uppercase tracking-wider font-heading">
            Live 3D Network Topology
          </h4>
          <span className="text-[11px] text-slate-400 font-mono">
            {nodes.length} Nodes · {connections.length} Edges · Real-time WebGL
          </span>
        </div>
      </div>

      {/* Hover Info Card */}
      {hoveredNode && !selectedNode && (
        <div className="absolute top-4 right-4 z-10 glass-panel px-3.5 py-2.5 max-w-xs animate-in fade-in">
          <div className="flex items-center justify-between gap-3 mb-1">
            <span className="text-xs font-bold text-slate-200">{hoveredNode.name}</span>
            <span
              className={`text-[10px] uppercase font-mono px-1.5 py-0.5 rounded ${
                hoveredNode.status === 'critical'
                  ? 'bg-rose-500/20 text-rose-400 border border-rose-500/30'
                  : hoveredNode.status === 'suspicious'
                  ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30'
                  : hoveredNode.status === 'isolated'
                  ? 'bg-slate-700/40 text-slate-400 border border-slate-600/30'
                  : 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/30'
              }`}
            >
              {hoveredNode.status}
            </span>
          </div>
          <div className="text-[11px] text-slate-400 font-mono">
            IP: {hoveredNode.ip} · Risk: {hoveredNode.riskScore}/100
          </div>
          <div className="text-[10px] text-slate-500 mt-1">Click node to inspect and contain</div>
        </div>
      )}

      {/* Selected Node Detailed Inspector Floating Drawer */}
      {selectedNode && (
        <div className="absolute top-4 right-4 z-20 glass-panel-elevated p-4 max-w-sm border border-cyan-500/30 shadow-2xl animate-in slide-in-from-right-4">
          <div className="flex items-start justify-between gap-3 mb-2">
            <div>
              <h3 className="text-sm font-bold text-slate-100 font-heading">{selectedNode.name}</h3>
              <p className="text-xs text-slate-400 font-mono">{selectedNode.ip} · {selectedNode.zone.toUpperCase()}</p>
            </div>
            <button
              onClick={() => onSelectNode(null)}
              className="text-slate-400 hover:text-slate-200 text-xs px-2 py-1 rounded bg-slate-800"
            >
              ✕
            </button>
          </div>

          <div className="grid grid-cols-2 gap-2 my-3 text-xs font-mono">
            <div className="bg-slate-900/60 p-2 rounded border border-slate-800">
              <span className="text-slate-500 block text-[10px]">RISK SCORE</span>
              <span className={`text-base font-bold ${selectedNode.riskScore > 60 ? 'text-rose-400' : 'text-cyan-400'}`}>
                {selectedNode.riskScore}/100
              </span>
            </div>
            <div className="bg-slate-900/60 p-2 rounded border border-slate-800">
              <span className="text-slate-500 block text-[10px]">TRAFFIC LOAD</span>
              <span className="text-base font-bold text-slate-200">
                {selectedNode.trafficRate} <span className="text-xs font-normal">pkts/s</span>
              </span>
            </div>
            <div className="bg-slate-900/60 p-2 rounded border border-slate-800">
              <span className="text-slate-500 block text-[10px]">CPU USAGE</span>
              <span className="text-slate-200 font-semibold">{selectedNode.cpuUsage}%</span>
            </div>
            <div className="bg-slate-900/60 p-2 rounded border border-slate-800">
              <span className="text-slate-500 block text-[10px]">MEMORY</span>
              <span className="text-slate-200 font-semibold">{selectedNode.memoryUsage}%</span>
            </div>
          </div>

          <div className="mb-3">
            <span className="text-[10px] text-slate-500 uppercase font-mono block mb-1">Services</span>
            <div className="flex flex-wrap gap-1">
              {selectedNode.services.map((s, idx) => (
                <span key={idx} className="text-[10px] bg-blue-950/50 text-blue-300 px-2 py-0.5 rounded border border-blue-500/20 font-mono">
                  {s}
                </span>
              ))}
            </div>
          </div>

          <div className="flex items-center gap-2 pt-2 border-t border-slate-800">
            {selectedNode.status === 'isolated' ? (
              <button
                onClick={() => onIsolateNode(selectedNode.id, false)}
                className="w-full py-2 px-3 text-xs font-semibold text-emerald-300 bg-emerald-950/60 hover:bg-emerald-900/60 border border-emerald-500/40 rounded-lg transition-colors flex items-center justify-center gap-1.5"
              >
                <RefreshCw className="w-3.5 h-3.5" /> Reconnect to Network
              </button>
            ) : (
              <button
                onClick={() => onIsolateNode(selectedNode.id, true)}
                className="w-full py-2 px-3 text-xs font-semibold text-rose-300 bg-rose-950/60 hover:bg-rose-900/60 border border-rose-500/40 rounded-lg transition-colors flex items-center justify-center gap-1.5"
              >
                <ShieldAlert className="w-3.5 h-3.5" /> Quarantine / Isolate Node
              </button>
            )}
          </div>
        </div>
      )}

      {/* Unified Bottom Control Dock (Zero Viewport Occlusion, Collapsible) */}
      <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-20 flex flex-col items-center">
        {/* Toggle Collapse Pill */}
        <button
          onClick={() => setIsDockCollapsed(!isDockCollapsed)}
          className="mb-1.5 px-3 py-1 text-[11px] font-mono text-slate-400 hover:text-slate-100 glass-panel rounded-full transition-colors flex items-center gap-1.5 shadow-lg"
          aria-label={isDockCollapsed ? 'Show 3D Viewport Controls' : 'Hide 3D Viewport Controls'}
        >
          {isDockCollapsed ? '▴ Expand Controls' : '▾ Collapse HUD'}
        </button>

        {!isDockCollapsed && (
          <div className="glass-panel px-4 py-2 flex items-center gap-4 text-xs shadow-2xl border border-blue-500/20">
            <div className="flex items-center gap-1 text-slate-300 font-mono">
              <span className="text-[10px] text-slate-500 uppercase mr-1">Camera:</span>
              <button
                onClick={() => handleZoom('in')}
                className="p-1.5 rounded hover:bg-slate-800 text-slate-300 transition-colors"
                title="Zoom in"
                aria-label="Zoom in"
              >
                <ZoomIn className="w-3.5 h-3.5" />
              </button>
              <button
                onClick={() => handleZoom('out')}
                className="p-1.5 rounded hover:bg-slate-800 text-slate-300 transition-colors"
                title="Zoom out"
                aria-label="Zoom out"
              >
                <ZoomOut className="w-3.5 h-3.5" />
              </button>
              <button
                onClick={handleResetCamera}
                className="p-1.5 rounded hover:bg-slate-800 text-slate-300 transition-colors ml-1"
                title="Reset View"
                aria-label="Reset View"
              >
                <RefreshCw className="w-3.5 h-3.5" />
              </button>
            </div>

            <div className="h-4 w-px bg-slate-800" />

            {/* Simulated Traffic State Indicator */}
            <div className="flex items-center gap-2">
              <span className="text-[10px] text-slate-500 font-mono uppercase">Telemetry Flow:</span>
              <span
                className={`text-[11px] font-mono font-medium px-2 py-0.5 rounded ${
                  isSimulating
                    ? 'text-rose-400 bg-rose-950/50 border border-rose-500/30'
                    : 'text-cyan-400 bg-cyan-950/50 border border-cyan-500/30'
                }`}
              >
                {isSimulating ? 'Simulated Attack Surge' : 'Nominal Synchronized'}
              </span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
