#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class SceneCreator
{
    private static string scenesPath = "Assets/Scenes";

    [MenuItem("Dino Monsters/Create All Scenes")]
    public static void CreateAllScenes()
    {
        // Ensure Scenes folder exists
        if (!AssetDatabase.IsValidFolder(scenesPath))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        CreateTitleScene();
        CreateStarterSelectScene();
        CreateOverworldScene();
        CreateBattleScene();
        SetupBuildSettings();

        Debug.Log("[SceneCreator] All 4 scenes created and added to Build Settings!");
        EditorUtility.DisplayDialog("Dino Monsters", "4 scenes created!\n\n- Title\n- StarterSelect\n- Overworld\n- Battle\n\nOpen Title scene to start playing.", "OK");
    }

    static void CreateTitleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.06f, 0.15f);
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";

        // Title UI
        var uiGO = new GameObject("TitleScreen");
        uiGO.AddComponent<TitleScreenUI>();

        // Light (minimal)
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.5f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        string path = Path.Combine(scenesPath, "Title.unity");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneCreator] Title scene created");
    }

    static void CreateStarterSelectScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.08f, 0.18f);
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";

        // Starter Select UI
        var uiGO = new GameObject("StarterSelect");
        uiGO.AddComponent<StarterSelectUI>();

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.5f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        string path = Path.Combine(scenesPath, "StarterSelect.unity");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneCreator] StarterSelect scene created");
    }

    static void CreateOverworldScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Overworld Manager
        var managerGO = new GameObject("OverworldManager");
        var manager = managerGO.AddComponent<OverworldManager>();

        // Player
        var playerGO = new GameObject("Player");
        var playerCtrl = playerGO.AddComponent<PlayerController>();
        playerGO.transform.position = new Vector3(10, 0, 10);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.4f, 0.6f, 0.9f); // Sky blue
        cam.fieldOfView = 45f;
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";
        var camCtrl = camGO.AddComponent<OverworldCamera>();
        camCtrl.target = playerGO.transform;

        // Wire references
        manager.player = playerCtrl;
        manager.mainCamera = cam;

        // Directional Light (sun)
        var lightGO = new GameObject("Sun");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.96f, 0.84f); // Warm sunlight
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Ambient light
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.50f, 0.55f);

        string path = Path.Combine(scenesPath, "Overworld.unity");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneCreator] Overworld scene created");
    }

    static void CreateBattleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Battle Manager
        var managerGO = new GameObject("BattleManager");
        managerGO.AddComponent<BattleSceneManager>();

        // Camera (will be repositioned by BattleSceneManager)
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.35f, 0.55f, 0.35f); // Battle green
        cam.fieldOfView = 40f;
        camGO.transform.position = new Vector3(0, 5, -6);
        camGO.transform.rotation = Quaternion.Euler(35, 0, 0);
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";

        // Battle Light
        var lightGO = new GameObject("Battle Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.color = new Color(1f, 0.98f, 0.90f);
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(45, -20, 0);

        // Fill light (softer, from opposite side)
        var fillGO = new GameObject("Fill Light");
        var fill = fillGO.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.4f;
        fill.color = new Color(0.7f, 0.8f, 1f); // Cool blue
        fill.shadows = LightShadows.None;
        fillGO.transform.rotation = Quaternion.Euler(30, 160, 0);

        string path = Path.Combine(scenesPath, "Battle.unity");
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneCreator] Battle scene created");
    }

    static void SetupBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/Title.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/StarterSelect.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Overworld.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Battle.unity", true),
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[SceneCreator] Build Settings updated with 4 scenes");
    }
}
#endif
