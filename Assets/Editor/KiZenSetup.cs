#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Crea la escena principal con el GameObject "Game" (KiZenGame) y la registra en Build Settings.
// Se ejecuta solo al abrir el proyecto si la escena no existe, o desde el menú Ki Zen > Setup Scene.
[InitializeOnLoad]
public static class KiZenSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    static KiZenSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!System.IO.File.Exists(ScenePath)) SetupScene();
        };
    }

    [MenuItem("Ki Zen/Setup Scene")]
    public static void SetupScene()
    {
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }

        new GameObject("Game").AddComponent<KiZenGame>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("[Ki Zen] Escena creada en " + ScenePath + ". Presiona Play.");
    }

    // Usado en modo batch: Unity.exe -batchmode -executeMethod KiZenSetup.BatchSetup -quit
    public static void BatchSetup() => SetupScene();

    // Piloto automático para probar el juego sin teclado (útil al verificar vía MCP).
    [MenuItem("Ki Zen/AutoPilot On")]
    public static void AutoPilotOn() { EditorPrefs.SetBool("KiZen.AutoPilot", true); Debug.Log("[Ki Zen] AutoPilot ON"); }

    [MenuItem("Ki Zen/AutoPilot Off")]
    public static void AutoPilotOff() { EditorPrefs.SetBool("KiZen.AutoPilot", false); Debug.Log("[Ki Zen] AutoPilot OFF"); }
}
#endif
