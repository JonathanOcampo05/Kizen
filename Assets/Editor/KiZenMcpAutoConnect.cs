#if UNITY_EDITOR
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

// Conecta automáticamente el bridge de MCP for Unity (HTTP local, puerto 8080)
// cada vez que el Editor carga o recompila, para que el agente pueda trabajar sin clics.
[InitializeOnLoad]
public static class KiZenMcpAutoConnect
{
    static int attempts;

    static KiZenMcpAutoConnect()
    {
        if (Application.isBatchMode) return;
        EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
        EditorApplication.update += Tick;
    }

    static async void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= Tick;
        try
        {
            if (MCPServiceLocator.Bridge.IsRunning) return;
            bool ok = await MCPServiceLocator.Bridge.StartAsync();
            Debug.Log(ok ? "[Ki Zen] MCP bridge conectado" : "[Ki Zen] MCP bridge no pudo conectar (¿servidor en :8080?)");
            if (!ok && ++attempts < 5) EditorApplication.update += Tick;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Ki Zen] MCP auto-connect: " + e.Message);
        }
    }
}
#endif
