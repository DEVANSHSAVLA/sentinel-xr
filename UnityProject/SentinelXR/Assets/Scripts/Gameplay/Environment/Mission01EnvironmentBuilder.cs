using UnityEngine;

namespace SentinelXR.Gameplay.Environment
{
    /// <summary>
    /// Procedurally constructs and manages the high-fidelity OBSIDIAN NEURAL cyber defense facility
    /// for Mission 01: Signal in the Noise.
    /// Builds industrial architecture, cyber gateway, central network core, elevated observation deck,
    /// defensive cover barricades, server banks, glowing data conduits, and holographic status panels.
    /// </summary>
    [ExecuteAlways]
    public class Mission01EnvironmentBuilder : MonoBehaviour
    {
        private const string FACILITY_ROOT_NAME = "CyberDefenseFacility";

        // OBSIDIAN NEURAL Palette
        public static readonly Color ColorDeepNavy = new Color(0.027f, 0.043f, 0.086f, 1f);     // #070B16
        public static readonly Color ColorMidnightBlue = new Color(0.063f, 0.102f, 0.200f, 1f); // #101A33
        public static readonly Color ColorElectricBlue = new Color(0.278f, 0.482f, 1.000f, 1f); // #477BFF
        public static readonly Color ColorCyan = new Color(0.000f, 0.851f, 1.000f, 1f);         // #00D9FF
        public static readonly Color ColorViolet = new Color(0.537f, 0.361f, 1.000f, 1f);       // #895CFF
        public static readonly Color ColorMagenta = new Color(0.831f, 0.361f, 1.000f, 1f);      // #D45CFF
        public static readonly Color ColorCriticalRed = new Color(1.000f, 0.212f, 0.373f, 1f);  // #FF365F
        public static readonly Color ColorSuccessGreen = new Color(0.145f, 0.902f, 0.647f, 1f); // #25E6A5

        // Cached Materials
        private static Material matWallDark;
        private static Material matServerRack;
        private static Material matTrimCyan;
        private static Material matTrimElectricBlue;
        private static Material matTrimViolet;
        private static Material matHazardRed;
        private static Material matHologram;

        private void Awake()
        {
            BuildFacilityIfMissing();
        }

        private void Start()
        {
            BuildFacilityIfMissing();
        }

        public static void BuildFacilityIfMissing()
        {
            var existing = GameObject.Find(FACILITY_ROOT_NAME);
            if (existing != null)
            {
                return;
            }

            Debug.Log("[Mission01EnvironmentBuilder] Initializing OBSIDIAN NEURAL Cyber-Defense Facility...");
            InitMaterials();

            GameObject root = new GameObject(FACILITY_ROOT_NAME);
            root.transform.position = Vector3.zero;

            BuildIndustrialPerimeterWalls(root.transform);
            BuildCyberGatewayComplex(root.transform);
            BuildCentralNetworkCore(root.transform);
            BuildObservationDeck(root.transform);
            BuildDefensiveCoverBarricades(root.transform);
            BuildServerRackBanks(root.transform);
            BuildBossContainmentArena(root.transform);
            BuildNetworkConduitLines(root.transform);
            BuildAtmosphericLighting(root.transform);

            Debug.Log("[Mission01EnvironmentBuilder] Cyber-Defense Facility generation COMPLETE.");
        }

        private static void InitMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Shader holoShader = Shader.Find("SentinelXR/ObsidianHologram");
            if (holoShader == null) holoShader = Shader.Find("Sprites/Default");

            matWallDark = CreateLitMat(litShader, ColorDeepNavy, Color.black, 0.8f, 0.6f);
            matServerRack = CreateLitMat(litShader, ColorMidnightBlue, Color.black, 0.9f, 0.7f);
            matTrimCyan = CreateLitMat(litShader, ColorCyan, ColorCyan * 2.2f, 0.5f, 0.8f);
            matTrimElectricBlue = CreateLitMat(litShader, ColorElectricBlue, ColorElectricBlue * 2.0f, 0.5f, 0.8f);
            matTrimViolet = CreateLitMat(litShader, ColorViolet, ColorViolet * 2.0f, 0.5f, 0.8f);
            matHazardRed = CreateLitMat(litShader, ColorCriticalRed, ColorCriticalRed * 2.5f, 0.5f, 0.8f);

            matHologram = new Material(holoShader);
            matHologram.name = "Hologram_Panel_Mat";
            if (matHologram.HasProperty("_BaseColor")) matHologram.SetColor("_BaseColor", new Color(0f, 0.85f, 1f, 0.75f));
            if (matHologram.HasProperty("_RimColor")) matHologram.SetColor("_RimColor", ColorElectricBlue);
        }

        private static Material CreateLitMat(Shader shader, Color baseCol, Color emissionCol, float metallic, float smoothness)
        {
            Material mat = new Material(shader);
            mat.color = baseCol;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseCol);
            if (emissionCol != Color.black)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionCol);
            }
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private static void BuildIndustrialPerimeterWalls(Transform parent)
        {
            GameObject wallsRoot = new GameObject("PerimeterWalls");
            wallsRoot.transform.SetParent(parent, false);

            float arenaHalf = 22f;
            float wallH = 7.0f;
            float wallThick = 1.0f;

            // 4 main walls
            CreateWallSegment(wallsRoot.transform, "Wall_North", new Vector3(0, wallH * 0.5f, arenaHalf), new Vector3(arenaHalf * 2, wallH, wallThick));
            CreateWallSegment(wallsRoot.transform, "Wall_South", new Vector3(0, wallH * 0.5f, -arenaHalf), new Vector3(arenaHalf * 2, wallH, wallThick));
            CreateWallSegment(wallsRoot.transform, "Wall_East", new Vector3(arenaHalf, wallH * 0.5f, 0), new Vector3(wallThick, wallH, arenaHalf * 2));
            CreateWallSegment(wallsRoot.transform, "Wall_West", new Vector3(-arenaHalf, wallH * 0.5f, 0), new Vector3(wallThick, wallH, arenaHalf * 2));

            // Glowing neon trim runners along top of walls
            CreateTrim(wallsRoot.transform, "Trim_North", new Vector3(0, wallH, arenaHalf - 0.1f), new Vector3(arenaHalf * 2, 0.25f, 0.3f), matTrimCyan);
            CreateTrim(wallsRoot.transform, "Trim_South", new Vector3(0, wallH, -arenaHalf + 0.1f), new Vector3(arenaHalf * 2, 0.25f, 0.3f), matTrimCyan);
            CreateTrim(wallsRoot.transform, "Trim_East", new Vector3(arenaHalf - 0.1f, wallH, 0), new Vector3(0.3f, 0.25f, arenaHalf * 2), matTrimElectricBlue);
            CreateTrim(wallsRoot.transform, "Trim_West", new Vector3(-arenaHalf + 0.1f, wallH, 0), new Vector3(0.3f, 0.25f, arenaHalf * 2), matTrimElectricBlue);

            // Structural buttress pillars
            for (float x = -16f; x <= 16f; x += 8f)
            {
                CreateButtress(wallsRoot.transform, new Vector3(x, wallH * 0.5f, arenaHalf - 0.4f), new Vector3(1.2f, wallH + 0.5f, 1.2f));
                CreateButtress(wallsRoot.transform, new Vector3(x, wallH * 0.5f, -arenaHalf + 0.4f), new Vector3(1.2f, wallH + 0.5f, 1.2f));
            }
            for (float z = -16f; z <= 16f; z += 8f)
            {
                CreateButtress(wallsRoot.transform, new Vector3(arenaHalf - 0.4f, wallH * 0.5f, z), new Vector3(1.2f, wallH + 0.5f, 1.2f));
                CreateButtress(wallsRoot.transform, new Vector3(-arenaHalf + 0.4f, wallH * 0.5f, z), new Vector3(1.2f, wallH + 0.5f, 1.2f));
            }
        }

        private static void CreateWallSegment(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = matWallDark;
        }

        private static void CreateTrim(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = name;
            trim.transform.SetParent(parent, false);
            trim.transform.position = pos;
            trim.transform.localScale = scale;
            var col = trim.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            trim.GetComponent<Renderer>().material = mat;
        }

        private static void CreateButtress(Transform parent, Vector3 pos, Vector3 scale)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.name = "PillarButtress";
            p.transform.SetParent(parent, false);
            p.transform.position = pos;
            p.transform.localScale = scale;
            p.GetComponent<Renderer>().material = matServerRack;

            // Vertical conduit strip on face
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "ConduitStrip";
            strip.transform.SetParent(p.transform, false);
            strip.transform.localPosition = new Vector3(0, 0, 0.55f);
            strip.transform.localScale = new Vector3(0.18f, 0.95f, 0.1f);
            var col = strip.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            strip.GetComponent<Renderer>().material = matTrimElectricBlue;
        }

        private static void BuildCyberGatewayComplex(Transform parent)
        {
            GameObject gwRoot = new GameObject("GatewayComplex");
            gwRoot.transform.SetParent(parent, false);
            Vector3 gwCenter = new Vector3(-4f, 0f, 0f);

            // Raised octagonal pedestal platform
            GameObject basePlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlat.name = "GatewayPlinth";
            basePlat.transform.SetParent(gwRoot.transform, false);
            basePlat.transform.position = gwCenter + new Vector3(0, 0.15f, 0);
            basePlat.transform.localScale = new Vector3(7.5f, 0.15f, 7.5f);
            basePlat.GetComponent<Renderer>().material = matServerRack;

            // Platform step trim ring
            GameObject plinthRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plinthRing.name = "PlinthNeonEdge";
            plinthRing.transform.SetParent(basePlat.transform, false);
            plinthRing.transform.localPosition = new Vector3(0, 0.05f, 0);
            plinthRing.transform.localScale = new Vector3(1.02f, 0.08f, 1.02f);
            var ringCol = plinthRing.GetComponent<Collider>();
            if (ringCol != null) DestroyImmediate(ringCol);
            plinthRing.GetComponent<Renderer>().material = matTrimCyan;

            // Left Cyber Gateway Tower
            CreateTower(gwRoot.transform, "GateTower_Left", gwCenter + new Vector3(0, 3.2f, -2.6f), new Vector3(0.9f, 6.4f, 0.9f), matTrimCyan);

            // Right Cyber Gateway Tower
            CreateTower(gwRoot.transform, "GateTower_Right", gwCenter + new Vector3(0, 3.2f, +2.6f), new Vector3(0.9f, 6.4f, 0.9f), matTrimCyan);

            // Transverse Overhead Cyber Arch
            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "GatewayArchSpan";
            arch.transform.SetParent(gwRoot.transform, false);
            arch.transform.position = gwCenter + new Vector3(0, 6.2f, 0);
            arch.transform.localScale = new Vector3(1.1f, 0.7f, 5.8f);
            arch.GetComponent<Renderer>().material = matServerRack;

            // Glowing conduit inside arch
            CreateTrim(arch.transform, "ArchConduitCore", new Vector3(0, -0.3f, 0), new Vector3(0.4f, 0.2f, 0.95f), matTrimCyan);

            // Interactive Diagnostics Console Desk
            GameObject consoleDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consoleDesk.name = "GatewayConsoleDesk";
            consoleDesk.transform.SetParent(gwRoot.transform, false);
            consoleDesk.transform.position = gwCenter + new Vector3(0, 0.8f, -1.8f);
            consoleDesk.transform.localScale = new Vector3(1.4f, 0.8f, 0.6f);
            consoleDesk.GetComponent<Renderer>().material = matServerRack;

            // Floating Diagnostic Hologram Panel
            GameObject holoPanel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            holoPanel.name = "HoloDisplay_GatewayStatus";
            holoPanel.transform.SetParent(gwRoot.transform, false);
            holoPanel.transform.position = gwCenter + new Vector3(0, 3.8f, 0);
            holoPanel.transform.localScale = new Vector3(2.8f, 1.6f, 1f);
            holoPanel.transform.rotation = Quaternion.Euler(0, 90f, 0);
            var hCol = holoPanel.GetComponent<Collider>();
            if (hCol != null) DestroyImmediate(hCol);
            holoPanel.GetComponent<Renderer>().material = matHologram;
        }

        private static void CreateTower(Transform parent, string name, Vector3 pos, Vector3 scale, Material trimMat)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = name;
            tower.transform.SetParent(parent, false);
            tower.transform.position = pos;
            tower.transform.localScale = scale;
            tower.GetComponent<Renderer>().material = matServerRack;

            // Emissive energy column on inward face
            GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            emitter.name = "EnergyCore";
            emitter.transform.SetParent(tower.transform, false);
            emitter.transform.localPosition = new Vector3(0, 0, 0);
            emitter.transform.localScale = new Vector3(0.35f, 0.98f, 0.35f);
            var col = emitter.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            emitter.GetComponent<Renderer>().material = trimMat;
        }

        private static void BuildCentralNetworkCore(Transform parent)
        {
            GameObject coreRoot = new GameObject("CentralNetworkCore");
            coreRoot.transform.SetParent(parent, false);
            Vector3 corePos = new Vector3(0, 0, -1f);

            // Core base pedestal
            GameObject coreBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreBase.name = "CorePlinth";
            coreBase.transform.SetParent(coreRoot.transform, false);
            coreBase.transform.position = corePos + new Vector3(0, 0.2f, 0);
            coreBase.transform.localScale = new Vector3(6f, 0.2f, 6f);
            coreBase.GetComponent<Renderer>().material = matServerRack;

            // Glowing central data column
            GameObject coreColumn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreColumn.name = "CentralDataColumn";
            coreColumn.transform.SetParent(coreRoot.transform, false);
            coreColumn.transform.position = corePos + new Vector3(0, 2.5f, 0);
            coreColumn.transform.localScale = new Vector3(1.4f, 2.3f, 1.4f);
            coreColumn.GetComponent<Renderer>().material = matTrimElectricBlue;

            // 4 energy containment pillars surrounding core
            Vector3[] offsets = {
                new Vector3(-2.2f, 1.8f, -2.2f),
                new Vector3(+2.2f, 1.8f, -2.2f),
                new Vector3(-2.2f, 1.8f, +2.2f),
                new Vector3(+2.2f, 1.8f, +2.2f)
            };

            foreach (var off in offsets)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "CorePylon";
                pillar.transform.SetParent(coreRoot.transform, false);
                pillar.transform.position = corePos + off;
                pillar.transform.localScale = new Vector3(0.5f, 3.6f, 0.5f);
                pillar.GetComponent<Renderer>().material = matServerRack;

                GameObject pylonConduit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pylonConduit.name = "PylonConduit";
                pylonConduit.transform.SetParent(pillar.transform, false);
                pylonConduit.transform.localPosition = Vector3.zero;
                pylonConduit.transform.localScale = new Vector3(0.2f, 0.98f, 0.2f);
                var c = pylonConduit.GetComponent<Collider>();
                if (c != null) DestroyImmediate(c);
                pylonConduit.GetComponent<Renderer>().material = matTrimCyan;
            }

            // Floating Central Topology Hologram
            GameObject holo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            holo.name = "HoloDisplay_SubnetTopology";
            holo.transform.SetParent(coreRoot.transform, false);
            holo.transform.position = corePos + new Vector3(0, 4.2f, 0);
            holo.transform.localScale = new Vector3(3f, 1.8f, 1f);
            holo.transform.rotation = Quaternion.Euler(0, 0, 0);
            var hc = holo.GetComponent<Collider>();
            if (hc != null) DestroyImmediate(hc);
            holo.GetComponent<Renderer>().material = matHologram;
        }

        private static void BuildObservationDeck(Transform parent)
        {
            GameObject deckRoot = new GameObject("ObservationDeck");
            deckRoot.transform.SetParent(parent, false);
            Vector3 deckPos = new Vector3(0, 2.8f, -17f);

            // Elevated Platform Floor
            GameObject deckFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckFloor.name = "DeckFloor";
            deckFloor.transform.SetParent(deckRoot.transform, false);
            deckFloor.transform.position = deckPos;
            deckFloor.transform.localScale = new Vector3(20f, 0.6f, 8f);
            deckFloor.GetComponent<Renderer>().material = matServerRack;

            // Edge Trim
            CreateTrim(deckFloor.transform, "DeckTrim", new Vector3(0, 0.35f, 0), new Vector3(1.01f, 0.15f, 1.01f), matTrimCyan);

            // Left Access Ramp
            GameObject rampL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampL.name = "DeckRamp_Left";
            rampL.transform.SetParent(deckRoot.transform, false);
            rampL.transform.position = new Vector3(-8f, 1.4f, -11.5f);
            rampL.transform.localScale = new Vector3(2.5f, 0.4f, 7.0f);
            rampL.transform.rotation = Quaternion.Euler(22f, 0, 0);
            rampL.GetComponent<Renderer>().material = matWallDark;

            // Right Access Ramp
            GameObject rampR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampR.name = "DeckRamp_Right";
            rampR.transform.SetParent(deckRoot.transform, false);
            rampR.transform.position = new Vector3(+8f, 1.4f, -11.5f);
            rampR.transform.localScale = new Vector3(2.5f, 0.4f, 7.0f);
            rampR.transform.rotation = Quaternion.Euler(22f, 0, 0);
            rampR.GetComponent<Renderer>().material = matWallDark;

            // Safety Railings along front
            GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railing.name = "FrontRailing";
            railing.transform.SetParent(deckRoot.transform, false);
            railing.transform.position = deckPos + new Vector3(0, 0.8f, 3.8f);
            railing.transform.localScale = new Vector3(19.6f, 1.0f, 0.2f);
            railing.GetComponent<Renderer>().material = matServerRack;

            // Glowing handrail top strip
            CreateTrim(railing.transform, "HandrailStrip", new Vector3(0, 0.55f, 0), new Vector3(1f, 0.15f, 1.2f), matTrimElectricBlue);

            // Command Terminal on Deck
            GameObject deckConsole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckConsole.name = "DeckCommandConsole";
            deckConsole.transform.SetParent(deckRoot.transform, false);
            deckConsole.transform.position = deckPos + new Vector3(0, 0.9f, 2.5f);
            deckConsole.transform.localScale = new Vector3(2.2f, 1.0f, 0.7f);
            deckConsole.GetComponent<Renderer>().material = matServerRack;
        }

        private static void BuildDefensiveCoverBarricades(Transform parent)
        {
            GameObject barricadesRoot = new GameObject("CoverBarricades");
            barricadesRoot.transform.SetParent(parent, false);

            Vector3[] positions = {
                new Vector3(-2.8f, 0.65f, -5.5f),
                new Vector3(+2.8f, 0.65f, -5.5f),
                new Vector3(-7.5f, 0.65f, +3.5f),
                new Vector3(+5.5f, 0.65f, +4.5f),
                new Vector3(0f, 0.65f, +6.0f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = $"Barricade_{i + 1}";
                bar.transform.SetParent(barricadesRoot.transform, false);
                bar.transform.position = positions[i];
                bar.transform.localScale = new Vector3(3.4f, 1.3f, 0.5f);
                bar.GetComponent<Renderer>().material = matServerRack;

                // Emissive cyber hazard trim on top
                GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trim.name = "BarricadeTrim";
                trim.transform.SetParent(bar.transform, false);
                trim.transform.localPosition = new Vector3(0, 0.52f, 0);
                trim.transform.localScale = new Vector3(0.98f, 0.12f, 1.05f);
                var col = trim.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                trim.GetComponent<Renderer>().material = matTrimCyan;
            }
        }

        private static void BuildServerRackBanks(Transform parent)
        {
            GameObject serversRoot = new GameObject("ServerRackBanks");
            serversRoot.transform.SetParent(parent, false);

            // Along West Wall (-21m)
            float[] zSpans = { -14f, -7f, +7f, +14f };
            foreach (var z in zSpans)
            {
                CreateServerBank(serversRoot.transform, new Vector3(-21.2f, 1.8f, z), new Vector3(0.8f, 3.6f, 4.5f), true);
            }

            // Along East Wall (+21m)
            foreach (var z in zSpans)
            {
                CreateServerBank(serversRoot.transform, new Vector3(+21.2f, 1.8f, z), new Vector3(0.8f, 3.6f, 4.5f), false);
            }
        }

        private static void CreateServerBank(Transform parent, Vector3 pos, Vector3 scale, bool isWest)
        {
            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "ServerBank";
            rack.transform.SetParent(parent, false);
            rack.transform.position = pos;
            rack.transform.localScale = scale;
            rack.GetComponent<Renderer>().material = matServerRack;

            // Blinking activity LED strips
            float inwardX = isWest ? 0.52f : -0.52f;
            for (float y = -0.3f; y <= 0.3f; y += 0.2f)
            {
                GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cube);
                led.name = "LedActivityStrip";
                led.transform.SetParent(rack.transform, false);
                led.transform.localPosition = new Vector3(inwardX, y, 0);
                led.transform.localScale = new Vector3(0.08f, 0.08f, 0.9f);
                var col = led.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                led.GetComponent<Renderer>().material = ((y < 0) ? matTrimCyan : matTrimViolet);
            }
        }

        private static void BuildBossContainmentArena(Transform parent)
        {
            GameObject bossRoot = new GameObject("BossContainmentSector");
            bossRoot.transform.SetParent(parent, false);
            Vector3 bossPos = new Vector3(0, 0, 12f);

            // Boss containment plinth
            GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plinth.name = "BossPlinth";
            plinth.transform.SetParent(bossRoot.transform, false);
            plinth.transform.position = bossPos + new Vector3(0, 0.15f, 0);
            plinth.transform.localScale = new Vector3(10f, 0.15f, 10f);
            plinth.GetComponent<Renderer>().material = matServerRack;

            // Critical red perimeter warning ring
            GameObject redRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            redRing.name = "ContainmentHazardRing";
            redRing.transform.SetParent(plinth.transform, false);
            redRing.transform.localPosition = new Vector3(0, 0.04f, 0);
            redRing.transform.localScale = new Vector3(1.02f, 0.1f, 1.02f);
            var col = redRing.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            redRing.GetComponent<Renderer>().material = matHazardRed;

            // 4 Containment Emitter Pylons
            Vector3[] pylonOffsets = {
                new Vector3(-4.5f, 2.5f, -4.0f),
                new Vector3(+4.5f, 2.5f, -4.0f),
                new Vector3(-4.5f, 2.5f, +4.0f),
                new Vector3(+4.5f, 2.5f, +4.0f)
            };

            foreach (var off in pylonOffsets)
            {
                GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pylon.name = "ContainmentPylon";
                pylon.transform.SetParent(bossRoot.transform, false);
                pylon.transform.position = bossPos + off;
                pylon.transform.localScale = new Vector3(0.7f, 5.0f, 0.7f);
                pylon.GetComponent<Renderer>().material = matServerRack;

                GameObject pConduit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pConduit.name = "PylonHazardCore";
                pConduit.transform.SetParent(pylon.transform, false);
                pConduit.transform.localPosition = Vector3.zero;
                pConduit.transform.localScale = new Vector3(0.28f, 0.98f, 0.28f);
                var c = pConduit.GetComponent<Collider>();
                if (c != null) DestroyImmediate(c);
                pConduit.GetComponent<Renderer>().material = matHazardRed;
            }

            // Floating Threat Warning Hologram Display
            GameObject holo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            holo.name = "HoloDisplay_BreachCoreAlert";
            holo.transform.SetParent(bossRoot.transform, false);
            holo.transform.position = bossPos + new Vector3(0, 5.8f, 0);
            holo.transform.localScale = new Vector3(3.5f, 2.0f, 1f);
            holo.transform.rotation = Quaternion.Euler(0, 180f, 0);
            var hc = holo.GetComponent<Collider>();
            if (hc != null) DestroyImmediate(hc);
            holo.GetComponent<Renderer>().material = matHologram;
        }

        private static void BuildNetworkConduitLines(Transform parent)
        {
            GameObject linesRoot = new GameObject("NetworkConduitLines");
            linesRoot.transform.SetParent(parent, false);

            CreateConduit(linesRoot.transform, "Conduit_GatewayToCore", new Vector3(-4f, 0.04f, 0), new Vector3(0, 0.04f, -1f), matTrimCyan);
            CreateConduit(linesRoot.transform, "Conduit_CoreToBoss", new Vector3(0, 0.04f, -1f), new Vector3(0, 0.04f, 12f), matHazardRed);
            CreateConduit(linesRoot.transform, "Conduit_CoreToDeck", new Vector3(0, 0.04f, -1f), new Vector3(0, 0.04f, -16f), matTrimElectricBlue);
            CreateConduit(linesRoot.transform, "Conduit_GatewayToServers", new Vector3(-4f, 0.04f, 0), new Vector3(-21f, 0.04f, 0), matTrimCyan);
        }

        private static void CreateConduit(Transform parent, string name, Vector3 start, Vector3 end, Material mat)
        {
            GameObject conduit = new GameObject(name);
            conduit.transform.SetParent(parent, false);
            var lr = conduit.AddComponent<LineRenderer>();
            lr.startWidth = 0.12f;
            lr.endWidth = 0.12f;
            lr.material = mat;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
        }

        private static void BuildAtmosphericLighting(Transform parent)
        {
            GameObject lightsRoot = new GameObject("AtmosphericLights");
            lightsRoot.transform.SetParent(parent, false);

            // Gateway accent light
            CreatePointLight(lightsRoot.transform, "Light_Gateway", new Vector3(-4f, 3.5f, 0), ColorCyan, 9f, 2.2f);

            // Central core accent light
            CreatePointLight(lightsRoot.transform, "Light_Core", new Vector3(0, 3.5f, -1f), ColorElectricBlue, 11f, 2.5f);

            // Boss containment accent light
            CreatePointLight(lightsRoot.transform, "Light_Boss", new Vector3(0, 4.0f, 12f), ColorCriticalRed, 13f, 3.0f);

            // Observation deck accent light
            CreatePointLight(lightsRoot.transform, "Light_Deck", new Vector3(0, 4.5f, -16f), ColorViolet, 12f, 2.0f);
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.position = pos;
            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None; // Optimized performance
        }
    }
}
