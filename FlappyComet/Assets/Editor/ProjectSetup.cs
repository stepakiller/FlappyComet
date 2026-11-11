using System.IO;
using System.Linq;
using FlappyComet.Core;
using FlappyComet.Gameplay;
using FlappyComet.Infrastructure;
using FlappyComet.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FlappyComet.EditorTools
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";
        const string ArtDir = "Assets/Art";
        const string PrefabDir = "Assets/Prefabs";
        const string SettingsDir = "Assets/Settings";
        const string ConfigPath = SettingsDir + "/GameConfig.asset";
        const string CometPrefabPath = PrefabDir + "/Comet.prefab";

        static readonly Color BackgroundColor = Color.black;
        static readonly Color BarColor = new Color(0.153f, 0.275f, 0.102f);
        static readonly Color CometColor = new Color(0.25f, 0.72f, 0.2f);
        static readonly Color ObstacleColor = new Color(0.55f, 0.27f, 0.27f);
        static readonly Color PickupColor = new Color(1f, 0.85f, 0.2f);
        const float CometRadius = 0.35f;

        static ProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(CometPrefabPath) || !SceneHasLifetimeScope()) Generate();
            };
        }

        static bool SceneHasLifetimeScope()
        {
            if (!File.Exists(ScenePath)) return false;
            string guid = AssetDatabase.FindAssets("GameLifetimeScope t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileName(path) == "GameLifetimeScope.cs")
                .Select(AssetDatabase.AssetPathToGUID)
                .FirstOrDefault();
            return guid == null || File.ReadAllText(ScenePath).Contains(guid);
        }

        [MenuItem("Flappy Comet/Rebuild Prefabs and Scene")]
        static void RebuildFromMenu()
        {
            if (File.Exists(CometPrefabPath) &&
                !EditorUtility.DisplayDialog("Rebuild", "Overwrite prefabs and scene?", "Yes", "No"))
                return;
            Generate();
        }

        public static void EnsureGenerated()
        {
            if (!File.Exists(CometPrefabPath) || !SceneHasLifetimeScope()) Generate();
            AddSceneToBuildSettings();
            ApplyPlayerSettings();
        }

        public static void Generate()
        {
            foreach (string dir in new[] { ArtDir, PrefabDir, SettingsDir, Path.GetDirectoryName(ScenePath) })
                Directory.CreateDirectory(dir);

            Sprite circle = WriteSprite(SpriteFactory.Circle, ArtDir + "/Circle.png", 128, FilterMode.Bilinear);
            Sprite square = WriteSprite(SpriteFactory.Square, ArtDir + "/Square.png", 4, FilterMode.Point);
            GameConfig config = LoadOrCreateConfig();
            var spriteMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

            GameObject cometPrefab = CreateCometPrefab(circle, spriteMaterial);
            GameObject obstaclePrefab = CreateObstaclePrefab(square);
            GameObject pickupPrefab = CreatePickupPrefab(square);
            GameObject hudPrefab = CreateHudPrefab(config);

            CreateScene(config, square, cometPrefab, obstaclePrefab, pickupPrefab, hudPrefab);
            AddSceneToBuildSettings();
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
        }

        static Sprite WriteSprite(Sprite source, string path, int pixelsPerUnit, FilterMode filter)
        {
            File.WriteAllBytes(path, source.texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filter;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static GameConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        static GameObject SavePrefab(GameObject go, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateCometPrefab(Sprite circle, Material spriteMaterial)
        {
            var root = new GameObject("Comet");

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            root.AddComponent<CircleCollider2D>().radius = CometRadius * 0.9f;
            root.AddComponent<CometController>();

            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = spriteMaterial;
            line.numCapVertices = 4;
            line.sortingOrder = 4;
            line.positionCount = 0;
            float diameter = CometRadius * 2f;
            line.widthCurve = new AnimationCurve(new Keyframe(0f, diameter * 0.15f), new Keyframe(1f, diameter * 0.8f));
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(CometColor * 0.6f, 0f), new GradientColorKey(CometColor, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(1f, 1f) });
            line.colorGradient = gradient;
            root.AddComponent<CometTrail>();

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = Vector3.one * diameter;
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.color = CometColor;
            sr.sortingOrder = 5;

            return SavePrefab(root, CometPrefabPath);
        }

        static GameObject CreateObstaclePrefab(Sprite square)
        {
            var go = new GameObject("Obstacle");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = ObstacleColor;
            sr.sortingOrder = 3;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<Obstacle>();
            return SavePrefab(go, PrefabDir + "/Obstacle.prefab");
        }

        static GameObject CreatePickupPrefab(Sprite square)
        {
            var go = new GameObject("Pickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = PickupColor;
            sr.sortingOrder = 6;
            go.AddComponent<Pickup>();
            return SavePrefab(go, PrefabDir + "/Pickup.prefab");
        }

        static GameObject CreateHudPrefab(GameConfig config)
        {
            var root = new GameObject("HUD", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            Font font = LoadFont();
            float barFraction = config.barHeight / (config.cameraSize * 2f);

            Text score = CreateText("Score", root.transform, font, 64, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            score.text = "0";
            SetAnchors(score.rectTransform, new Vector2(0.3f, 1f - barFraction), new Vector2(0.7f, 1f), Vector2.zero);

            Text prompt = CreateText("Prompt", root.transform, font, 40, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f));
            prompt.text = "Press SPACEBAR to start";
            SetAnchors(prompt.rectTransform, new Vector2(config.cometScreenX, 0.45f), new Vector2(1f, 0.55f), new Vector2(110f, 0f));

            var hud = root.AddComponent<HudView>();
            Wire(hud, "score", score);
            Wire(hud, "prompt", prompt);

            return SavePrefab(root, PrefabDir + "/HUD.prefab");
        }

        static void CreateScene(GameConfig config, Sprite square, GameObject cometPrefab, GameObject obstaclePrefab,
            GameObject pickupPrefab, GameObject hudPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = config.cameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BackgroundColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            var playField = new GameObject("PlayField").transform;
            float barY = config.cameraSize - config.barHeight * 0.5f;
            CreateBar("TopBar", playField, barY, config.barHeight, square);
            CreateBar("BottomBar", playField, -barY, config.barHeight, square);

            var comet = ((GameObject)PrefabUtility.InstantiatePrefab(cometPrefab)).GetComponent<CometController>();
            comet.transform.position = new Vector3(config.CometX(16f / 9f), 0f, 0f);

            var hud = ((GameObject)PrefabUtility.InstantiatePrefab(hudPrefab)).GetComponent<HudView>();

            var scope = new GameObject("GameLifetimeScope").AddComponent<GameLifetimeScope>();
            scope.transform.SetAsFirstSibling();
            Wire(scope, "config", config);
            Wire(scope, "obstaclePrefab", obstaclePrefab.GetComponent<Obstacle>());
            Wire(scope, "pickupPrefab", pickupPrefab.GetComponent<Pickup>());
            Wire(scope, "gameCamera", cam);
            Wire(scope, "comet", comet);
            Wire(scope, "cometTrail", comet.GetComponent<CometTrail>());
            Wire(scope, "hud", hud);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static void CreateBar(string name, Transform parent, float y, float height, Sprite square)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, y, 0f);
            go.transform.localScale = new Vector3(200f, height, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = BarColor;
            sr.sortingOrder = 10;
        }

        static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = anchor;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return t;
        }

        static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 offset)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offset;
            rt.offsetMax = offset;
        }

        static Font LoadFont()
        {
            Font f = null;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null)
            {
                try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            return f;
        }

        static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "FlappyCometClone";
            PlayerSettings.productName = "Flappy Comet";
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 600;
            PlayerSettings.runInBackground = true;

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
        }
    }
}
