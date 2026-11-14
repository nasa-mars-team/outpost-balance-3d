#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OutpostBalance.Core;
using OutpostBalance.Data;
using OutpostBalance.Gameplay;
using OutpostBalance.UI;
using OutpostBalance.World3D;

namespace OutpostBalance.Editor
{
    [InitializeOnLoad]
    public static class OutpostSceneBuilder
    {
        private static Font _font;
        private static Sprite _whiteSprite;
        private static Material _standardMat;

        static OutpostSceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists("Assets/Scenes/MainScene.unity"))
                {
                    BuildAndSaveScene();
                }
            };
        }

        [MenuItem("Outpost Balance/Build Complete 3D Scene & UI")]
        public static void BuildAndSaveScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _whiteSprite = EnsureWhiteSprite();
            _standardMat = new Material(Shader.Find("Standard"));

            // 1. Lighting & Fog
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.30f, 0.38f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.012f;

            var sunGo = new GameObject("SunLight");
            var sunLight = sunGo.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.35f;
            sunLight.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            // 2. Camera + PhysicsRaycaster
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = MissionConfig.Moon.skyColor;
            cam.farClipPlane = 200f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<PhysicsRaycaster>();
            var orbit = camGo.AddComponent<CameraOrbit>();

            // 3. 3D Outpost World
            var worldRoot = new GameObject("OutpostWorld3D");
            var visuals = worldRoot.AddComponent<OutpostVisuals>();
            visuals.mainCam = cam;
            visuals.sunLight = sunLight;
            orbit.target = worldRoot.transform;

            Build3DWorld(worldRoot.transform, visuals);

            // 4. Core Services
            var mgrGo = new GameObject("GameManager");
            var gm = mgrGo.AddComponent<GameManager>();
            var donki = mgrGo.AddComponent<NasaDonkiClient>();
            var comm = mgrGo.AddComponent<CommDelayMgr>();
            var sfx = mgrGo.AddComponent<SfxPlayer>();

            gm.donkiClient = donki;
            gm.commDelayMgr = comm;
            gm.outpostVisuals = visuals;
            gm.sfxPlayer = sfx;

            // 5. EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();

            // 6. Complete UI Canvas
            BuildUI(gm);

            // 7. Save Scene & Register in Build Settings
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");

            string scenePath = "Assets/Scenes/MainScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.Refresh();
            Debug.Log("[OutpostSceneBuilder] MainScene.unity built and saved successfully!");
        }

        private static Sprite EnsureWhiteSprite()
        {
            if (!Directory.Exists("Assets/Textures"))
                Directory.CreateDirectory("Assets/Textures");
            string path = "Assets/Textures/WhiteSquare.png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var cols = new Color[16];
                for (int i = 0; i < 16; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
            }
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Material MakeMat(Color color, bool transparent = false, bool emissive = false)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            if (transparent)
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.2f);
            }
            return mat;
        }

        private static GameObject MakePrim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (rot.HasValue) go.transform.localEulerAngles = rot.Value;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static void Build3DWorld(Transform root, OutpostVisuals visuals)
        {
            // Ground & Craters
            var ground = MakePrim(PrimitiveType.Cylinder, "Ground", root, new Vector3(0, -0.25f, 0), new Vector3(45f, 0.5f, 45f), MakeMat(MissionConfig.Moon.groundColor));
            visuals.groundRenderer = ground.GetComponent<Renderer>();

            var craterMat = MakeMat(MissionConfig.Moon.groundAccentColor);
            var c1 = MakePrim(PrimitiveType.Cylinder, "Crater1", root, new Vector3(-8f, 0.02f, 5f), new Vector3(4.5f, 0.08f, 4.5f), craterMat);
            var c2 = MakePrim(PrimitiveType.Cylinder, "Crater2", root, new Vector3(8.5f, 0.02f, -4.5f), new Vector3(3.8f, 0.08f, 3.8f), craterMat);
            var c3 = MakePrim(PrimitiveType.Cylinder, "Crater3", root, new Vector3(-5f, 0.02f, -7.5f), new Vector3(5.2f, 0.08f, 5.2f), craterMat);
            visuals.craterRenderers = new[] { c1.GetComponent<Renderer>(), c2.GetComponent<Renderer>(), c3.GetComponent<Renderer>() };

            // Blue Earth in Lunar Sky
            var earth = MakePrim(PrimitiveType.Sphere, "EarthInSky", root, new Vector3(14f, 12f, 22f), new Vector3(2.8f, 2.8f, 2.8f), MakeMat(new Color(0.18f, 0.55f, 0.96f), false, true));
            Object.DestroyImmediate(earth.GetComponent<Collider>());
            visuals.earthInSky = earth;

            // Main Habitat Dome
            var habMat = MakeMat(new Color(0.88f, 0.91f, 0.96f));
            var darkMat = MakeMat(new Color(0.22f, 0.27f, 0.35f));
            MakePrim(PrimitiveType.Sphere, "HabitatDome", root, new Vector3(0f, 0.3f, 0f), new Vector3(4.2f, 3.2f, 4.2f), habMat);
            MakePrim(PrimitiveType.Cylinder, "HabitatBase", root, new Vector3(0f, 0.25f, 0f), new Vector3(4.4f, 0.35f, 4.4f), darkMat);

            var winMat1 = MakeMat(new Color(1f, 0.88f, 0.35f), false, true);
            var winMat2 = MakeMat(new Color(1f, 0.88f, 0.35f), false, true);
            var w1 = MakePrim(PrimitiveType.Cube, "HabWindow1", root, new Vector3(0f, 1.1f, 2.0f), new Vector3(1.2f, 0.45f, 0.2f), winMat1);
            var w2 = MakePrim(PrimitiveType.Cube, "HabWindow2", root, new Vector3(2.0f, 1.1f, 0f), new Vector3(0.2f, 0.45f, 1.2f), winMat2);
            visuals.habWindowRenderers = new[] { w1.GetComponent<Renderer>(), w2.GetComponent<Renderer>() };

            // Connecting Tunnels
            MakePrim(PrimitiveType.Cube, "TunnelLeft", root, new Vector3(-2.6f, 0.45f, 0f), new Vector3(2.2f, 0.7f, 0.8f), darkMat);
            MakePrim(PrimitiveType.Cube, "TunnelRight", root, new Vector3(2.6f, 0.45f, 0f), new Vector3(2.2f, 0.7f, 0.8f), darkMat);

            // Module 1: O2 & Water Recycler Tower (Clickable!)
            var o2Root = new GameObject("Module_O2Recycler");
            o2Root.transform.SetParent(root, false);
            o2Root.transform.localPosition = new Vector3(-4.2f, 0f, 0f);
            var o2Body = MakePrim(PrimitiveType.Cylinder, "O2Tower", o2Root.transform, new Vector3(0f, 1.3f, 0f), new Vector3(1.6f, 1.3f, 1.6f), MakeMat(new Color(0.22f, 0.68f, 0.92f)));
            MakePrim(PrimitiveType.Sphere, "O2TankCap", o2Root.transform, new Vector3(0f, 2.6f, 0f), new Vector3(1.5f, 0.8f, 1.5f), habMat);
            o2Body.AddComponent<ModuleClickable>().subsystem = OutpostSubsystem.O2;

            // Module 2: Hydroponic Greenhouse Dome + 3D Plants (Clickable!)
            var ghRoot = new GameObject("Module_Greenhouse");
            ghRoot.transform.SetParent(root, false);
            ghRoot.transform.localPosition = new Vector3(4.2f, 0f, 0f);
            var ghDome = MakePrim(PrimitiveType.Sphere, "GreenhouseGlass", ghRoot.transform, new Vector3(0f, 0.2f, 0f), new Vector3(3.2f, 2.4f, 3.2f), MakeMat(new Color(0.55f, 0.96f, 0.72f, 0.35f), true));
            ghDome.AddComponent<ModuleClickable>().subsystem = OutpostSubsystem.Greenhouse;

            var plantList = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                float px = (i % 2 == 0 ? -0.55f : 0.55f);
                float pz = (i < 2 ? -0.55f : 0.55f);
                var plant = MakePrim(PrimitiveType.Capsule, $"SpacePlant_{i}", ghRoot.transform, new Vector3(px, 0.65f, pz), new Vector3(0.42f, 0.9f, 0.42f), MakeMat(new Color(0.16f, 0.78f, 0.32f)));
                Object.DestroyImmediate(plant.GetComponent<Collider>());
                plantList[i] = plant.GetComponent<Renderer>();
            }
            visuals.plantRenderers = plantList;

            // Module 3: Radiation Shield Tower & Translucent Energy Dome (Clickable!)
            var shRoot = new GameObject("Module_ShieldTower");
            shRoot.transform.SetParent(root, false);
            shRoot.transform.localPosition = new Vector3(0f, 0f, -4.0f);
            var shTower = MakePrim(PrimitiveType.Cylinder, "ShieldPylon", shRoot.transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.9f, 1.5f, 0.9f), MakeMat(new Color(0.25f, 0.52f, 0.95f)));
            MakePrim(PrimitiveType.Sphere, "ShieldEmitterOrb", shRoot.transform, new Vector3(0f, 3.2f, 0f), new Vector3(1.1f, 1.1f, 1.1f), MakeMat(new Color(0.32f, 0.85f, 1f), false, true));
            shTower.AddComponent<ModuleClickable>().subsystem = OutpostSubsystem.Shield;

            var bubble = MakePrim(PrimitiveType.Sphere, "RadiationShieldDome", root, new Vector3(0f, 0.2f, 0f), new Vector3(13.5f, 8.2f, 13.5f), MakeMat(new Color(0.22f, 0.82f, 1f, 0.22f), true));
            Object.DestroyImmediate(bubble.GetComponent<Collider>());
            visuals.shieldBubbleRenderer = bubble.GetComponent<Renderer>();

            // Module 4: Science & Radio Lab + Dish + Rover (Clickable!)
            var labRoot = new GameObject("Module_ScienceLab");
            labRoot.transform.SetParent(root, false);
            labRoot.transform.localPosition = new Vector3(0f, 0f, 4.0f);
            var labBox = MakePrim(PrimitiveType.Cube, "ScienceLabCube", labRoot.transform, new Vector3(0f, 0.9f, 0f), new Vector3(2.2f, 1.8f, 2.0f), MakeMat(new Color(0.65f, 0.42f, 0.92f)));
            labBox.AddComponent<ModuleClickable>().subsystem = OutpostSubsystem.Lab;

            var dishPivot = new GameObject("CommsDishPivot");
            dishPivot.transform.SetParent(labRoot.transform, false);
            dishPivot.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            MakePrim(PrimitiveType.Cylinder, "DishBowl", dishPivot.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1.4f, 0.15f, 1.4f), habMat, new Vector3(35f, 0f, 0f));
            visuals.commsDishTransform = dishPivot.transform;

            // Solar Arrays
            var panel1 = MakePrim(PrimitiveType.Cube, "SolarPanel_1", root, new Vector3(-6.8f, 1.1f, 2.8f), new Vector3(2.6f, 0.1f, 1.5f), MakeMat(new Color(0.12f, 0.34f, 0.72f)), new Vector3(24f, 0f, 0f));
            var panel2 = MakePrim(PrimitiveType.Cube, "SolarPanel_2", root, new Vector3(-6.8f, 1.1f, -2.8f), new Vector3(2.6f, 0.1f, 1.5f), MakeMat(new Color(0.12f, 0.34f, 0.72f)), new Vector3(24f, 0f, 0f));
            MakePrim(PrimitiveType.Cylinder, "SolarPole1", root, new Vector3(-6.8f, 0.55f, 2.8f), new Vector3(0.2f, 0.55f, 0.2f), darkMat);
            MakePrim(PrimitiveType.Cylinder, "SolarPole2", root, new Vector3(-6.8f, 0.55f, -2.8f), new Vector3(0.2f, 0.55f, 0.2f), darkMat);
            visuals.solarPanelRenderers = new[] { panel1.GetComponent<Renderer>(), panel2.GetComponent<Renderer>() };

            // 3D Science Rover
            var roverGo = new GameObject("ScienceRover3D");
            roverGo.transform.SetParent(root, false);
            roverGo.transform.localPosition = new Vector3(5f, 0.35f, -3f);
            MakePrim(PrimitiveType.Cube, "RoverBody", roverGo.transform, Vector3.zero, new Vector3(0.9f, 0.45f, 1.3f), MakeMat(new Color(0.95f, 0.65f, 0.12f)));
            MakePrim(PrimitiveType.Sphere, "RoverCameraMast", roverGo.transform, new Vector3(0f, 0.45f, 0.35f), new Vector3(0.28f, 0.28f, 0.28f), habMat);
            var patrol = roverGo.AddComponent<RoverPatrol>();
            visuals.roverPatrol = patrol;

            // Particle Systems (Solar Flare & Mars Dust Storm)
            visuals.solarFlareParticles = CreateParticleSystem("SolarFlareVFX", root, new Vector3(0f, 12f, 0f), new Color(1f, 0.45f, 0.1f, 0.85f), 45f, 0.35f);
            visuals.dustStormParticles = CreateParticleSystem("DustStormVFX", root, new Vector3(-12f, 2.5f, 0f), new Color(0.85f, 0.42f, 0.22f, 0.55f), 60f, 0.5f);
        }

        private static ParticleSystem CreateParticleSystem(string name, Transform parent, Vector3 pos, Color color, float rate, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.startColor = color;
            main.startSize = size;
            main.startLifetime = 2.6f;
            main.startSpeed = 4.5f;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 2f, 14f);
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = MakeMat(color, true, true);
            return ps;
        }

        private static void BuildUI(GameManager gm)
        {
            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Color panelBg = new Color(0.06f, 0.10f, 0.20f, 0.90f);
            Color cardBg = new Color(0.10f, 0.15f, 0.28f, 0.95f);
            Color blueBtn = new Color(0.11f, 0.48f, 0.88f, 1f);
            Color marsBtn = new Color(0.88f, 0.34f, 0.10f, 1f);

            // Top Bar
            var topBar = CreatePanel("TopBar", canvasGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(-24, 56), panelBg);
            gm.appTitleText = CreateText("AppTitle", topBar, "OUTPOST BALANCE 3D", 20, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0, 0.45f), new Vector2(0.45f, 1), new Vector2(16, 0), new Vector2(0, -4));
            gm.appSubText = CreateText("AppSub", topBar, "NASA Mission Trade-Off Simulator", 12, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.65f, 0.75f, 0.90f), new Vector2(0, 0), new Vector2(0.45f, 0.45f), new Vector2(16, 4), Vector2.zero);

            var btnData = CreateButton("BtnDataLab", topBar, "NASA Data & Glossary", cardBg, new Vector2(0.47f, 0.15f), new Vector2(0.64f, 0.85f), out gm.dataBtnText);
            UnityEventTools.AddPersistentListener(btnData.onClick, gm.OpenDataModal);

            var btnSound = CreateButton("BtnSound", topBar, "Sound: ON", cardBg, new Vector2(0.65f, 0.15f), new Vector2(0.76f, 0.85f), out gm.soundBtnText);
            UnityEventTools.AddPersistentListener(btnSound.onClick, gm.ToggleSound);

            var btnLang = CreateButton("BtnLang", topBar, "UZ / EN", cardBg, new Vector2(0.77f, 0.15f), new Vector2(0.86f, 0.85f), out _);
            UnityEventTools.AddPersistentListener(btnLang.onClick, gm.ToggleLanguage);

            var btnSwitch = CreateButton("BtnSwitch", topBar, "Switch Site", cardBg, new Vector2(0.87f, 0.15f), new Vector2(0.99f, 0.85f), out gm.switchBtnText);
            gm.switchSiteButton = btnSwitch;
            UnityEventTools.AddPersistentListener(btnSwitch.onClick, gm.ReturnToSiteSelect);

            // DONKI Status Banner
            var bannerGo = CreatePanel("DonkiBanner", canvasGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -68), new Vector2(-24, 30), new Color(0.14f, 0.20f, 0.34f, 0.88f));
            gm.donkiBannerText = CreateText("DonkiBannerTxt", bannerGo, "Checking NASA DONKI...", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.98f, 0.85f, 0.42f), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));

            // SCREEN 1: Site Select
            var selectScreen = CreatePanel("SiteSelectScreen", canvasGo.transform, new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.82f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, panelBg);
            gm.siteSelectScreen = selectScreen.gameObject;
            gm.chooseTitleText = CreateText("ChooseTitle", selectScreen, "Choose Your 3D Outpost Mission", 26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0, 0.82f), new Vector2(1, 0.96f), new Vector2(20, 0), new Vector2(-20, 0));
            gm.chooseDescText = CreateText("ChooseDesc", selectScreen, "", 15, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 0.96f), new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);

            var moonCard = CreatePanel("MoonCard", selectScreen, new Vector2(0.04f, 0.08f), new Vector2(0.48f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, cardBg);
            gm.moonCardSpecText = CreateText("MoonSpecs", moonCard, "", 14, FontStyle.Normal, TextAnchor.UpperLeft, Color.white, new Vector2(0.06f, 0.25f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            var btnMoon = CreateButton("BtnMoon", moonCard, "Launch Moon Base", blueBtn, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.22f), out gm.moonBtnText);
            UnityEventTools.AddPersistentListener(btnMoon.onClick, gm.StartMoonMission);

            var marsCard = CreatePanel("MarsCard", selectScreen, new Vector2(0.52f, 0.08f), new Vector2(0.96f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, cardBg);
            gm.marsCardSpecText = CreateText("MarsSpecs", marsCard, "", 14, FontStyle.Normal, TextAnchor.UpperLeft, Color.white, new Vector2(0.06f, 0.25f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            var btnMars = CreateButton("BtnMars", marsCard, "Launch Mars Base", marsBtn, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.22f), out gm.marsBtnText);
            UnityEventTools.AddPersistentListener(btnMars.onClick, gm.StartMarsMission);

            // SCREEN 2: Gameplay HUD + Power/Dilemma Panels
            var hudRoot = CreatePanel("GameplayHUDRoot", canvasGo.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
            gm.gameplayHudRoot = hudRoot.gameObject;
            var hudCtrl = hudRoot.gameObject.AddComponent<HudController>();
            gm.hud = hudCtrl;

            var hudTop = CreatePanel("HudGauges", hudRoot, new Vector2(0.01f, 0.72f), new Vector2(0.99f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, panelBg);
            hudCtrl.siteBadgeText = CreateText("SiteBadge", hudTop, "Moon", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.45f, 0.85f, 1f), new Vector2(0.01f, 0.65f), new Vector2(0.35f, 0.96f), Vector2.zero, Vector2.zero);
            hudCtrl.dayBadgeText = CreateText("DayBadge", hudTop, "Day 1 / 7", 14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.36f, 0.65f), new Vector2(0.55f, 0.96f), Vector2.zero, Vector2.zero);
            hudCtrl.commBadgeText = CreateText("CommBadge", hudTop, "Delay: 1.3s", 14, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.98f, 0.75f, 0.35f), new Vector2(0.56f, 0.65f), new Vector2(0.99f, 0.96f), Vector2.zero, Vector2.zero);

            CreateGauge(hudTop, "Power", new Vector2(0.01f, 0.08f), new Vector2(0.24f, 0.62f), out hudCtrl.powerLabelText, out hudCtrl.powerValText, out hudCtrl.powerFill);
            CreateGauge(hudTop, "Life", new Vector2(0.26f, 0.08f), new Vector2(0.49f, 0.62f), out hudCtrl.lifeLabelText, out hudCtrl.lifeValText, out hudCtrl.lifeFill);
            CreateGauge(hudTop, "Shield", new Vector2(0.51f, 0.08f), new Vector2(0.74f, 0.62f), out hudCtrl.shieldLabelText, out hudCtrl.shieldValText, out hudCtrl.shieldFill);
            CreateGauge(hudTop, "Science", new Vector2(0.76f, 0.08f), new Vector2(0.99f, 0.62f), out hudCtrl.scienceLabelText, out hudCtrl.scienceValText, out hudCtrl.scienceFill);

            // Ripple Box
            var rippleGo = CreatePanel("RippleBox", hudRoot, new Vector2(0.01f, 0.65f), new Vector2(0.99f, 0.71f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.45f, 0.28f, 0.05f, 0.92f));
            hudCtrl.rippleBox = rippleGo.gameObject;
            hudCtrl.rippleText = CreateText("RippleTxt", rippleGo, "", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.60f), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));

            // Bottom Power Budget Panel (leaves center 3D viewport open!)
            var powerGo = CreatePanel("PowerPanel", hudRoot, new Vector2(0.01f, 0.02f), new Vector2(0.99f, 0.34f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero, panelBg);
            var powerUI = powerGo.gameObject.AddComponent<PowerPanelUI>();
            gm.powerPanel = powerUI;

            powerUI.titleText = CreateText("PowerTitle", powerGo, "Power Budget", 17, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.02f, 0.80f), new Vector2(0.48f, 0.96f), Vector2.zero, Vector2.zero);
            powerUI.descText = CreateText("PowerDesc", powerGo, "", 12, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.75f, 0.82f, 0.92f), new Vector2(0.02f, 0.66f), new Vector2(0.48f, 0.80f), Vector2.zero, Vector2.zero);
            powerUI.summaryText = CreateText("PowerSummary", powerGo, "", 14, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.55f, 0.92f, 1f), new Vector2(0.48f, 0.74f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero);

            var warnGo = CreatePanel("WarnBanner", powerGo, new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.65f, 0.14f, 0.14f, 0.92f));
            powerUI.warningBanner = warnGo.gameObject;
            powerUI.warningText = CreateText("WarnTxt", warnGo, "", 12, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CreateSubsystemCard(powerGo, "O2", new Vector2(0.02f, 0.20f), new Vector2(0.25f, 0.50f), cardBg, out powerUI.o2NameText, out powerUI.o2ValText, gm.AdjustO2Minus, gm.AdjustO2Plus);
            CreateSubsystemCard(powerGo, "GH", new Vector2(0.26f, 0.20f), new Vector2(0.49f, 0.50f), cardBg, out powerUI.ghNameText, out powerUI.ghValText, gm.AdjustGhMinus, gm.AdjustGhPlus);
            CreateSubsystemCard(powerGo, "Shield", new Vector2(0.51f, 0.20f), new Vector2(0.74f, 0.50f), cardBg, out powerUI.shieldNameText, out powerUI.shieldValText, gm.AdjustShieldMinus, gm.AdjustShieldPlus);
            CreateSubsystemCard(powerGo, "Lab", new Vector2(0.75f, 0.20f), new Vector2(0.98f, 0.50f), cardBg, out powerUI.labNameText, out powerUI.labValText, gm.AdjustLabMinus, gm.AdjustLabPlus);

            var btnRun = CreateButton("BtnRunDay", powerGo, "Run Mission Day →", blueBtn, new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.17f), out powerUI.runButtonText);
            UnityEventTools.AddPersistentListener(btnRun.onClick, gm.RunMissionDay);

            // Dilemma Panel
            var dilGo = CreatePanel("DilemmaPanel", hudRoot, new Vector2(0.10f, 0.03f), new Vector2(0.90f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, panelBg);
            var dilUI = dilGo.gameObject.AddComponent<DilemmaPanelUI>();
            gm.dilemmaPanel = dilUI;

            dilUI.tagText = CreateText("DilTag", dilGo, "ALERT", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.25f), new Vector2(0.03f, 0.88f), new Vector2(0.45f, 0.97f), Vector2.zero, Vector2.zero);
            dilUI.sourceText = CreateText("DilSource", dilGo, "Source:", 12, FontStyle.Normal, TextAnchor.MiddleRight, new Color(0.65f, 0.78f, 0.90f), new Vector2(0.45f, 0.88f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero);
            dilUI.titleText = CreateText("DilTitle", dilGo, "Event", 20, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.03f, 0.76f), new Vector2(0.97f, 0.88f), Vector2.zero, Vector2.zero);
            dilUI.descText = CreateText("DilDesc", dilGo, "", 14, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.88f, 0.92f, 0.98f), new Vector2(0.03f, 0.58f), new Vector2(0.97f, 0.76f), Vector2.zero, Vector2.zero);

            var commBox = CreatePanel("CommDelayBox", dilGo, new Vector2(0.03f, 0.47f), new Vector2(0.97f, 0.57f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.55f, 0.24f, 0.08f, 0.92f));
            dilUI.commDelayBox = commBox.gameObject;
            dilUI.commDelayText = CreateText("CommTxt", commBox, "", 12, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero);
            var commTrack = CreatePanel("CommTrack", commBox, new Vector2(0.02f, 0.10f), new Vector2(0.98f, 0.30f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.15f, 0.15f));
            var commFillGo = CreatePanel("CommFill", commTrack, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero, new Color(1f, 0.72f, 0.18f));
            var commImg = commFillGo.GetComponent<Image>();
            commImg.type = Image.Type.Filled;
            commImg.fillMethod = Image.FillMethod.Horizontal;
            dilUI.commProgressFill = commImg;

            dilUI.choiceButtons = new Button[3];
            dilUI.choiceButtonTexts = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float yTop = 0.45f - i * 0.11f;
                dilUI.choiceButtons[i] = CreateButton($"ChoiceBtn_{i}", dilGo, "Choice", cardBg, new Vector2(0.03f, yTop - 0.095f), new Vector2(0.97f, yTop), out dilUI.choiceButtonTexts[i]);
            }

            var sciCallout = CreatePanel("ScienceCallout", dilGo, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.22f, 0.36f, 0.92f));
            dilUI.scienceCalloutText = CreateText("SciCalloutTxt", sciCallout, "", 12, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.82f, 0.94f, 1f), new Vector2(0.02f, 0), new Vector2(0.98f, 1), Vector2.zero, Vector2.zero);

            // SCREEN 3: Debrief
            var debGo = CreatePanel("DebriefScreen", canvasGo.transform, new Vector2(0.10f, 0.08f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, panelBg);
            var debUI = debGo.gameObject.AddComponent<DebriefUI>();
            gm.debriefUI = debUI;

            debUI.titleText = CreateText("DebTitle", debGo, "Debrief", 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);
            debUI.rankText = CreateText("DebRank", debGo, "", 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.55f, 0.90f, 1f), new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.84f), Vector2.zero, Vector2.zero);
            debUI.statsText = CreateText("DebStats", debGo, "", 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.72f), Vector2.zero, Vector2.zero);
            debUI.logText = CreateText("DebLog", debGo, "", 14, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.85f, 0.90f, 0.96f), new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.60f), Vector2.zero, Vector2.zero);

            var btnReplay = CreateButton("BtnPlayAgain", debGo, "Play Again", blueBtn, new Vector2(0.10f, 0.04f), new Vector2(0.46f, 0.14f), out debUI.playAgainBtnText);
            UnityEventTools.AddPersistentListener(btnReplay.onClick, gm.ReplaySameSite);
            var btnOther = CreateButton("BtnOtherPlanet", debGo, "Other Planet", marsBtn, new Vector2(0.54f, 0.04f), new Vector2(0.90f, 0.14f), out debUI.otherSiteBtnText);
            UnityEventTools.AddPersistentListener(btnOther.onClick, gm.PlayOtherPlanet);

            // MODAL: NASA Data & Glossary
            var modalGo = CreatePanel("DataGlossaryModal", canvasGo.transform, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.92f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.07f, 0.14f, 0.97f));
            var modalUI = modalGo.gameObject.AddComponent<DataGlossaryUI>();
            gm.dataModal = modalUI;

            modalUI.titleText = CreateText("ModalTitle", modalGo, "NASA Data & Glossary", 20, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.03f, 0.88f), new Vector2(0.68f, 0.97f), Vector2.zero, Vector2.zero);
            var btnCloseModal = CreateButton("BtnCloseModal", modalGo, "Close", blueBtn, new Vector2(0.70f, 0.88f), new Vector2(0.97f, 0.97f), out modalUI.closeBtnText);
            UnityEventTools.AddPersistentListener(btnCloseModal.onClick, gm.CloseDataModal);
            modalUI.bodyText = CreateText("ModalBody", modalGo, "", 14, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.88f, 0.92f, 0.98f), new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero);
            modalGo.gameObject.SetActive(false);
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var img = go.GetComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
            return rt;
        }

        private static Text CreateText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor align, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var t = go.GetComponent<Text>();
            t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color bg, Vector2 anchorMin, Vector2 anchorMax, out Text labelText)
        {
            var rt = CreatePanel(name, parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, bg);
            var btn = rt.gameObject.AddComponent<Button>();
            labelText = CreateText("Label", rt, label, 14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(6, 2), new Vector2(-6, -2));
            return btn;
        }

        private static void CreateGauge(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, out Text lbl, out Text val, out Image fill)
        {
            var card = CreatePanel($"Gauge_{name}", parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.11f, 0.16f, 0.30f));
            lbl = CreateText("Lbl", card, name, 13, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.05f, 0.48f), new Vector2(0.65f, 0.95f), Vector2.zero, Vector2.zero);
            val = CreateText("Val", card, "80%", 13, FontStyle.Bold, TextAnchor.MiddleRight, Color.white, new Vector2(0.65f, 0.48f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            var track = CreatePanel("Track", card, new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.12f));
            var fillRt = CreatePanel("Fill", track, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero, new Color(0.22f, 0.78f, 0.98f));
            fill = fillRt.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
        }

        private static void CreateSubsystemCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color bg, out Text nameTxt, out Text valTxt, UnityEngine.Events.UnityAction onMinus, UnityEngine.Events.UnityAction onPlus)
        {
            var card = CreatePanel($"Sys_{name}", parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, bg);
            nameTxt = CreateText("Title", card, name, 13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.04f, 0.55f), new Vector2(0.96f, 0.95f), Vector2.zero, Vector2.zero);
            var btnMinus = CreateButton("Minus", card, "−", new Color(0.20f, 0.28f, 0.45f), new Vector2(0.08f, 0.08f), new Vector2(0.32f, 0.50f), out _);
            UnityEventTools.AddPersistentListener(btnMinus.onClick, onMinus);
            valTxt = CreateText("Val", card, "2 / 4", 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.45f, 0.90f, 1f), new Vector2(0.34f, 0.08f), new Vector2(0.66f, 0.50f), Vector2.zero, Vector2.zero);
            var btnPlus = CreateButton("Plus", card, "+", new Color(0.20f, 0.28f, 0.45f), new Vector2(0.68f, 0.08f), new Vector2(0.92f, 0.50f), out _);
            UnityEventTools.AddPersistentListener(btnPlus.onClick, onPlus);
        }
    }
}
#endif
