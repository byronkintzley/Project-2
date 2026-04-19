#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project2.EditorTools
{
    /// <summary>
    /// One-click project scaffolder. Menu: Project2 -> Setup Barebones Project.
    ///
    /// Creates MainMenu + Level1/2/3 scenes with player, HUD, enemies, hazards,
    /// doors, goals, and all button wiring. Fully overwrites existing scenes
    /// with the same names, so confirm first.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("Project2/Setup Barebones Project")]
        public static void RunSetup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            bool ok = EditorUtility.DisplayDialog(
                "Project2 - Setup Barebones Project",
                "This will create/overwrite these scenes in Assets/Scenes:\n" +
                "  - MainMenu\n  - Level1\n  - Level2\n  - Level3\n\n" +
                "It will also configure Build Settings and add a 'Player' tag.\n\n" +
                "Continue?",
                "Yes, build it", "Cancel");
            if (!ok) return;

            AddTagIfMissing("Player");

            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            CreateMainMenuScene();
            CreateLevelScene("Level1", enemyCount: 1, withKeyDoor: false);
            CreateLevelScene("Level2", enemyCount: 2, withKeyDoor: true);
            CreateLevelScene("Level3", enemyCount: 3, withKeyDoor: true);

            SetupBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(ScenesFolder + "/MainMenu.unity");

            EditorUtility.DisplayDialog(
                "Project2 - Done",
                "All four scenes are built and wired. Press Play from MainMenu to test.\n\n" +
                "Controls: WASD move, mouse look, Space jump, Shift sprint, " +
                "Left-click shoot, Esc pause.",
                "OK");
        }

        // ---------------------------------------------------------------------
        // MAIN MENU
        // ---------------------------------------------------------------------

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera(new Vector3(0, 1, -10), dark: true);
            CreateEventSystem();

            GameObject canvasGO = CreateCanvas("MenuCanvas");

            CreateUIText(canvasGO.transform, "Title", "MY GAME",
                anchoredPos: new Vector2(0, -120), size: new Vector2(800, 140),
                fontSize: 80, anchor: new Vector2(0.5f, 1f));

            Button play = CreateUIButton(canvasGO.transform, "PlayButton", "Play",
                anchoredPos: new Vector2(0, 30));
            Button quit = CreateUIButton(canvasGO.transform, "QuitButton", "Quit",
                anchoredPos: new Vector2(0, -60));

            MainMenuController controller = canvasGO.AddComponent<MainMenuController>();

            // GameManager lives in the MainMenu scene; DontDestroyOnLoad carries it forward.
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();

            UnityEventTools.AddPersistentListener(play.onClick, controller.OnPlayPressed);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.OnQuitPressed);

            EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/MainMenu.unity");
        }

        // ---------------------------------------------------------------------
        // LEVEL
        // ---------------------------------------------------------------------

        private static void CreateLevelScene(string sceneName, int enemyCount, bool withKeyDoor)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5);

            // Jumping geometry
            CreatePlatforms();

            // Player
            GameObject player = CreatePlayer();

            // HUD (and grabs refs back from player)
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            CreateHUD(playerHealth);

            // Enemies
            for (int i = 0; i < enemyCount; i++)
            {
                float angle = (i + 1) * 2.3f;
                CreateEnemy(new Vector3(Mathf.Cos(angle) * 8, 1, Mathf.Sin(angle) * 8 + 4));
            }

            // Hazard
            CreateHazardZone(new Vector3(-5, 0.1f, 3));

            // Goal tracker - goal completes when all enemies are dead
            GameObject trackerGO = new GameObject("GoalTracker");
            GoalTracker tracker = trackerGO.AddComponent<GoalTracker>();
            SetGoalTrackerClearLevelGoal(tracker, "clearLevel");

            // Optional key + locked door between spawn and exit
            if (withKeyDoor)
            {
                CreatePickup(new Vector3(6, 1, -4), Pickup.PickupType.Key, "RedKey");
                CreateLockedDoor(new Vector3(5, 1.5f, 12), "RedKey");
            }

            // Exit portal - requires the "clearLevel" goal before it works
            CreateLevelGoal(new Vector3(8, 1.5f, 15), requireGoalId: "clearLevel");

            EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{sceneName}.unity");
        }

        private static void CreatePlatforms()
        {
            Vector3[] positions =
            {
                new Vector3(3, 1,   3),
                new Vector3(6, 2,   6),
                new Vector3(-3, 1.5f, 5),
                new Vector3(0, 2.5f, 9),
                new Vector3(4, 3.5f, 11),
            };
            foreach (var pos in positions)
            {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name = "Platform";
                p.transform.position = pos;
                p.transform.localScale = new Vector3(3, 0.5f, 3);
            }
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1.5f, -4);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.center = new Vector3(0, 1, 0);
            cc.height = 2f;
            cc.radius = 0.5f;

            // Camera as a child of the player
            GameObject camGO = new GameObject("PlayerCamera");
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(player.transform, false);
            camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
            camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();

            FirstPersonController fpc = player.AddComponent<FirstPersonController>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerInventory>();
            PlayerShoot shoot = player.AddComponent<PlayerShoot>();

            SetFieldReference(fpc, "cameraTransform", camGO.transform);
            SetFieldReference(shoot, "cameraTransform", camGO.transform);

            return player;
        }

        private static void CreateHUD(PlayerHealth playerHealth)
        {
            CreateEventSystem();
            GameObject canvasGO = CreateCanvas("HUD");

            // Crosshair
            CreateUIText(canvasGO.transform, "Crosshair", "+",
                anchoredPos: Vector2.zero, size: new Vector2(40, 40), fontSize: 40);

            // Health label (top-left)
            TextMeshProUGUI healthLabel = CreateUIText(canvasGO.transform, "HealthLabel", "100/100",
                anchoredPos: new Vector2(30, -30), size: new Vector2(300, 60),
                fontSize: 36, anchor: new Vector2(0, 1));

            // Game Over panel (starts enabled; GameOverUI.Awake disables it)
            GameObject gameOverPanel = CreateFullscreenPanel(canvasGO.transform, "GameOverPanel", "GAME OVER");
            Button gameOverRestart = CreateUIButton(gameOverPanel.transform, "RestartButton", "Restart",
                anchoredPos: new Vector2(0, -40));
            Button gameOverQuit = CreateUIButton(gameOverPanel.transform, "QuitToMenuButton", "Quit to Menu",
                anchoredPos: new Vector2(0, -130));
            GameOverUI gameOverUI = gameOverPanel.AddComponent<GameOverUI>();
            UnityEventTools.AddPersistentListener(gameOverRestart.onClick, gameOverUI.OnRestartPressed);
            UnityEventTools.AddPersistentListener(gameOverQuit.onClick, gameOverUI.OnQuitToMenuPressed);

            // Pause panel (PauseMenu component lives on the Canvas so Update always runs)
            GameObject pausePanel = CreateFullscreenPanel(canvasGO.transform, "PausePanel", "PAUSED");
            Button pauseResume = CreateUIButton(pausePanel.transform, "ResumeButton", "Resume",
                anchoredPos: new Vector2(0, -40));
            Button pauseQuit = CreateUIButton(pausePanel.transform, "PauseQuitButton", "Quit to Menu",
                anchoredPos: new Vector2(0, -130));
            pausePanel.SetActive(false); // pre-hide; PauseMenu.Awake also hides it
            PauseMenu pauseMenu = canvasGO.AddComponent<PauseMenu>();
            SetFieldReference(pauseMenu, "panelRoot", pausePanel);
            UnityEventTools.AddPersistentListener(pauseResume.onClick, pauseMenu.OnResumePressed);
            UnityEventTools.AddPersistentListener(pauseQuit.onClick, pauseMenu.OnQuitToMenuPressed);

            // HealthUI
            HealthUI healthUI = canvasGO.AddComponent<HealthUI>();
            SetFieldReference(healthUI, "playerHealth", playerHealth);
            SetFieldReference(healthUI, "healthLabel", healthLabel);

            // Hook Game Over to player
            SetFieldReference(playerHealth, "gameOverUI", gameOverUI);
        }

        // ---------------------------------------------------------------------
        // ENEMIES / INTERACTION
        // ---------------------------------------------------------------------

        private static void CreateEnemy(Vector3 position)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Enemy";
            enemy.transform.position = position;

            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.freezeRotation = true;

            enemy.AddComponent<EnemyHealth>();
            enemy.AddComponent<EnemyAI>();

            ApplyColor(enemy, new Color(0.9f, 0.2f, 0.2f));
        }

        private static void CreateHazardZone(Vector3 position)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = "HazardZone";
            hazard.transform.position = position;
            hazard.transform.localScale = new Vector3(3, 0.2f, 3);
            hazard.GetComponent<Collider>().isTrigger = true;
            hazard.AddComponent<HazardZone>();
            ApplyColor(hazard, new Color(1f, 0.4f, 0f));
        }

        private static void CreatePickup(Vector3 position, Pickup.PickupType type, string keyId)
        {
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pickup.name = "Pickup_" + keyId;
            pickup.transform.position = position;
            pickup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            pickup.GetComponent<Collider>().isTrigger = true;
            Pickup p = pickup.AddComponent<Pickup>();
            SetFieldEnum(p, "type", (int)type);
            SetFieldString(p, "keyId", keyId);
            ApplyColor(pickup, Color.yellow);
        }

        private static void CreateLockedDoor(Vector3 position, string keyId)
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "LockedDoor";
            door.transform.position = position;
            door.transform.localScale = new Vector3(3, 3, 0.3f);
            LockedDoor ld = door.AddComponent<LockedDoor>();
            SetFieldString(ld, "keyId", keyId);
            ApplyColor(door, new Color(0.5f, 0.3f, 0.1f));
        }

        private static void CreateLevelGoal(Vector3 position, string requireGoalId)
        {
            GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "LevelGoal_Exit";
            goal.transform.position = position;
            goal.transform.localScale = new Vector3(2, 3, 2);
            goal.GetComponent<Collider>().isTrigger = true;

            LevelGoal lg = goal.AddComponent<LevelGoal>();
            SetFieldBool(lg, "requireGoalCompleted", true);
            SetFieldString(lg, "goalId", requireGoalId);

            ApplyColor(goal, new Color(0.1f, 0.9f, 0.3f));
        }

        private static void SetGoalTrackerClearLevelGoal(GoalTracker tracker, string goalId)
        {
            SerializedObject so = new SerializedObject(tracker);
            SerializedProperty goals = so.FindProperty("goals");
            goals.arraySize = 1;
            SerializedProperty g = goals.GetArrayElementAtIndex(0);
            g.FindPropertyRelative("id").stringValue = goalId;
            g.FindPropertyRelative("type").enumValueIndex = (int)GoalTracker.GoalType.KillAllEnemies;
            g.FindPropertyRelative("requiredCount").intValue = 1;
            so.ApplyModifiedProperties();
        }

        // ---------------------------------------------------------------------
        // UI HELPERS
        // ---------------------------------------------------------------------

        private static void CreateMainCamera(Vector3 position, bool dark)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = position;
            Camera cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            if (dark)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            }
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasGO = new GameObject(name);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            return canvasGO;
        }

        private static TextMeshProUGUI CreateUIText(
            Transform parent, string name, string text,
            Vector2 anchoredPos, Vector2 size, float fontSize,
            Vector2? anchor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform rt = tmp.rectTransform;
            Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
            rt.anchorMin = a;
            rt.anchorMax = a;
            rt.pivot = a;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return tmp;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);

            Button btn = btnGO.AddComponent<Button>();

            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(260, 70);

            GameObject labelGO = new GameObject("Text");
            labelGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            RectTransform lrt = tmp.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            return btn;
        }

        private static GameObject CreateFullscreenPanel(Transform parent, string name, string title)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            Image img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.85f);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CreateUIText(panel.transform, "Title", title,
                anchoredPos: new Vector2(0, 150), size: new Vector2(900, 140), fontSize: 80);

            return panel;
        }

        // ---------------------------------------------------------------------
        // RENDERING HELPERS
        // ---------------------------------------------------------------------

        private static void ApplyColor(GameObject go, Color color)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
            mat.color = color;
            rend.sharedMaterial = mat;
        }

        // ---------------------------------------------------------------------
        // SERIALIZED-FIELD SETTERS (reach into [SerializeField] privates)
        // ---------------------------------------------------------------------

        private static void SetFieldReference(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] {target.name}.{field} not found"); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetFieldString(Object target, string field, string value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] {target.name}.{field} not found"); return; }
            p.stringValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetFieldBool(Object target, string field, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] {target.name}.{field} not found"); return; }
            p.boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetFieldEnum(Object target, string field, int enumIndex)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] {target.name}.{field} not found"); return; }
            p.enumValueIndex = enumIndex;
            so.ApplyModifiedProperties();
        }

        // ---------------------------------------------------------------------
        // BUILD SETTINGS + TAGS
        // ---------------------------------------------------------------------

        private static void SetupBuildSettings()
        {
            List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{ScenesFolder}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Level1.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Level2.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Level3.unity", true),
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        private static void AddTagIfMissing(string tag)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (tagsProp == null) return;

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
#endif
